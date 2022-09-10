using System;
using System.IO;
using System.Linq;

namespace Libster.Migrator;

public class SqlScript
{
    public long Version { get; set; }
            
    public string Description { get; set; }
            
    public MigrationType MigrationType { get; set; }
    
    /// <summary>
    /// The actual script to run.
    /// </summary>
    public string ScriptContent { get; set; }
    
    public string ScriptName { get; set; }

    // filename format: "{versionnumber}_{up|down}_{description?}.sql" 
    internal static bool TryParseFromFileName(string fileName, out SqlScript sqlScript)
    {
        sqlScript = null;
        var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
                
        // at least version and migrationtype are required.
        if (parts.Length < 2)
        {
            return false;
        }

        // first part must be version
        if (!long.TryParse(parts[0], out long version))
        {
            return false;
        }
                
        // second part must be migrationtype
        if (!Enum.TryParse(parts[1], ignoreCase: true, out MigrationType migrationType))
        {
            return false;
        }

        if (migrationType == MigrationType.Unknown)
        {
            return false;
        }

        sqlScript = new SqlScript()
        {
            Version = version,
            MigrationType = migrationType
        };

        // all other parts are interpreted as description
        if (parts.Length > 2)
        {

            sqlScript.Description = string.Join('_', parts.Skip(2));
        }
                
        return true;
    }
}