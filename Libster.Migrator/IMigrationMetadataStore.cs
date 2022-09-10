using System.Data;

namespace Libster.Migrator;

public interface IMigrationMetadataStore
{
    public void Initialize();
    long? GetCurrentInstalledVersion(string identifier);
        
    /// <summary>
    /// Implementors should set the command text and parameters so that metadata information is removed that is greater than the given version information.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="identifier"></param>
    /// <param name="targetVersion"></param>
    void PrepareRemoveVersionsGreaterThanCommand(IDbCommand cmd, string identifier, long? targetVersion);

    /// <summary>
    /// Implementors should set the command text and parameters so that metadata information is stored for the given script.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="identifier"></param>
    /// <param name="script"></param>
    void PrepareScriptVersionInstalledCommand(IDbCommand cmd, string identifier, SqlScript script);
}