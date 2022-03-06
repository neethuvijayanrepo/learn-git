CREATE TABLE [SEIDR].[FileSystemJob] (
    [FileSystemJobId]     INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]    INT           NOT NULL,
    [Source]              VARCHAR (300) NULL,
    [OutputPath]          VARCHAR (300) NULL,
    [Filter]              VARCHAR (230) NULL,
    [Operation]           VARCHAR (30)  DEFAULT ('COPY') NOT NULL,
    [UpdateExecutionPath] BIT           DEFAULT ((1)) NOT NULL,
    [Overwrite]           BIT           DEFAULT ((0)) NOT NULL,
    [DD]                  SMALLDATETIME NULL,
    [Active]              AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    [LoadProfileID]       INT           NULL,
    [DatabaseLookUpID]    INT           NULL,
    PRIMARY KEY CLUSTERED ([FileSystemJobId] ASC),
    FOREIGN KEY ([DatabaseLookUpID]) REFERENCES [SEIDR].[DatabaseLookup] ([DatabaseLookupID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
    FOREIGN KEY ([Operation]) REFERENCES [SEIDR].[FileSystemOperation] ([FileSystemOperationCode])
);


GO
CREATE UNIQUE INDEX UQ_JobProfile_JobID
ON SEIDR.FileSystemJob(JobProfile_JobID)
WHERE DD IS NULL
GO
