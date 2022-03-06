CREATE PROCEDURE [SEIDR].[usp_JobExecution_Help]
	@JobExecutionID bigint,
	@IncludeProfile bit = 1,
	@IncludeConfigurations bit = 1,
	@IncludePaths bit = 1,
	@IncludeNotes bit = 1,
	@IncludeHistory bit = 1,
	@HistoryLatestOnly bit = 1 --Does nothing unless @IncludeHistory = 1	
	--ToDo: Include Contact Info, ContactNotes, profile documentation (JobProfileNote) (separate branch currently)
AS
BEGIN
	--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SET NOCOUNT ON;
	SET XACT_ABORT ON
	
	DECLARE @CurrentStep tinyint, @JobProfileID int, @JobProfile_JobID int, @ProcessingDate datetime,
		@RetryCount int, @IsComplete bit, @IsError bit, @IsWorking bit,
		@STATUS varchar(150), @StopAfterStepNumber tinyint,
		@StatusCode varchar(178), @HasOrgID bit, @HasProjectID bit, @NextTry datetime,
		@Manual bit, @Duplicate bit, @NotNeeded bit, 
		@Branch varchar(30)

	SELECT @CurrentStep = je.StepNumber, 
		@JobProfileID = je.JobProfileID,
		@JobProfile_JobID = je.JobProfile_JobID, 
		@ProcessingDate = ProcessingDate,
		@RetryCount = RetryCount,
		@IsComplete = es.IsComplete,
		@IsError = es.isError,
		@IsWorking = je.IsWorking,
		@STATUS = es.Description,
		@StatusCode = je.ExecutionStatus,
		@Branch = je.Branch,
		@HasOrgID = IIF(je.OrganizationID is null, 0, 1),
		@HasProjectID = IIF(je.ProjectID is null, 0, 1),
		@StopAfterStepNumber = je.StopAfterStepNumber,
		@NextTry = CASE WHEN je.RetryCount > 0 AND je.IsWorking = 0 THEN DATEADD(minute, jpj.RetryDelay, je.LU) end,
		@Manual = je.[Manual],
		@Duplicate = je.[Duplicate],
		@NotNeeded = je.NotNeeded
	FROM SEIDR.JobExecution je WITH (NOLOCK)
	JOIN SEIDR.ExecutionStatus es WITH (NOLOCK)
		ON je.ExecutionStatusCode = es.ExecutionStatusCode
		AND je.ExecutionStatusNameSpace = es.[NameSpace]
	LEFT JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON je.JobProfile_JobID = jpj.JobProfile_JobID
	WHERE JobExecutionID = @JobExecutionID
	

	IF @@ROWCOUNT = 0
	BEGIN
		--DECLARE @JobExecutionID bigint = 5430042
		RAISERROR('JobExecutionID not found: %I64d', 16, 1, @JobExecutionID) 
		RETURN
	END
	
	IF @NextTry is not null
		SET @NextTry = DATEADD(hour, SECURITY.ufn_GetTimeOFfset(null), @NextTry)

	DECLARE @InSequence bit, @WorkPriority int
	IF @JobProfile_JobID IS NOT NULL
		SELECT @InSequence = InSequence, @WorkPriority = WorkPriority
		FROM SEIDR.vw_JobExecution WITH (NOLOCK) 
		WHERE JobExecutionID = @JobExecutionID
	

	DECLARE @executionSQL varchar(4000) = '
	SELECT ''' +
		@StatusCode + ''' [ExecutionStatus], ''' + @STATUS + ''' [Status Description], ''' + 
		CASE 
			WHEN @Manual = 1 then 'DEACTIVATED - MANUALLY COMPLETED OUTSIDE OF SERVICE'
			WHEN @Duplicate = 1 then 'DEAcTIVATED - DUPLICATE'
			WHEN @NotNeeded = 1 then 'DEACTIVATED - JOB EXECUTION IS NOT NEEDED'
			WHEN @IsComplete = 1 AND @IsError = 1 then 'COMPLETED VIA ERROR'
			WHEN @IsComplete = 1 then 'COMPLETE'					
			WHEN @StopAfterStepNumber is not null and @CurrentStep > @StopAfterStepNumber AND @IsWorking = 0 then 'STOPPED'				
			WHEN @IsError = 1 AND @JobProfile_JobID is null then 'ERROR'
			WHEN @IsError = 1 then 'ERROR HANDLING STEP - ' + IIF(@IsWorking = 1, 'WORKING', 'PENDING')
			WHEN @RetryCount > 0 then 'ERROR - RETRYING - ' + IIF(@IsWorking = 1, 'WORKING', 'PENDING')
			WHEN @JObProfile_JobID IS NOT NULL then IIF(@IsWorking = 1, 'WORKING', 'PENDING')
			ELSE 'INCOMPLETE'
			end 
		  + IIF(@InSequence = 0, '- OUT OF SEQUENCE', '') + ''' as [Progress], je.IsWorking,
		' + CONVERT(varchar(30), @RetryCount) + ' [RetryCount],'
	IF @NextTry IS NOT NULL AND @NextTry > GETDATE()
		SET @ExecutionSQL += '''' + CONVERT(varchar(30), @NextTry) + ''' as [NextAttempt],'
	DECLARE @Stopped varchar(500) = ''
	IF @StopAfterStepNumber is not null
	BEGIN
		IF @CurrentStep <= @StopAfterStepNumber OR @IsWorking = 1
			SET @Stopped = 'CAST(0 as bit) [Stopped],'
		ELSE 
			SET @Stopped = 'CAST(1 as bit) [Stopped],'			
	END

	SET @ExecutionSQL += '
		je.[StepNumber], je.[StopAfterStepNumber], ' + @Stopped + '
		je.[ProcessingDate], je.Branch, je.FilePath, '

	IF @InSequence is not null
	BEGIN
		SELECT @executionSQL += '''' + 
			CASE WHEN @InSequence = 1 then 'TRUE' else 'FALSE' end + ''' [InSequence],'
			+ CONVERT(varchar(30), @WorkPriority) + ' [WorkPriority],'
	END
	IF @JobProfile_JobID is not null
	BEGIN
		SET @ExecutionSQL += '
		j.JobName, j.JobNameSpace,'
	END

	SET @ExecutionSQL += '
		je.UserKey1, je.UserKey2, '

	IF @HasOrgID = 1
	BEGIN
		SET @ExecutionSQL += '
			je.OrganizationID,  o.Description [Organization], '
	END


	IF @HasProjectID = 1
	BEGIN
		SET @ExecutionSQL += ' 
		jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive], '
	END
	SET @ExecutionSQL += '
		je.LoadProfileID,
		je.TotalExecutiontimeSeconds,
		je.SpawningJobExecutionID, je.METRIX_ExportBatchID, '
	IF @JobProfile_JobID IS NULL
	BEGIN
		SET @ExecutionSQL += '
		jp.RequiredThreadID'
	END
	ELSE
	BEGIN
		SET @ExecutionSQL += '
		COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID) RequiredThreadID,  
		CASE WHEN jpj.JobProfile_JobID IS NOT NULL THEN ISNULL(jpj.RetryDelay, 10) end [RetryDelay],
		jpj.CanRetry, jpj.RetryLimit'
	END
	SET @ExecutionSQL += '
		FROM SEIDR.JobExecution je
		JOIN SEIDR.JobProfile jp
			ON je.JobProfileID = jp.JobProfileID'

	IF @JobProfile_JobID IS NOT NULL
	BEGIN
		SET @ExecutionSQL += '
		INNER JOIN SEIDR.JobProfile_Job jpj
			ON je.JobProfile_JobID = jpj.JobProfile_JobID
		INNER JOIN SEIDR.Job j
			ON jpj.JobID = j.JobID'
	END
	IF @HasOrgID = 1
	BEGIN
		SET @ExecutionSQL += '
		INNER JOIN REFERENCE.Organization o
			ON je.OrganizationID = o.OrganizationID'
	END
	IF @HasProjectID = 1
	BEGIN 
		SET @ExecutionSQL += '
		INNER JOIN REFERENCE.Project p
			ON je.ProjectID = p.ProjectID'
	END
	SET @ExecutionSQL += '
		WHERE je.JobExecutionID = ' + CONVERT(varchar(30), @JobExecutionID)
			
	--print @ExecutionSQL
	exec (@ExecutionSQL)
	IF @IncludeProfile = 1
	BEGIN
		SELECT '' [JOB PROFILE] WHERE 1=0
		SELECT * 
		FROM SEIDR.vw_JobProfile WITH (NOLOCK) 
		WHERE JobProfileID = @JobProfileID
	END
		

	IF EXISTS(SELECT null 
				FROM SEIDR.vw_JobExecution WITH (NOLOCK)
				WHERE JobExecutionID = @JobExecutionID
				AND InSequence = 0)
	BEGIN
		SELECT '' [JOBEXECUTION IS NOT IN SEQUENCE] WHERE 1=0

		IF EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent WITH (NOLOCK) WHERE JobProfile_JobID = @JobProfile_JobID)
		BEGIN
			SELECT p.*, 
				je.JobExecutionID [ParentJobExecutionID], 
				je.StepNumber [CurrentStepNumber], 
				je.ExecutionStatus [CurrentExecutionStatus], 
				es.Description [CurrentExecutionStatusDescription], 
				es.IsError, es.IsComplete
			FROM SEIDR.vw_JobProfile_Parent p WITH (NOLOCK)
			LEFT JOIN SEIDR.JobExecution je WITH (NOLOCK)
				ON p.ParentJobProfileID = je.JobProfileID
				AND DATEDIFF(day, @ProcessingDate, je.ProcessingDate) = p.SequenceDayMatchDifference
			LEFT JOIN SEIDR.ExecutionStatus es WITH (NOLOCK)
				ON je.ExecutionStatusCode = es.ExecutionStatusCode
				AND je.ExecutionStatusNameSpace = es.[NameSpace]
			WHERE p.JobProfile_JobID = @JobProfile_JobID
		END		
		IF EXISTS(SELECT null FROM SEIDR.JobProfile_Job WITH (NOLOCK) WHERE JobProfile_JobID = @JobProfile_JobID AND SequenceScheduleID IS NOT NULL)
		BEGIN
			DECLARE @SeqScheduleID int 

			SELECT @SeqScheduleID = SequenceScheduleID
			FROM SEIDR.JobProfile_Job WITH (NOLOCK)
			WHERE JobProfile_JobID = @JobProfile_JobID

			SELECT CASE WHEN SEIDR.ufn_CheckSchedule(@SeqScheduleID, CONVERT(datetime, @ProcessingDate) + CASE WHEN @ProcessingDate < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, MAX(jes.ProcessingDate)) is null then 0 else 1 end 
				as [SequenceScheduleMatches]
			FROM SEIDR.JobExecution_ExecutionStatus jes WITH (NOLOCK)
			JOIN SEIDR.JobExecution je2 WITH (NOLOCK)
				ON jes.JobExecutionID = je2.JobExecutionID
				AND je2.Active = 1
			WHERE jes.JobProfile_JobID = @JobProfile_JobID
			AND jes.Success = 1 AND jes.IsLatestForExecutionStep = 1
		END
	END

	IF @IncludeConfigurations = 1
	BEGIN	/*	
		IF @IncludeHistory = 0
		BEGIN		
			SELECT '' [CURRENT STEP CONFIGURATION] WHERE 1=0
			exec SEIDR.usp_JobProfile_Help @JobProfileID = @JobProfileID, @JobProfile_JobID = @JobProfile_JobID
		END
		ELSE
		BEGIN
			SELECT JobProfile_JobID, StepNumber
			FROM SEIDR.JobProfile_job
			WHERE JobProfileID = @JobProfileID
			AND StepNumber <= @CurrentStep
			AND Active = 1
			ORDER BY StepNumber asc*/

			SELECT '' [STEP CONFIGURATIONS] WHERE 1=0
			RAISERROR('To view Scripts for setting up profile... run the following: 
exec SEIDR.usp_JobProfile_GetConfigurationScript @JobProfileID = %d

', 0, 0, @JobProfileID) WITH NOWAIT
			RAISERROR('To view more details about profile configuration.... run the following:
exec SEIDR.usp_JobProfile_Help @JobProfileID = %d

', 0, 0, @JobProfileID) WITH NOWAIT
			DECLARE step_Cursor CURSOR LOCAL FAST_FORWARD
			FOR SELECT distinct ConfigurationTable, j.JobID, j.JobName
			FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
			JOIN SEIDR.Job j WITH (NOLOCK)
				ON jpj.JobID = j.JobID
			WHERE jpj.Active = 1
			AND Jpj.JobProfileID = @JobProfileID
			AND (@IncludeHistory = 1 or JobProfile_JobID = @JobProfile_JobID)
			AND jpj.StepNumber <= @CurrentStep
			--ORDER BY StepNumber ASC, TriggerExecutionNameSpace ASC, TriggerExecutionStatusCode ASC
	

			DECLARE @stepID int
			DECLARE @Config varchar(256), @JobID int, @JobName varchar(128)

			OPEN step_Cursor

			FETCH NEXT FROM step_Cursor
			INTO @Config, @JobID, @JobName

			WHILE @@FETCH_STATUS = 0
			BEGIN		
				RAISERROR('

@Config "%s", @JobID: %d (%s)', 0, 0, @Config, @JobID, @Jobname) WITH NOWAIT
				DECLARE @Active nvarchar(100) = ''
			
				IF COL_LENGTH(@Config, 'Active') IS NOT NULL
					SET @Active = 'AND x.Active = 1 '
				IF COL_LENGTH(@Config, 'IsValid') IS NOT NULL
					SET @Active += 'AND x.IsValid = 1 '
				SET @Active += 'AND jpj.Active = 1'
			
				DECLARE @ColList varchar(2000) = ''
				IF @JobName = 'FTPJob'
					SET @ColList = ', CASE WHEN FTPOperationID = 2 then ''IF RECEIVING A SINGLE FILE - JobExecution.FilePath will update'' end [FilePathNote]'
		
				SELECT @ColList += ', x.[' + name + ']'
				FROM sys.columns 
				WHERE OBJECT_ID = OBJECT_ID(@Config)
				AND Name NOT IN ('DC', 'LU', 'JobProfile_JobID', 'RV', 'CB', 'DD', 'Active', 'IsValid')
				
								
				DECLARE @ConfigSproc varchar(130) = REPLACE(@Config, '.', '.usp_JobProfile_') + '_iu'
				DECLARE @ConfigView varchar(130) = REPLACE(@Config, '.', '.vw_')
				IF @Config NOT LIKE '%Job'
				BEGIN
					IF OBJECT_ID(@ConfigSproc) IS NULL
						SET @ConfigSproc = REPLACE(@ConfigSproc,'_iu', 'Job_iu')
					IF OBJECT_ID(@ConfigView) is null
						SET @ConfigView += 'Job'
				END

				DECLARE @SQL nvarchar(4000) = 'SELECT ''' + @config + ''' as [' + @JobName + '], 
					[ConfigurationSelectCommand] = ''SELECT * FROM ' + @ConfigView + ' WHERE JobProfileID = ' + CONVERT(varchar(30), @JobProfileID) + ''''										
				exec (@SQL)
				
				RAISERROR('Configuration Stored Procedure: %s
Configuration View: SELECT * FROM %s WHERE JobProfileID = %d', 0, 0, @ConfigSproc, @ConfigView, @JobProfileID)

				SET @SQL = N'SELECT jpj.JobProfile_JobID, jpj.StepNumber, jpj.Description [Step Description], jpj.JobID' + @ColList + N'
				,jpj.CanRetry, jpj.RetryLimit, ISNULL(jpj.RetryDelay, 10) [RetryDelay], jpj.RequiredThreadID, jpj.FailureNotificationMail
				FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
				JOIN ' + @Config + ' x WITH (NOLOCK)
					ON jpj.JobProfile_JobID = x.JobProfile_JobID ' + @Active + N'
				WHERE jpj.JobProfileID = ' + CONVERT(varchar(20), @JobProfileID) 
				--IF @JobProfile_JobID IS NOT NULL
				--	SET @SQL += N'	AND jpj.JobProfile_JobID = ' + CONVERT(varchar(20), @JobProfile_JobID)
		
				SET @SQL += N'
				ORDER BY jpj.StepNumber ASC, jpj.JobProfile_JobID ASC'

				--print @SQL
				exec(@SQL)

			
				FETCH NEXT FROM step_Cursor
				INTO @Config, @JobID, @JobName
			END
	
			CLOSE step_Cursor
			DEALLOCATE step_cursor
		--END
	END

	IF @IncludeHistory = 1 AND @IncludePaths = 1
	BEGIN
		SELECT '' [EXECUTION HISTORY + EXECUTION FILE PATHS] WHERE 1=0
		SELECT jes.JobExecution_ExecutionStatusID, 
			jes.JobProfile_JobID, r.[StepDescription], jes.StepNumber, jes.ExecutionStatus,
			jes.FilePath [StepExecutionFilePath], jes.FileSize, jes.Success, jes.RetryCount,
			ConfigurationSource, UnmaskedFilePath, SetExecutionFilePath, 
			r.[RollbackCommand],
			r.RollbackHint
		FROM SEIDR.JobExecution_ExecutionStatus jes WITH (NOLOCK)
		JOIN [SEIDR].[vw_JobExecution_Rollback_Helper] r WITH (NOLOCK)
			ON jes.JobExecution_ExecutionStatusID = r.JobExecution_ExecutionStatusID
		LEFT JOIN SEIDR.vw_JobExecution_Filepath f WITH (NOLOCK)
			ON jes.JobExecutionID = f.JobExecutionID
			AND jes.StepNumber = f.StepNumber
		WHERE jes.JobExecutionID = @JobExecutionID 
		AND jes.IsLatestForExecutionStep >= @HistoryLatestOnly
		ORDER BY jes.JobExecution_ExecutionStatusID 
	END
	ELSE
	BEGIN
		IF @IncludeHistory = 1
		BEGIN			
			SELECT '' [EXECUTION HISTORY] WHERE 1=0
		--SELECT TOp 10 * FROM [SEIDR].[vw_JobExecution_Rollback_Helper]
			SELECT jes.JobExecution_ExecutionStatusID, 
			jes.JobProfile_JobID, r.StepDescription, jes.StepNumber, jes.ExecutionStatus,
			jes.FilePath [StepExecutionFilePath], jes.FileSize, jes.Success, jes.RetryCount,						
			r.[RollbackCommand],
			r.RollbackHint
			FROM SEIDR.JobExecution_ExecutionStatus jes WITH (NOLOCK)
			JOIN [SEIDR].[vw_JobExecution_Rollback_Helper] r WITH (NOLOCK)
				ON jes.JobExecution_ExecutionStatusID = r.JobExecution_ExecutionStatusID
			WHERE jes.JobExecutionID = @JobExecutionID 
			AND jes.IsLatestForExecutionStep >= @HistoryLatestOnly
			ORDER BY jes.JobExecution_ExecutionStatusID
		END
		IF @IncludePaths = 1
		BEGIN
			SELECT '' [EXECUTION FILE PATHS]
			SELECT JobProfile_JobID, StepDescription, StepNumber, ConfigurationSource, UnmaskedFilePath, SetExecutionFilePath
			FROM SEIDR.vw_JobExecution_FilePath WITH (NOLOCK)
			WHERE JobExecutionID = @JobExecutionID
			AND StepNumber <= @CurrentStep
			ORDER BY StepNumber, JobProfile_JobID, ConfigurationSource
		END
	END
	IF @IncludeHistory = 0
	BEGIN
		SELECT '' [LATEST LOG ENTRY FOR CURRENT STEP] WHERE 1=0
		SELECT * 
		FROM  [SEIDR].[vw_LogLatest]  WITH (NOLOCK)
		WHERE jobExecutionID = @JobExecutionID 		
		ORDER BY ID ASC
	END
	ELSE
	BEGIN
		SELECT '' [LATEST LOG ENTRIES] WHERE 1=0
		SELECT * 
		FROM  [SEIDR].[vw_LogStepLatest]  WITH (NOLOCK)
		WHERE jobExecutionID = @JobExecutionID 
		ORDER BY ID ASC
	END		

	IF @IncludeNotes = 1
	BEGIN
		SELECT 'SELECT * FROM SEIDR.vw_JobExecution_Note WHERE JobExecutionID = ' + CONVERT(varchar(30), @JobExecutionID) [JOB EXECUTION NOTES],
				'EXEC SEIDR.usp_JobExecution_Note_i @JobExecutionID = ' +  CONVERT(varchar(30), @JobExecutionID) + ', 
		@NoteText = ??, --Note Content
		@StepNumber = null, /* Default - current step, override for specific step (history note) */
		@Technical = 1 /* Set Technical to 0 if details do not involve Code or File Content: e.g., rerun package or needing to reach out to client */' [ADD NOTE COMMAND]
		SELECT 
			StepNumber, StepNoteSequence, NoteSequence, 
			UserName, NoteText, 
			DC, ExecutionStatus, --ProcessingDate, 
			FilePath, Success, JobName, JobNameSpace
		FROM SEIDR.vw_JobExecution_Note WITH (NOLOCK) 
		WHERE JobExecutionID = @JobExecutionID
		ORDER BY StepNumber, StepNoteSequence
	END
END
GO

