using System;
using System.Data;
using System.IO;
using Libster.Migrator.ScriptSources;
using Microsoft.Extensions.Logging;

namespace Libster.Migrator.MigrationMetadataStores;

public class MsSqlMigrationMetadataStore : IMigrationMetadataStore
{
    private readonly ILogger _logger;
    private readonly IDbConnection _connection;

    public MsSqlMigrationMetadataStore(ILogger logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    public long? GetCurrentInstalledVersion(string identifier)
    {
        using (var cmd = _connection.CreateCommand())
        {
            // if we are installing the schema for the metadatastore, the tables do not exist yet, but will be installed with the first script
            cmd.CommandText = "IF OBJECT_ID('dbo.__Libster_Migrations__') IS NULL BEGIN SELECT CAST(NULL AS BIGINT) END " +
                              "ELSE BEGIN" +
                              " SELECT MAX(Version) FROM dbo.__Libster_Migrations__ WHERE SourceId = @SourceId" +
                              " END";
            SqlHelper.AddParameter(cmd, "@SourceId", identifier);
                
            var currentVersionValue = cmd.ExecuteScalar();
            if (currentVersionValue is DBNull)
            {
                return null;
            }
            return (long)currentVersionValue;
        }
    }

    private bool _isInstallingMetadataSchema = false;

    public void Initialize()
    {
        // if we are currnely in the process of installing the medata schema (using the migrator internally)
        // make sure to not install the metadata schema again and again in an endless loop.
        if (_isInstallingMetadataSchema)
        {
            return;
        }
        
        _isInstallingMetadataSchema = true;
        try {
            Migrator metadataMigrator = new Migrator(_logger,nameof(MsSqlMigrationMetadataStore),
                new EmbeddedResourceScriptSource(_logger, "Libster.Migrator.MigrationMetadataStores.MsSqlMigrationMetadataStoreMigrations", this.GetType().Assembly),
                this,
                _connection);

            metadataMigrator.Migrate();
        } 
        finally
        {
            _isInstallingMetadataSchema = false;
        }
    }


    public void StoreScriptVersionSuccessfullyDowngraded(IDbTransaction transaction, string identifier, long? targetVersion)
    {
        using (var cmd = _connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText =
                "DELETE FROM dbo.__Libster_Migrations__ WHERE SourceId = @SourceId AND Version > @Version";
            SqlHelper.AddParameter(cmd, "@SourceId", identifier);
            SqlHelper.AddParameter(cmd, "@Version", targetVersion.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public void StoreScriptVersionSuccessfullyMigrated(IDbTransaction transaction, string identifier, SqlScript script)
    {
        using (var cmd = _connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText =
                "INSERT INTO dbo.__Libster_Migrations__(SourceId, Version, Description, Name, ScriptContent) VALUES (@SourceId, @Version, @Description, @Name, @ScriptContent)";
            SqlHelper.AddParameter(cmd, "@SourceId", identifier);
            SqlHelper.AddParameter(cmd, "@Version", script.Version);
            SqlHelper.AddParameter(cmd, "@Description", script.Description);
            SqlHelper.AddParameter(cmd, "@Name", script.ScriptName);
            SqlHelper.AddParameter(cmd, "@ScriptContent", script.ScriptContent);
            cmd.ExecuteNonQuery();
        }
    }
}