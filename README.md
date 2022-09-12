About
=====

Libster.Migrator is a library that helps with database migrations.
This is not fully implemented and mostly for personal entertainment (read: more complex than needed, but I justify it by thinking about a certain problem and learning)


![Build status](https://github.com/bernhardkircher/migrator/actions/workflows/dotnet.yml/badge.svg)


Example
=======
```
var connection = new SqlConnection("Server=.\\SQLExpress;Database=Libster_Migrator_Tests;Trusted_Connection=True;");
var logger = new CustomConsoleLogger();

// you can use relative or absolute paths. when suing relative, make sure that the folders are ther (e.g. use copy to output dir in the solution/project) 
var scriptSource = new FolderScriptSource(logger, "migrations");
var metaDataStore = new MsSqlMigrationMetadataStore(logger, connection);

// the identifier is used if you have multiple scriptsources to identify the source of the scripts.
string applicationIdentifier = "example";
var migrationSource = new Migrator(logger, applicationIdentifier, scriptSource, metaDataStore, connection);

// migrate to latest version:
await migrationSource.Migrate();

Console.WriteLine("Installed schema... uninstalling now...");
// use the "down" scripts to revert back to version 0.
await migrationSource.Migrate(0);
```


Key concepts
===========
SqlScript: The definition of a script itself with additional metadata (version, ...). The main idea is to have a version (=long) which developers can just manually increment (e.g. 1, 2, 3, ...) or use a dateformat (e.g. yyyyMMddmmss). 
It is important that version is ordered - newer scripts need to have a higher version than an older one.

There are 2 types of scripts: up scripts (install newer versions, e.g. add new columsn, tables,) and down scripts (that are used to undo an "up" script.) Downscripts are optional but should always remove what the matching "up" script did.


IScriptSource: An interface, who is responsible to provide all scripts that are available and probably might need to be executed.
The idea is to separate the location of scripts from their execution (they can be on a filesystem, embedded ressources, a database or whatever).

IMigrationMetadataStore: AN interface that is responsible to store version information, so that the system knows which scripts (version) have been applied.

Migrator: the migrator itself, that uses the IScriptSource and IMigrationMetadataStore to actually apply the scripts.

Currently there is only a MS SQL implementation available.
For Scripts, the current naming convention that needs to be followed id:
"{versionnumber}_{up|down}_{descriptiontext?}.sql"


when using the sql scripts, make sure their build type is set correctly, so that they get picked/location is as expected.

TODO
====
Return more info about what has been migrated (e.g. new version etc)

currently uses only sync. interfaces, no support for async.
