
CREATE PROCEDURE [SEIDR].[usp_JobExecution_SetCurrentStep] 
	@JobExecutionID bigint,
	@StepNumber tinyint = null,
	@JobProfile_JobID int = null,
	@Prioritize bit = null,
	@Refresh bit = 0,
	@ProcessingDate date = null, --Allow forcing an incorrect ProcessingDate to be changed
	@FilePath varchar(500) = null, --Only usable on current step when @SafetyMode = 1. 
	@ResetRetryCountOnRefresh bit = 1,
	@SafetyMode bit = 1,
	@StopAfterStepNumber tinyint = null,
	@ClearStopAfterStepNumber bit = 0,
	@UserNote varchar(2000) = null
AS
BEGIN
	SET XACT_ABORT ON

	IF @Refresh = 1 
	BEGIN
		IF @StepNumber is not null
		BEGIN
			SELECT TOP 1 @JobProfile_JobID = JobProfile_JobID
			FROM SEIDR.vw_LogLatest 
			WHERE JobExecutionID = @JobexecutionID
			AND StepNumber = @StepNumber

			IF @@ROWCOUNT = 0
			BEGIN
				RAISERROR('@Refresh = 1; parameter conflicts with @StepNumber parameter (%d). 
DO NOT PASS @REFRESH = 1 UNLESS YOU WANT TO RERUN THE MOST RECENTLY ATTEMPTED STEP.', 16, 1, @StepNumber)
				RETURN
			END
		END
		ELSE
			SELECT TOP 1 @JobProfile_JobID = JobProfile_JobID, @StepNumber = StepNumber
			FROM SEIDR.vw_LogLatest 
			WHERE JobExecutionID = @JobexecutionID						
	END
	IF @StepNumber is null and @JobProfile_JobID is null
	BEGIN
		IF @Prioritize = 1 
		BEGIN
			UPDATE SEIDR.JobExecution
			SET PrioritizeNow = 1
			WHERE JobExecutionID = @JobExecutionID
			RAISERROR('No @StepNumber/@JobProfile_JobID - Only Setting PrioritizeNow.', 0, 0)
			RETURN
		END
		RAISERROR('Must provide @StepNumber or @JobProfile_JobID', 16, 1)
		RETURN
	END
	SET NOCOUNT ON
	DECLARE @CurrentStep tinyint
	DECLARE @ExecutionStatusCode varchar(2) = 'SC', @ExecutionStatusNameSpace varchar(130) = 'SEIDR', @NoteText varchar(2000) = null
	DECLARE @JobProfileID int, @CurrentStatusNameSpace varchar(130), @JobNameSpace varchar(130)
	SET @FilePath = NULLIF(@FilePath, '')
	IF @FilePath is NOT null AND @Refresh = 0
	BEGIN
		IF @StepNumber is null
		BEGIN
			RAISERROR('MUST PROVIDE STEP NUMBER IF MANUALLY SETTING @FILEPATH WITHOUT REFRESH', 16, 1)
			RETURN
		END
			
		SELECT @JobProfileID = JobProfileID, 
				@CurrentStatusNameSpace = ExecutionStatusNameSpace,
				@CurrentStep = StepNumber,
				@NoteText = 'REQUEST SETTING FILE PATH FROM ' + CASE WHEN FilePath is null then '(NULL)' else '"' + FilePath + '"' end
								+ ' TO "' + @FilePath + '" via usp_JobExecution_SetCurrentStep'
		FROM SEIDR.JobExecution
		WHERE JobExecutionID = @JobExecutionID
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('JobExecution not found for ID %d', 16, 1, @JobExecutionID)
			RETURN
		END		
		IF @CurrentStep <> @StepNumber AND @SafetyMode = 1
		BEGIN
			RAISERROR('Cannot change FilePath if StepNumber is also being changed. Please set @SafetyMode = 0 if this is really needed.', 16, 1)
			RETURN
		END
	END		
	ELSE
	BEGIN
		IF @FilePath is not null
		BEGIN
			SET @NoteText = 'Refreshing latest step but with new FilePath... "' + @FilePath + '"'
		END

		SELECT @JobProfileID = JobProfileID, 
				@CurrentStatusNameSpace = ExecutionStatusNameSpace,
				@CurrentStep = StepNumber,
				@FilePath = COALESCE(@FilePath, FilePath, '')
		FROM SEIDR.JobExecution
		WHERE JobExecutionID = @JobExecutionID
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('JobExecution not found for ID %d', 16, 1, @JobExecutionID)
			RETURN
		END
			
	END

	IF @JobProfile_JobID IS NULL
	BEGIN
		SELECT @JobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job
		WHERE StepNumber = @StepNumber 
		and JobProfileID = @JobProfileID
		AND Active = 1

		IF @@ROWCOUNT <> 1
		BEGIN
			RAISERROR('Unable to uniquely identify JobProfile_Job record for StepNumber %d', 16, 1, @StepNumber)
			RETURN
		END
	END
	
	DECLARE @ReqBranch varchar(30)
	SELECT 
		@StepNumber = StepNumber,
		@ExecutionStatusCode = COALESCE(TriggerExecutionStatusCode, 'SC'),
		@ExecutionStatusNameSpace = CASE WHEN TriggerExecutionStatusCode is null then 'SEIDR' else TriggerExecutionStatusCode end,		
		@ReqBranch = COALESCE(jpj.TriggerBranch, jpj.Branch),
		@JobNameSpace = j.[JobNameSpace]
	FROM SEIDR.JobProfile_Job jpj
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	WHERE JobProfile_JobID = @JObProfile_JobID
		
	IF @SafetyMode = 1
	AND @StepNumber > (SELECT MAX(StepNumber)
						FROM SEIDR.JobExecution_ExecutionStatus WITH (NOLOCK)
						WHERE JobExecutionID = @JobExecutionID) 
	BEGIN
		SELECT * FROM SEIDR.vw_JobExecutionHistory WITH (NOLOCK) WHERE JobExecutionID = @JobExecutionID
		RAISERROR('Step has not previously attempted to run. Please verify that this step is really ready to run and pass @SafetyMode = 0 if so.', 16, 1)
		RETURN
	END

	IF @SafetyMode = 1
	AND (@StepNumber < @CurrentStep OR @StepNumber > @CurrentStep + 1)
	AND EXISTS(SELECT null 
				FROM SEIDR.JobExecution_ExecutionStatus WITH (NOLOCK)
				WHERE JobExecutionID = @JobExecutionID
				AND StepNumber = @StepNumber
				AND FilePath <> @FilePath) 
	--Note, those won't capture null FilePath history records, and that's okay. 
	---If a jobExecution runs with null FilePath successfully in the first place, then it shouldn't be using the column
	BEGIN						
		SELECT * 
		FROM SEIDR.vw_JobExecution_Rollback_Helper WITH (NOLOCK) 
		WHERE JobExecutionID = @JobExecutionID
		AND StepNumber = @StepNumber

		RAISERROR('Please see SEIDR.usp_JobExecution_Rollback instead. FilePath has changed since Step was originally run.', 16, 1)
		RETURN
	END

	IF NOT EXISTS(SELECT null 
					FROM SEIDR.ExecutionStatus s
					WHERE s.ExecutionStatusCode = @ExecutionStatusCode 
					AND COALESCE(@ExecutionstatusNameSpace, @CurrentStatusNameSpace) = s.[NameSpace])
	BEGIN
		IF EXISTS(SELECT null FROM SEIDR.ExecutionStatus WHERE ExecutionStatusCode = @ExecutionStatusCode AND [NameSpace] = 'SEIDR')
		BEGIN
			SET @ExecutionStatusNameSpace = 'SEIDR'
		END
		ELSE IF EXISTS(SELECT null FROM SEIDR.ExecutionStatus WHERE ExecutionStatusCode = @ExecutionStatusCode AND NameSpace = @JobNameSpace)
		BEGIN
			SET @ExecutionStatusNameSpace = @JobNameSpace
		END
		ELSE
		BEGIN
			RAISERROR('Could not identify NameSpace for JobProfile_Job status.', 16, 1)
			RETURN
		END
	END 
	/*
	IF @SafetyMode = 1
	AND @ProcessingDate is not null
	AND @ProcessingDate <> (SELECT ProcessingDate FROM SEIDR.JobExecution WHERE JobExecutionID = @JobExecutionID)
	BEGIN
		RAISERROR('Must Set @SafetyMode = 0 to change ProcessingDate.', 16, 1)
		RETURN
	END
	*/

	IF @SafetyMode = 1
	AND EXISTS(SELECT null 
				FROM SEIDR.JobExecution je
				JOIN SEIDR.ExecutionStatus es
					ON je.ExecutionStatusCode = es.ExecutionStatusCode
					AND je.ExecutionStatusNameSpace = es.[NameSpace]
				WHERE je.JobExecutionID = @JobExecutionID
				AND es.IsComplete =1)
	BEGIN
		RAISERROR('JobExecution is complete.', 16, 1)
		RETURN
	END

	BEGIN TRY
		BEGIN TRAN
		
		IF @StepNumber <> @CurrentStep
			SET @ResetRetryCountOnRefresh = 0
		IF @ResetRetryCountOnRefresh = 1
			SET @NoteText = 'Setting RetryCount = 0 and retrying current step...	' + ISNULL(@NoteText, '')		


		UPDATE SEIDR.JobExecution
		SET StepNumber = @StepNumber,
			ExecutionStatusCode = @ExecutionStatusCode,
			ExecutionStatusNameSpace = @ExecutionStatusNameSpace,
			PrioritizeNow = COALESCE(@Prioritize, PrioritizeNow),
			Branch = @ReqBranch,
			PreviousBranch = null, --Potentially breaking sequence, so this history is not going to be meaningful anymore.
			FilePath = NULLIF(@FilePath, ''),
			ProcessingDate = COALESCE(@ProcessingDate, ProcessingDate),
			RetryCount = IIF(@ResetRetryCountOnRefresh = 1, 0, RetryCount),
			StopAfterStepNumber = CASE WHEN @ClearStopAfterStepNumber = 0 then COALESCE(@StopAfterStepNumber, StopAfterStepNumber) end
		WHERE JobExecutionID = @JobExecutionID

		IF @NoteText is not null
			EXEC SEIDR.usp_JobExecution_Note_i
				@JobExecutionID = @JobExecutionID,
				@NoteText = @NoteText,
				@Technical = 0--, @Auto = 1
		IF @UserNote is not null
			exec SEIDR.usp_JobExecution_Note_i
				@JobExecutionID = @JobExecutionID,
				@NoteText = @UserNote,
				@Technical = 0--, @Auto = 0


		SELECT * FROM SEIDR.vw_JobExecution WITH (NOLOCK) WHERE JobExecutionID = @JobExecutionID
		
		IF @@ROWCOUNT = 0 OR @JobProfile_JobID <> (SELECT JobProfile_JobID FROM SEIDR.JobExecution WHERE JobExecutionID = @JobExecutionID)
		BEGIN
			--ROLLBACK
			RAISERROR('Failed to Set JobExecution to expected step.', 16, 1)
			--RETURN
		END

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
		;throw
		RETURN -1
	END CATCH
END