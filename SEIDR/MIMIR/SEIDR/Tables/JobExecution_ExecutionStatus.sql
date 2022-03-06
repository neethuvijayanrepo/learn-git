CREATE TABLE [SEIDR].[JobExecution_ExecutionStatus] (
    [JobExecution_ExecutionStatusID] BIGINT        IDENTITY (1, 1) NOT NULL,
    [JobExecutionID]                 BIGINT        NOT NULL,
    [JobProfile_JobID]               INT           NULL,
    [StepNumber]                     SMALLINT      NOT NULL,
    [ExecutionStatusCode]            VARCHAR (2)   NOT NULL,
    [ExecutionStatusNameSpace]       VARCHAR (128) NOT NULL,
    [ExecutionStatus]                AS            (([ExecutionStatusNameSpace]+'.')+[ExecutionStatusCode]) PERSISTED NOT NULL,
	[Branch]						 VARCHAR(30)   NOT NULL DEFAULT('MAIN'),
	[PreviousBranch]					VARCHAR(30) NULL,
    [ProcessingDate]                 DATE          NOT NULL,
    [FilePath]                       VARCHAR (250) NULL,
    [FileSize]                       BIGINT        NULL,
    [Success]                        BIT           DEFAULT ((1)) NOT NULL,
    [RetryCount]                     SMALLINT      NOT NULL,
    [DC]                             DATETIME      DEFAULT (getdate()) NOT NULL,
    [FileHash]                       VARCHAR (88)  NULL,
    [ExecutionTimeSeconds]           INT           NULL,
    [IsLatestForExecutionStep]       BIT           DEFAULT ((1)) NOT NULL,
    [METRIX_LoadBatchID] INT NULL, 
	[METRIX_ExportBatchID] INT NULL,
    PRIMARY KEY CLUSTERED ([JobExecution_ExecutionStatusID] ASC),
    FOREIGN KEY ([JobExecutionID]) REFERENCES [SEIDR].[JobExecution] ([JobExecutionID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
    FOREIGN KEY ([ExecutionStatusNameSpace], [ExecutionStatusCode]) REFERENCES [SEIDR].[ExecutionStatus] ([NameSpace], [ExecutionStatusCode])
);










GO
CREATE NONCLUSTERED INDEX [idx_JobProfile_JobID_Success_IsLatest]
    ON [SEIDR].[JobExecution_ExecutionStatus]([JobProfile_JobID] ASC, [Success] ASC, [IsLatestForExecutionStep] ASC)
    INCLUDE([JobExecutionID], [ProcessingDate]);


GO
CREATE NONCLUSTERED INDEX [ix_JobExecution_ExecutionStatus_JobExecutionID_StepNumber_IsLatestForExecutionStep_includes]
    ON [SEIDR].[JobExecution_ExecutionStatus]([JobExecutionID] ASC, [StepNumber] ASC, [IsLatestForExecutionStep] ASC)
    INCLUDE([JobProfile_JobID], [FilePath]) WITH (FILLFACTOR = 100);


GO
CREATE NONCLUSTERED INDEX [IX_JobExecutionID_StepNumber_Success_IsLatestForExecutionStep_Includes]
    ON [SEIDR].[JobExecution_ExecutionStatus]([JobExecutionID] ASC, [StepNumber] ASC, [Success] ASC, [IsLatestForExecutionStep] ASC)
    INCLUDE([JobProfile_JobID], [FilePath]);

