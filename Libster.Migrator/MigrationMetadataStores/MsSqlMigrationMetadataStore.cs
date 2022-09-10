using System;
using System.Data;

namespace Libster.Migrator.MigrationMetadataStores;

public class MsSqlMigrationMetadataStore : IMigrationMetadataStore
{
    private readonly IDbConnection _connection;

    public MsSqlMigrationMetadataStore(IDbConnection connection)
    {
        _connection = connection;
    }

    public long? GetCurrentInstalledVersion(string identifier)
    {
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "SELECT MAX(Version) FROM dbo.__Libster_Migrations__ WHERE SourceId = @SourceId";
            SqlHelper.AddParameter(cmd, "@SourceId", identifier);
                
            var currentVersionValue = cmd.ExecuteScalar();
            if (currentVersionValue is DBNull)
            {
                return null;
            }
            return (long)currentVersionValue;
        }
    }

    public void Initialize()
    {
        // TODO it would be cool if this were migrations as well.
        // TODO this is very bad design: all methods take the IDbCommand, so we have transactional guarantees, but in here we work on the connection.
        // either make the interface completlely isolated form underlying infrastructure (e.g. metadata could be stored in files or whatever) or make this consoistent.
        using (var tx = _connection.BeginTransaction())
        {
            using(var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = tx;

                cmd.CommandText = @"
                    IF OBJECT_ID('dbo.__Libster_Migrations__') IS NULL BEGIN
                        CREATE TABLE dbo.__Libster_Migrations__ (
                            SourceId NVARCHAR(256) NOT NULL,
                            Version BIGINT NOT NULL,
                            DateAppliedUtc DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                            Name NVARCHAR(256) NULL,
                            ScriptContent NVARCHAR(MAX) NOT NULL,
                            Description NVARCHAR(256) NULL,
                            CONSTRAINT PK_Libster_Migrations PRIMARY KEY CLUSTERED (SourceId, Version)
                        )
                    END
                    ";
                cmd.ExecuteNonQuery();
            }
                
            tx.Commit();
        }
    }


    public void PrepareRemoveVersionsGreaterThanCommand(IDbCommand cmd, string identifier, long? targetVersion)
    {
        cmd.CommandText =
            "DELETE FROM dbo.__Libster_Migrations__ WHERE SourceId = @SourceId AND Version > @Version";
        SqlHelper.AddParameter(cmd, "@SourceId", identifier);
        SqlHelper.AddParameter(cmd, "@Version", targetVersion.Value);
    }

    public void PrepareScriptVersionInstalledCommand(IDbCommand cmd, string identifier, SqlScript script)
    {
        cmd.CommandText =
            "INSERT INTO dbo.__Libster_Migrations__(SourceId, Version, Description, Name, ScriptContent) VALUES (@SourceId, @Version, @Description, @Name, @ScriptContent)";
        SqlHelper.AddParameter(cmd, "@SourceId", identifier);
        SqlHelper.AddParameter(cmd, "@Version", script.Version);
        SqlHelper.AddParameter(cmd, "@Description", script.Description);
        SqlHelper.AddParameter(cmd, "@Name", script.ScriptName);
        SqlHelper.AddParameter(cmd, "@ScriptContent", script.ScriptContent);
    }
}