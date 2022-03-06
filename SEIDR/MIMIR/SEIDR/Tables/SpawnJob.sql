CREATE TABLE [SEIDR].[SpawnJob] (
    [SpawnJobID]       INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID] INT           NOT NULL,
    [JobProfileID]     INT           NOT NULL,
    [SourceFile]       VARCHAR (500) NULL,
    [DD]               DATETIME      NULL,
    [Active]           AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    CONSTRAINT [PK_SpawnJob] PRIMARY KEY CLUSTERED ([SpawnJobID] ASC),
    CONSTRAINT [FK_SpawnJob_JobProfile] FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID]),
    CONSTRAINT [FK_SpawnJob_JobProfile_Job] FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);



GO

CREATE NONCLUSTERED INDEX [IDX_SpawnJob_JobProfile_JobID]
    ON [SEIDR].[SpawnJob]([JobProfile_JobID] ASC) WHERE ([DD] IS NULL);

GO
