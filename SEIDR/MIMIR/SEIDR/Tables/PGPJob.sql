	CREATE TABLE [SEIDR].[PGPJob] (
    [PGPJobID]         INT            IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID] INT            NOT NULL,
    [PGPOperationID]   TINYINT        NOT NULL,
    [SourcePath]       NVARCHAR (500) NULL,
    [OutputPath]       NVARCHAR (500) NULL,
    [PublicKeyFile]    NVARCHAR (500) NULL,
    [PrivateKeyFile]   NVARCHAR (500) NULL,
    [KeyIdentity]      NVARCHAR (500) NULL,
    [PassPhrase]       NVARCHAR (500) NULL,
    [Description]      VARCHAR (256)  NULL,
    [DD]               SMALLDATETIME  NULL,
    [Active]           AS             (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    CONSTRAINT [PK_PGPJob] PRIMARY KEY CLUSTERED ([PGPJobID] ASC) WITH (FILLFACTOR = 85),
    CONSTRAINT [FK_PGPJob_JobProfile_Job] FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
    CONSTRAINT [FK_PGPJob_PGPOperation] FOREIGN KEY ([PGPOperationID]) REFERENCES [SEIDR].[PGPOperation] ([PGPOperationID])
);







