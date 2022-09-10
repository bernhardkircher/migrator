// See https://aka.ms/new-console-template for more information


using System.Data.SqlClient;
using System.Threading.Channels;
using Libster.Migrator;
using Libster.Migrator.MigrationMetadataStores;
using Libster.Migrator.ScriptSources;
using Microsoft.Extensions.Logging;

try
{
    var connection = new SqlConnection("Server=.\\SQLExpress;Database=Libster_Migrator_Tests;Trusted_Connection=True;");
    var logger = new CustomConsoleLogger();
    var scriptSource = new FolderScriptSource(logger, "migrations");
    var metaDataStore = new MsSqlMigrationMetadataStore(logger, connection);
    var migrationSource = new Migrator(logger, "example", scriptSource, metaDataStore, connection);
    // migrate to latest version:
    await migrationSource.Migrate();

    Console.WriteLine("Installed schema... uninstalling now...");
    await migrationSource.Migrate(0);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

Console.WriteLine("Press any key to quit.");
Console.ReadLine();


class CustomConsoleLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"{logLevel}: {eventId} : {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}