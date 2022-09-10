using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Libster.Migrator.ScriptSources;

public class FolderScriptSource : IScriptSource
{
    private readonly ILogger _logger;
    private readonly string _pathToFolderWithSqlScripts;
        
    public FolderScriptSource(ILogger logger, string pathToFolderWithSqlScripts)
    {
        _logger = logger;
        _pathToFolderWithSqlScripts = pathToFolderWithSqlScripts;
    }

    public IEnumerable<SqlScript> GetAllScripts()
    {
        _logger.LogInformation($"Getting script files from {_pathToFolderWithSqlScripts}");
        var allFiles = Directory.GetFiles(_pathToFolderWithSqlScripts, "*.sql", SearchOption.TopDirectoryOnly);
        var validFilesOrderedByVersion = allFiles.Select(x =>
        {
            if (!SqlScript.TryParseFromFileName(x, out SqlScript script))
            {
                _logger.LogDebug($"Could not parse scriptfile {x}");
                return null;
            }

            script.ScriptContent = File.ReadAllText(Path.Combine(_pathToFolderWithSqlScripts, x));
            script.ScriptName = x;
            return script;
        }).Where(x => x != null);
            
        return validFilesOrderedByVersion;
    }
}