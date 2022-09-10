IF OBJECT_ID('dbo.__Libster_Migrations__') IS NULL BEGIN
    CREATE TABLE dbo.__Libster_Migrations__ (
        SourceId NVARCHAR(256) NOT NULL,
        Version BIGINT NOT NULL,
        DateAppliedUtc DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        Name NVARCHAR(256) NULL,
        ScriptContent NVARCHAR(MAX) NOT NULL,
        Description NVARCHAR(256) NULL,
        CONSTRAINT PK_Libster_Migrations PRIMARY KEY CLUSTERED (SourceId, Version)
        )
END