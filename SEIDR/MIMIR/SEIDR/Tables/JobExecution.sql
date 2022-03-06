CREATE TABLE [SEIDR].[JobExecution] (
    [JobExecutionID]               BIGINT        IDENTITY (1, 1) NOT NULL,
    [JobProfileID]                 INT           NOT NULL,
    [UserKey]                      INT           NULL,
    [UserKey1]                     VARCHAR (50)  NOT NULL,
    [UserKey2]                     VARCHAR (50)  NULL,
    [StepNumber]                   SMALLINT      DEFAULT ((1)) NOT NULL,
    [ExecutionStatusCode]          VARCHAR (2)   NOT NULL,
    [ExecutionStatusNameSpace]     VARCHAR (128) DEFAULT ('SEIDR') NOT NULL,
    [ExecutionStatus]              AS            (([ExecutionStatusNameSpace]+'.')+[ExecutionStatusCode]) PERSISTED NOT NULL,
	Branch							varchar(30) NOT NULL DEFAULT('MAIN'),
	PreviousBranch					VARCHAR(30) NULL,
    [FilePath]                     VARCHAR (250) NULL,
    [FileSize]                     BIGINT        NULL,
    [ProcessingDate]               DATE          DEFAULT (getdate()) NOT NULL,
    [ForceSequence]                BIT           DEFAULT ((0)) NOT NULL,
    [RetryCount]                   SMALLINT      DEFAULT ((0)) NOT NULL,
    [LastExecutionStatusCode]      VARCHAR (2)   NULL,
    [LastExecutionStatusNameSpace] VARCHAR (128) NULL,
    [LastExecutionStatus]          AS            (([LastExecutionStatusNameSpace]+'.')+[LastExecutionStatusCode]) PERSISTED,
    [DC]                           DATETIME      DEFAULT (getdate()) NOT NULL,
    [LU]                           DATETIME      DEFAULT (getdate()) NOT NULL,
    [DD]                           DATETIME      NULL,
    [Active]                       AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)),
    [IsWorking]                    BIT           DEFAULT ((0)) NOT NULL,
    [FileHash]                     VARCHAR (88)  NULL,
    [JobPriority]                  VARCHAR (10)  DEFAULT ('NORMAL') NOT NULL,
    [InWorkQueue]                  BIT           DEFAULT ((0)) NOT NULL,
    [ProcessingTime]               TIME (7)      NULL,
    [ProcessingDateTime]           AS            (coalesce(CONVERT([datetime],[ProcessingDate])+CONVERT([datetime],[ProcessingTime]),[ProcessingDate])) PERSISTED NOT NULL,
    [ExecutionTimeSeconds]         INT           NULL,
    [TotalExecutionTimeSeconds]    INT           NULL,
    [ScheduleRuleClusterID]        INT           NULL,
    [PrioritizeNow]                BIT           DEFAULT ((0)) NOT NULL,
    [OrganizationID]               INT           NULL,
    [ProjectID]                    SMALLINT      NULL,
    [LoadProfileID]                INT           NULL,
    [JobProfile_JobID]             INT           NULL,
    [SpawningJobExecutionID]       BIGINT        NULL,
    [Duplicate]                    BIT           DEFAULT ((0)) NOT NULL,
    [Manual]                       BIT           DEFAULT ((0)) NOT NULL,
    [NotNeeded]                    BIT           DEFAULT ((0)) NOT NULL,
    [StopAfterStepNumber]          TINYINT       NULL,
    [METRIX_ExportBatchID]         INT           NULL,
    [METRIX_LoadBatchID] INT NULL, 
    PRIMARY KEY CLUSTERED ([JobExecutionID] ASC),
    CHECK ([StepNumber]>(0)),
    CONSTRAINT [CK_DD_Flag] CHECK ([DD] IS NULL AND (0)=((((0)+[Duplicate])+[Manual])+[NotNeeded]) OR [DD] IS NOT NULL AND (1)=((((0)+[Duplicate])+[Manual])+[NotNeeded])),
    FOREIGN KEY ([JobPriority]) REFERENCES [SEIDR].[Priority] ([PriorityCode]),
    FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID]),
    FOREIGN KEY ([ScheduleRuleClusterID]) REFERENCES [SEIDR].[ScheduleRuleCluster] ([ScheduleRuleClusterID]),
    FOREIGN KEY ([ExecutionStatusNameSpace], [ExecutionStatusCode]) REFERENCES [SEIDR].[ExecutionStatus] ([NameSpace], [ExecutionStatusCode]),
    FOREIGN KEY ([LastExecutionStatusNameSpace], [LastExecutionStatusCode]) REFERENCES [SEIDR].[ExecutionStatus] ([NameSpace], [ExecutionStatusCode]),
    CONSTRAINT [FK_JobExecution_JobExecution] FOREIGN KEY ([JobExecutionID]) REFERENCES [SEIDR].[JobExecution] ([JobExecutionID]),
    CONSTRAINT [FK_JobExecution_JobExecutionSpawn] FOREIGN KEY ([SpawningJobExecutionID]) REFERENCES [SEIDR].[JobExecution] ([JobExecutionID]),
    CONSTRAINT [FK_JobExecution_JobProfile_Job] FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);






























GO
CREATE UNIQUE NONCLUSTERED INDEX [uq_JobProfileID_FilePath]
    ON [SEIDR].[JobExecution]([JobProfileID] ASC, [FilePath] ASC) WHERE ([DD] IS NULL AND [FilePath] IS NOT NULL);



GO
CREATE TRIGGER [SEIDR].[trg_JobExecution_iu]
ON [SEIDR].[JobExecution] after INSERT, UPDATE
AS
BEGIN	
	SET NOCOUNT ON;

	DECLARE @Insert bit = 1
	IF EXISTS(SELECT null FROM DELETED)
		SET @Insert = 0
	
	IF NOT UPDATE(PreviousBranch)
	AND @Insert = 0
	AND (UPDATE(Branch) OR UPDATE(StepNumber))
	BEGIN
		UPDATE je
		SET PreviousBranch = d.Branch
		FROM SEIDR.JobExecution je
		JOIN DELETED d
			ON je.JobExecutionID = d.JobExecutionID
		WHERE je.StepNumber <> d.StepNumber 
		OR je.Branch <> d.Branch
	END

	IF UPDATE(ExecutionStatusCode) OR UPDATE(ExecutionStatusNameSpace) 
	OR UPDATE(FilePath)  OR UPDATE(StepNumber) OR UPDATE(DD) OR UPDATE(Branch)
	BEGIN		
		UPDATE je
		SET JobProfile_JobID = jpj.JobProfile_JobID,
			Branch = COALESCE(jpj.Branch, je.Branch) --Left join
		FROM SEIDR.JobExecution je
		JOIN INSERTED i 
			ON je.JobExecutionID = i.JobExecutionID
		LEFT JOIN DELETED d
			ON je.JobExecutionID = d.JobExecutionID
		LEFT JOIN SEIDR.JobProfile_Job jpj
			ON jpj.JobProfile_JobID = SEIDR.ufn_GetJobProfile_JobID(je.JobProfileID, je.StepNumber, je.ExecutionStatusCode, je.ExecutionStatusNameSpace, je.Branch) 
			--Note: at time of match up, we want to use the JobExecution's branch prior to link. Previous Branch is if we need to re-evaluate after the trigger logic has run for whatever reason. (e.g., JobProfile_Job trigger)
		WHERE je.Active = 1
		AND 
		(
			d.JobExecutionID IS NULL
			OR d.Active = 0
			OR CONCAT(je.ExecutionStatus, je.StepNumber) <> CONCAT(d.ExecutionStatus, d.StepNumber)
		)
		
		IF [SECURITY].[ufn_IsSystemUser]() = 0 --Centralize SUSER_NAME check for caller.		
		BEGIN
			INSERT INTO SEIDR.JobExecution_Note(JobExecutionID, StepNumber, JobProfile_JobID, NoteText, UserName, Technical)
			SELECT je.JobExecutionID, je.StepNumber, je.JobProfile_JobID,
				''
				+ CASE WHEN je.FilePath is null and d.FilePath is not null then 'Set FilePath to null from "' + d.FilePath + '"' + char(13) + CHAR(10) 
					WHEN je.FilePath is not null and d.FilePath is null then 'Set FilePath to "' + je.FilePath + '" from NULL.' + CHAR(13) + CHAR(10)
					WHEN je.FilePath <> d.FilePath then 'Set FilePath to "' + je.FilePath + '" from "' + d.FilePath + '"' + CHAR(13) + CHAR(10)
					ELSE '' END				
				+ CASE WHEN je.StepNumber <> d.StepNumber then 'Modified Step Number from ' + CONVERT(varchar(30), d.StepNumber) + CHAR(13) + CHAR(10) else '' end
				+ CASE WHEN je.Branch <> d.Branch THEN 'Modified Branch from "' + d.Branch + '" to "' + je.Branch + '"' + CHAR(13) + CHAR(10) else '' end
				+ CASE WHEN je.ExecutionStatus <> d.ExecutionStatus then 'Modified Status from ' + d.ExecutionStatus + ' to ' + je.ExecutionStatus + char(13) + CHAR(10) else '' end
				+ CASE WHEN ISNULL(je.JobProfile_JobID, -1)  <> ISNULL(d.JobProfile_JobID, -1) then 'Modified Job Link from ' + COALESCE('"' + ds.Description + '"', '(NULL)') + ' to '
					+ ISNULL('"' + jes.Description + '"', '(NULL)') + CHAR(13) + CHAR(10) else '' end
				+ CASE WHEN je.Active = 1 and d.Active = 0 then 'Reactivated JobExecution' 
					WHEN je.Active = 0 and d.Active = 1 then 'Deactivated JobExecution'
					else ''
					end,
				SUSER_NAME(), --UserName for note
				CAST(1 as bit) -- Technical - status detailing
			FROM SEIDR.JobExecution je
			JOIN DELETED d
				ON je.JobExecutionID = d.JobExecutionID
			LEFT JOIN SEIDR.JobProfile_Job jes WITH (NOLOCK)
				ON je.JobProfile_JobID = jes.JobProfile_JobID
			LEFT JOIN SEIDR.JobProfile_Job ds WITH (NOLOCK)
				ON d.JobProfile_JobID = ds.JobProfile_JobID
			WHERE ISNULL(je.FilePath, '') <> ISNULL(D.FilePath, '')
			OR je.StepNumber <> d.StepNumber
			OR je.ExecutionStatus <> d.ExecutionStatus
			OR ISNULL(je.JobProfile_JobID, -1) <> ISNULL(d.JobProfile_JobID, -1)
			OR je.Active <> d.Active
			OR Je.Branch <> d.Branch
			--Auto Notate status and JobProfile_JobID changes as a result of user updates/inserts
		END
	
		
		INSERT INTO SEIDR.JobExecution_ExecutionStatus(JobExecutionID, JobProfile_JobID, 
			StepNumber, ExecutionStatusCode, ExecutionStatusNameSpace, 
			ExecutionTimeSeconds, 
			[Branch], PreviousBranch,
			FilePath, FileSize, FileHash, RetryCount, 
			Success, 
			ProcessingDate,
			METRIX_LoadBatchID, METRIX_ExportBatchID)
		SELECT d.JobExecutionID, d.JobProfile_JobID, 
			d.StepNumber, d.ExecutionStatusCode, d.ExecutionStatusNameSpace, 
			i.ExecutionTimeSeconds, --Actually want to store the time that it took to get to the new status - the new executionTimeSeconds.
			d.[Branch], d.PreviousBranch,
			d.FilePath, d.FileSize, d.FileHash, d.RetryCount, 
			CASE WHEN i.StepNumber = d.StepNumber + 1 then CAST(1 as bit) else s.IsComplete end, --On success of job, either the StepNumber is incremented, or Complete = 1
			d.ProcessingDate,
			d.METRIX_LoadBatchID, d.METRIX_ExportBatchID
		FROM DELETED d
		JOIN INSERTED i
			ON d.JobExecutionID = i.JobExecutionID					
		JOIN SEIDR.ExecutionStatus s
			ON i.ExecutionStatusNameSpace = s.[NameSpace]
			AND i.ExecutionStatusCode = s.ExecutionStatusCode			
		WHERE NOT EXISTS( --Log to History if the JobExecution doesn't have an IsLatest for this step number pointing to d's FilePath/JobProfile_JobID
						SELECT null	
						FROM SEIDR.JobExecution_ExecutionStatus
						WHERE IsLatestForExecutionStep = 1 
						AND JobExecutionID = d.JobExecutionID
						AND StepNumber = d.StepNumber											
						AND ISNULL(FilePath,'') = ISNULL(d.FilePath, '')
						AND (JobProfile_JobID = d.JobProfile_JobID 
							or JobProfile_JobID is null AND d.JobProfile_JobID is null)
						AND Branch = d.Branch
						AND (PreviousBranch = d.PreviousBranch OR PreviousBranch is null and d.PreviousBranch is null)
						AND Success = CASE WHEN i.StepNumber = d.StepNumber + 1 then CAST(1 as bit) else s.IsComplete end
						)
						
								
		INSERT INTO SEIDR.JobExecution_ExecutionStatus(JobExecutionID, JobProfile_JobID, 			
			StepNumber, ExecutionStatusCode, ExecutionStatusNameSpace, ExecutionTimeSeconds,
			Branch, PreviousBranch,
			FilePath, FileSize, FileHash, RetryCount, Success, ProcessingDate, 
			METRIX_LoadBatchID, METRIX_ExportBatchID)
		SELECT i.JobExecutionID, i.JobProfile_JobID, 
			i.StepNumber, i.ExecutionStatusCode, i.ExecutionStatusNameSpace, i.ExecutionTimeSeconds,
			i.Branch, i.PreviousBranch,
			i.FilePath, i.FileSize, i.FileHash, i.RetryCount, 1, i.ProcessingDate, 
			i.METRIX_LoadBatchID, d.METRIX_ExportBatchID
		FROM INSERTED i
		JOIN SEIDR.ExecutionStatus s
			ON i.ExecutionStatusNameSpace = s.[NameSpace]
			AND i.ExecutionStatusCode = s.ExecutionStatusCode					
		JOIN DELETED d --Will be inserted with R or S, which are not Complete. R/S -> C will insert the original above
			ON d.JobExecutionID = i.JobExecutionID			
		WHERE s.IsComplete = 1 
		AND (d.ExecutionStatusCode <> i.ExecutionStatusCode 
			OR d.ExecutionStatusNameSpace <> i.ExecutionStatusNameSpace 
			or d.StepNumber <> i.StepNumber --Shouldn't really happen if the status is the same and Complete...Check anyway, though, since it affects the jpj join
			) 

		;WITH CTE AS(SELECT IsLatestForExecutionStep, 
							ROW_NUMBER() OVER (PARTITION BY JobExecutionID, StepNumber ORDER BY JobExecution_ExecutionStatusID DESC) rn
					FROM SEIDR.JobExecution_ExecutionStatus 
					WHERE JobExecutionID IN (SElECT JobExecutionID FROM INSERTED)
					AND IsLatestForExecutionStep = 1
		)
		UPDATE cte
		SET IsLatestForExecutionStep= 0
		WHERE rn > 1

	END	

	IF NOT UPDATE(LU)
		UPDATE je
		SET LU = GETDATE()
		FROM SEIDR.JobExecution je
		JOIN DELETED d
			ON je.JobExecutionID = d.JobExecutionID
		WHERE je.ExecutionStatus <> d.ExecutionStatus
		OR je.IsWorking <> d.IsWorking
		OR je.InWorkQueue <> d.InWorkQueue
		or je.StepNumber <> d.StepNumber
		or je.FilePath <> d.FilePath
		or je.ForceSequence <> d.ForceSequence		



	IF @Insert = 1 -- NOT EXISTS (SELECT NULL FROM DELETED)
	BEGIN		
		UPDATE je
		SET je.StopAfterStepNumber = jp.StopAfterStepNumber
		FROM SEIDR.JobExecution je 
		JOIN SEIDR.JobProfile jp 
			on je.JobProfileID = jp.JobProfileID
		JOIN INSERTED i 
			ON i.JobExecutionID = je.JobExecutionID
		WHERE i.StopAfterStepNumber IS NULL 
		AND jp.StopAfterStepNumber IS NOT NULL

		--Initialize Execution History to be available for viewing history. Not really useful to service, but could be useful for any changes that happen during the very first job call.
		INSERT INTO SEIDR.JobExecution_ExecutionStatus(JobExecutionID, 
			JobProfile_JobID, 			
			StepNumber, ExecutionStatusCode, ExecutionStatusNameSpace, ExecutionTimeSeconds,
			Branch, PreviousBranch,
			FilePath, FileSize, FileHash, RetryCount, Success, ProcessingDate, 
			METRIX_LoadBatchID, METRIX_ExportBatchID)
		SELECT i.JobExecutionID, 
			null as JobProfile_JobID, --Ensure that step completion will insert a new record with the time taken for executing this step.
			i.StepNumber, i.ExecutionStatusCode, i.ExecutionStatusNameSpace, i.ExecutionTimeSeconds,
			i.Branch, i.PreviousBranch,
			i.FilePath, i.FileSize, i.FileHash, i.RetryCount, 1, i.ProcessingDate, 
			i.METRIX_LoadBatchID, i.METRIX_ExportBatchID
		FROM JobExecution i
		WHERE JobExecutionID = ANY(SELECT JobExecutionID FROM INSERTED)		
	END

END

GO
CREATE NONCLUSTERED INDEX [idx_JobExecution_Active]
    ON [SEIDR].[JobExecution]([Active] ASC)
    INCLUDE([JobProfileID], [ExecutionStatusCode], [ExecutionStatusNameSpace], [ProcessingDate], [ForceSequence], [IsWorking], [InWorkQueue], [JobProfile_JobID]);


GO
CREATE NONCLUSTERED INDEX [ix_JobExecution_JobProfileID_ProcessingDate]
    ON [SEIDR].[JobExecution]([JobProfileID] ASC, [ProcessingDate] ASC) WITH (FILLFACTOR = 100);


GO
CREATE NONCLUSTERED INDEX [IDX_JobExecution_ExecutionStatus_JobExecutionID_ExecutionStatusCode_ExecutionStatusNameSpace]
    ON [SEIDR].[JobExecution]([ExecutionStatus] ASC)
    INCLUDE([JobExecutionID], [ExecutionStatusCode], [ExecutionStatusNameSpace]);

