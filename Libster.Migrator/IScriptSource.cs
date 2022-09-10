using System.Collections.Generic;
using System.Threading.Tasks;

namespace Libster.Migrator;

/// <summary>
/// An IScriptSource is an abstraction that handles the retrieval of the actual scripts to be executed.
/// </summary>
public interface IScriptSource
{

    /// <summary>
    /// Retrieves all scripts of the <see cref="IScriptSource"/>.
    /// This might be on the filesystem, embedded resources or something else.
    /// </summary>
    /// <returns></returns>
    IEnumerable<SqlScript> GetAllScripts();
}