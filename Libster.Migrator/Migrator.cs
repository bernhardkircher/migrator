using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Libster.Migrator
{
    
    /// <summary>
    /// Executes database migrations.
    /// </summary>
    public class Migrator
    {
        private readonly string _identifier;
        private readonly IScriptSource _scriptSource;
        private readonly IMigrationMetadataStore _metadataStore;

        private readonly IDbConnection _connection;
        private readonly ILogger _logger;

        public Migrator(ILogger logger, string identifier, IScriptSource scriptSource, IMigrationMetadataStore metadataStore, IDbConnection connection)
        {
            _identifier = identifier;
            _scriptSource = scriptSource;
            _metadataStore = metadataStore;
            _connection = connection;
            _logger = logger;
        }

        /// <summary>
        /// Migrate the database to the desired target version.
        /// </summary>
        /// <param name="targetVersion">The desired version to up/downgrade to. If no value is provided, upgrades to the latest version of the scripts found.</param>
        /// <returns></returns>
        public Task Migrate(long? targetVersion = null)
        {
            _logger.LogInformation("Ensure that database connection is open...");
            SqlHelper.EnsureOpenConnection(_connection);
            _logger.LogInformation("Database connection is open.");

            _logger.LogInformation("Checking metadata schema.");
            _metadataStore.Initialize();
            _logger.LogInformation("Metadata schema installed.");

            // 1. get current installed version
            var currentInstalledVersion = _metadataStore.GetCurrentInstalledVersion(_identifier);
            _logger.LogInformation($"Current installed version for {_identifier} is {currentInstalledVersion}");
            
            
            var needsDownGrade = currentInstalledVersion.HasValue && targetVersion.HasValue &&
                                 currentInstalledVersion.Value > targetVersion.Value;
            
            var scriptsToExecute = GetScriptsToExecute(targetVersion, needsDownGrade, currentInstalledVersion);

            _logger.LogInformation(
                $"Is downgrade: {needsDownGrade}; targetVersion: {targetVersion}; Number of scripts to execute: {scriptsToExecute.Length}");

            // 3. run script and update metadata (installed version).
            ApplyMigrationScripts(targetVersion, scriptsToExecute, needsDownGrade);

            return Task.CompletedTask;
        }

        private void ApplyMigrationScripts(long? targetVersion, SqlScript[] scriptsToExecute, bool needsDownGrade)
        {
            using (var tx = _connection.BeginTransaction())
            {
                foreach (var script in scriptsToExecute)
                {
                    using (var cmd = _connection.CreateCommand())
                    {
                        try
                        {
                            cmd.Transaction = tx;

                            var scriptFileName = script.ScriptName;
                            _logger.LogInformation($"Executing script {scriptFileName}");

                            cmd.CommandText = script.ScriptContent;
                            cmd.ExecuteNonQuery();

                            if (needsDownGrade)
                            {
                                // remove also all version larger than current version - there might be no "down" script for some verions
                                _metadataStore.StoreScriptVersionSuccessfullyDowngraded(tx, _identifier, script.Version);
                            }
                            else
                            {
                                _metadataStore.StoreScriptVersionSuccessfullyMigrated(tx, _identifier, script);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Error while executing script");
                            throw;
                        }
                    }
                }

                // final cleanup in case of downgrade - if we downgrade to a version that is smaller than min installed script version (= e.g. zo "0")
                // remove all installed scripts that do not have a down script.
                if (needsDownGrade)
                {
                    // remove also all version larger than current version - there might be no "down" script for some verions
                    _metadataStore.StoreScriptVersionSuccessfullyDowngraded(tx, _identifier, targetVersion);
                }

                tx.Commit();
            }
        }

        private SqlScript[] GetScriptsToExecute(long? targetVersion, bool needsDownGrade, long? currentInstalledVersion)
        {
            var validFilesOrderedByVersion = _scriptSource.GetAllScripts()
                .OrderBy(x => x.Version)
                .ToArray();

            _logger.LogInformation($"Found {validFilesOrderedByVersion.Length} script files.");


            // 2. get all versions that are higher than current installed version
            var scriptsToExecute = validFilesOrderedByVersion
                .Where(x => x.MigrationType == (needsDownGrade ? MigrationType.Down : MigrationType.Up)).ToArray();
            if (needsDownGrade)
            {
                // when downgrading, we are downgrading from version 3, to 2, then to 1 (therefore order by desc).
                scriptsToExecute = scriptsToExecute.OrderByDescending(x => x.Version)
                    .Where(x => x.Version <= currentInstalledVersion.Value && x.Version > targetVersion).ToArray();
            }
            else
            {
                scriptsToExecute = scriptsToExecute.OrderBy(x => x.Version).Where(x =>
                    !currentInstalledVersion.HasValue || x.Version > currentInstalledVersion.Value).ToArray();
            }

            return scriptsToExecute;
        }
    }
}