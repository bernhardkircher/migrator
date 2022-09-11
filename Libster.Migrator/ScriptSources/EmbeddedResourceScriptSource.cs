using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Libster.Migrator.ScriptSources;

public class EmbeddedResourceScriptSource : IScriptSource
{
    private readonly ILogger _logger;
    private readonly string _embeddedResourceFolderPrefix;
    private readonly Assembly _assemblyWithEmbeddedResources;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="embeddedResourceFolderPrefix">Path to the embedded resources. E.g. if the folder containing the scripts is in an Assembly "Test.dll" nd the folder is "scripts" it would be "Test.scripts"</param>
    /// <param name="assemblyWithEmbeddedResources"></param>
    public EmbeddedResourceScriptSource(ILogger logger, string embeddedResourceFolderPrefix, Assembly assemblyWithEmbeddedResources)
    {
        _logger = logger;
        _embeddedResourceFolderPrefix = embeddedResourceFolderPrefix;
        _assemblyWithEmbeddedResources = assemblyWithEmbeddedResources;
    }

    public IEnumerable<SqlScript> GetAllScripts()
    {
        _logger.LogInformation($"Getting script files from {_embeddedResourceFolderPrefix} in {_assemblyWithEmbeddedResources.FullName}");
        var allResourcesInDefinedNamespace = _assemblyWithEmbeddedResources.GetManifestResourceNames()
            .Where(x=> x.ToLowerInvariant().StartsWith(_embeddedResourceFolderPrefix.ToLowerInvariant()));
        
        
        var validFilesOrderedByVersion = allResourcesInDefinedNamespace.Select(x =>
        {

            var nameWithoutAssemblyPath = x.Replace(_embeddedResourceFolderPrefix, "");
            if (nameWithoutAssemblyPath.StartsWith("."))
            {
                nameWithoutAssemblyPath = nameWithoutAssemblyPath.Remove(0, 1);
            }
            
            if (!SqlScript.TryParseFromFileName(nameWithoutAssemblyPath, out SqlScript script))
            {
                _logger.LogDebug($"Could not parse scriptfile {x}");
                return null;
            }
            
            using (var stream = _assemblyWithEmbeddedResources.GetManifestResourceStream(x))
            {
                using (var reader = new StreamReader(stream))
                {
                    script.ScriptContent = reader.ReadToEnd();
                }
            }

            script.ScriptName = x;
            return script;
        }).Where(x => x != null);
            
        return validFilesOrderedByVersion;
    }
}