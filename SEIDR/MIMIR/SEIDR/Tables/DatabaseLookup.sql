CREATE TABLE [SEIDR].[DatabaseLookup] (
    [DatabaseLookupID]  INT           IDENTITY (1, 1) NOT NULL,
    [Description]       VARCHAR (70)  NOT NULL,
    [ServerName]        VARCHAR (128) NOT NULL,
    [DatabaseName]      VARCHAR (128) NULL,
    [UserName]          VARCHAR (128) NULL,
    [EncryptedPassword] VARCHAR (256) NULL,
    [TrustedConnection] AS            (CONVERT([bit],case when [UserName] IS NULL OR [EncryptedPassword] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    [Provider] VARCHAR(60) NULL, 
    PRIMARY KEY CLUSTERED ([DatabaseLookupID] ASC),
    UNIQUE NONCLUSTERED ([Description] ASC)
);


