CREATE TABLE [SEIDR].[JobProfile_Job] (
    [JobProfile_JobID]           INT           IDENTITY (1, 1) NOT NULL,
    [JobProfileID]               INT           NOT NULL,
    [StepNumber]                 SMALLINT      NOT NULL,
	[Branch] varchar(30) NOT null DEFAULT 'MAIN', --Once triggered, JobExecution uses this branch.
    [Description]                VARCHAR (100) NOT NULL,
    [JobID]                      INT           NOT NULL,
    [TriggerExecutionStatusCode] VARCHAR (2)   NULL,
    [TriggerExecutionNameSpace]  VARCHAR (128) NULL,
	[TriggerBranch] varchar(30) null, -- JobExecution branch at time of evaluation. Previous branch if using 
	--PreviousJobProfile_JobID	 INT	       NULL FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),
    [CanRetry]                   BIT           DEFAULT ((1)) NOT NULL,
    [RequiredThreadID]           INT           NULL,
    [FailureNotificationMail]    VARCHAR (500) NULL,
    [RetryDelay]                 SMALLINT      NULL,
    [SequenceScheduleID]         INT           NULL,
	[RetryCountBeforeFailureNotification] smallint null,
    [DD]                         DATETIME      NULL,
    [Active]                     AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    [RetryLimit]                 SMALLINT      DEFAULT ((10)) NOT NULL,
    [DateCreated]                SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [LastUpdate]                 SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [CreatedBy]                  VARCHAR (128) NOT NULL,
    [UpdatedBy]                  VARCHAR (128) NULL,
    [DeletedBy]                  VARCHAR (128) NULL,
    PRIMARY KEY CLUSTERED ([JobProfile_JobID] ASC),
    CHECK ([RetryDelay] IS NULL OR [RetryDelay]>=(5)),
    CHECK ([StepNumber]>(0)),
    FOREIGN KEY ([JobID]) REFERENCES [SEIDR].[Job] ([JobID]),
    FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID]),
    FOREIGN KEY ([SequenceScheduleID]) REFERENCES [SEIDR].[Schedule] ([ScheduleID]),
    FOREIGN KEY ([TriggerExecutionNameSpace], [TriggerExecutionStatusCode]) REFERENCES [SEIDR].[ExecutionStatus] ([NameSpace], [ExecutionStatusCode])
);



GO 
CREATE UNIQUE INDEX UQ_JobProfile_Job_Step
ON SEIDR.JobProfile_Job
([JobProfileID] ASC, [StepNumber] ASC, 
	Branch ASC, [TriggerBranch] DESC, --Allow grouping by branch, and converging previous branches.E.g., check if a file sometimes has SELFPAY in the name, do a couple steps differently, then converge back to the main branch
	[TriggerExecutionStatusCode] DESC, [TriggerExecutionNameSpace] DESC)
WHERE ([DD] IS NULL)
GO
CREATE STATISTICS [st_JobProfile_Job_JobID]
    ON [SEIDR].[JobProfile_Job]([JobID]);


GO
CREATE TRIGGER [SEIDR].[trg_JobProfile_Job_u]
ON [SEIDR].[JobProfile_Job] after UPDATE,DELETE
AS
BEGIN	
	SET XACT_ABORT ON
	SET NOCOUNT ON

	--Potential - don't worry about this? Just let the existing job executions process?
	IF (UPDATE(TriggerExecutionStatusCode) OR UPDATE(TriggerExecutionNameSpace) OR UPDATE(StepNumber) OR UPDATE(DD) OR UPDATE([TriggerBranch]))
	AND EXISTS
	(
		SELECT NULL
		FROM [SEIDR].[JobExecution] JE WITH (NOLOCK) 
		JOIN SEIDR.ExecutionStatus s  WITH (NOLOCK) 
			ON je.ExecutionStatusNameSpace = s.[NameSpace]  
			AND je.ExecutionStatusCode = s.ExecutionStatusCode  
			AND s.IsComplete = 0
			AND je.Active = 1
		WHERE je.JobProfile_JobID IN (select JobProfile_JobID from Deleted) --Curretly pointing to
		OR EXISTS(SELECT null 
					FROM INSERTED 
					WHERE JobProfileID = je.JobProfileID
					AND StepNumber = je.StepNumber
					AND Active = 1) -- Can START pointing to.
	)
	BEGIN
			
		UPDATE JE
		SET JobProfile_JobID =  jpj.JobProfile_JobID,
			Branch = COALESCE(jpj.Branch, je.Branch)
		FROM [SEIDR].[JobExecution] JE
		JOIN SEIDR.ExecutionStatus s  
			ON je.ExecutionStatusNameSpace = s.[NameSpace]  
			AND je.ExecutionStatusCode = s.ExecutionStatusCode  
			AND s.IsComplete = 0
			AND je.Active = 1			
		LEFT JOIN SEIDR.JobProfile_Job jpj
			ON jpj.JobProfile_JobID = SEIDR.ufn_GetJobProfile_JobID(je.JobProfileID, je.StepNumber, je.ExecutionStatusCode, je.ExecutionStatusNameSpace, je.PreviousBranch)
		WHERE je.JobProfile_JobID IN (select JobProfile_JobID from Deleted) --Curretly pointing to
		OR EXISTS(SELECT null 
					FROM INSERTED 
					WHERE JobProfileID = je.JobProfileID
					AND StepNumber = je.StepNumber
					AND Active = 1) -- Can START pointing to.
		 
	END
END
GO

CREATE INDEX idx_JobProfile_Job_Execution_Map 
	ON SEIDR.JobProfile_Job(JobProfileID, StepNumber, TriggerBranch, TriggerExecutionStatusCode, TriggerExecutionNameSpace)
	WHERE DD IS NULL
GO
CREATE STATISTICS [st_JobProfile_Job_TriggerCode]
    ON [SEIDR].[JobProfile_Job]([TriggerExecutionStatusCode], [TriggerExecutionNameSpace], [StepNumber]);


GO
CREATE STATISTICS [st_JobProfile_Job_Trigger]
    ON [SEIDR].[JobProfile_Job]([TriggerExecutionNameSpace], [TriggerExecutionStatusCode], [StepNumber]);


GO
CREATE STATISTICS [st_JobProfile_Job_Active]
    ON [SEIDR].[JobProfile_Job]([Active]);


GO
CREATE STATISTICS [st_JobProfile_Job_jobID_PK]
    ON [SEIDR].[JobProfile_Job]([JobID], [JobProfile_JobID]);

