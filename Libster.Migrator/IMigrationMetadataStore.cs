using System.Data;

namespace Libster.Migrator;

/// <summary>
/// A <see cref="IMigrationMetadataStore"/> is responsible to store and retrieve information about applied migrations.
/// </summary>
public interface IMigrationMetadataStore
{
    public void Initialize();
    
    /// <summary>
    /// Implementors should return the currently installed version of the given identifier or null if no migration has yet been applied.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    long? GetCurrentInstalledVersion(string identifier);
        
    /// <summary>
    /// Implementors should set the command text and parameters so that metadata information is removed that is greater than the given version information.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="identifier"></param>
    /// <param name="targetVersion"></param>
    void StoreScriptVersionSuccessfullyDowngraded(IDbTransaction transaction, string identifier, long? targetVersion);

    /// <summary>
    /// Implementors should store the given script information after applying it in the given transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="identifier"></param>
    /// <param name="script"></param>
    void StoreScriptVersionSuccessfullyMigrated(IDbTransaction transaction, string identifier, SqlScript script);
}