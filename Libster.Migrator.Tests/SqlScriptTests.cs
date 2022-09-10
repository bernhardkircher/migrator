namespace Libster.Migrator.Tests;

public class SqlScriptTests
{
    [Fact]
    public void TryParseFromFileName_WhenFilenameDoesNotContainEnoughNameParts_ReturnsFalse()
    {
        var filenameWithoutEnoughParts = "notenough underlines in name";
        var canParse = SqlScript.TryParseFromFileName(filenameWithoutEnoughParts, out SqlScript script);
        
        Assert.False(canParse);
    }
    
    [Fact]
    public void TryParseFromFileName_WhenFilenameContainsInvalidVersion_ReturnsFalse()
    {
        var filenameWithoutValidVersion = "notaversionnumber_up_description.sql";
        var canParse = SqlScript.TryParseFromFileName(filenameWithoutValidVersion, out SqlScript script);
        
        Assert.False(canParse);
    }
    
    
    [Fact]
    public void TryParseFromFileName_WhenFilenameContainsInvalidUpOrDown_ReturnsFalse()
    {
        var filenameWithoutValidVersion = "1_notupordown_description.sql";
        var canParse = SqlScript.TryParseFromFileName(filenameWithoutValidVersion, out SqlScript script);
        
        Assert.False(canParse);
    }
    
    [Fact]
    public void TryParseFromFileName_WhenFilenameContainsMigrationTypeUnknown_ReturnsFalse()
    {
        var filenameWithoutValidVersion = "1_unknown_description.sql";
        var canParse = SqlScript.TryParseFromFileName(filenameWithoutValidVersion, out SqlScript script);
        
        Assert.False(canParse);
    }
    
    [Fact]
    public void TryParseFromFileName_WhenFilenameContainsVersionAndMigrationTypeAndDescription_ReturnsTrue()
    {
        var validFileName = "1_up_description.sql";
        var canParse = SqlScript.TryParseFromFileName(validFileName, out SqlScript script);
        
        Assert.True(canParse);
        Assert.Equal(1, script.Version);
        Assert.Equal(MigrationType.Up, script.MigrationType);
        Assert.Equal("description", script.Description);
    }
    
    [Fact]
    public void TryParseFromFileName_WhenFilenameContainsVersionAndMigrationTypeWithoutDescription_ReturnsTrue()
    {
        var validFileName = "1_down.sql";
        var canParse = SqlScript.TryParseFromFileName(validFileName, out SqlScript script);
        
        Assert.True(canParse);
        Assert.Equal(1, script.Version);
        Assert.Equal(MigrationType.Down, script.MigrationType);
        Assert.Null(script.Description);
    }
}