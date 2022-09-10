using System.Data;

namespace Libster.Migrator;

internal static class SqlHelper
{
        
    internal static void EnsureOpenConnection(IDbConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
    }
        
    internal static IDbDataParameter AddParameter(IDbCommand cmd, string name, object value)
    {
        var newParam = cmd.CreateParameter();
        newParam.Value = value;
        newParam.ParameterName = name;
        cmd.Parameters.Add(newParam);
        return newParam;
    }
}