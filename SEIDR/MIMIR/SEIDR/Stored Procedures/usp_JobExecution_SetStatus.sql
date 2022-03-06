CREATE PROCEDURE [SEIDR].[usp_JobExecution_SetStatus]
	@JobExecutionID bigint,
	@JobProfileID int,
	@FilePath varchar(250),
	@FileSize bigint,
	@FileHash varchar(88),	
	@Success bit,
	@ExecutionStatusCode varchar(5) = null,
	@ExecutionStatusNameSpace varchar(128),
	@ExecutionTimeSeconds int = null,
	@Complete bit = null output,
	@METRIX_LoadBatchID int,
	@METRIX_ExportBatchID int = null
AS
BEGIN	
	SET DEADLOCK_PRIORITY HIGH
	DECLARE @CanRetry bit = 0 
	set @Complete = 0

	DECLARE @RetryLimit SMALLINT
	DECLARE @RetryCount SMALLINT
	DECLARE @StepNumber smallint
	DECLARE @IsError bit = 0
	DECLARE @Branch varchar(30)

	SELECT @StepNumber = StepNumber, @Branch = Branch
	FROM SEIDR.JobExecution 
	WHERE JobExecutionID = @JobExecutionID
	
	DECLARE @CurrentStatusCode varchar(5)
	DECLARE @CurrentNameSpace varchar(128)
	

	IF EXISTS(SELECT 1 FROM SEIDR.JobExecution WITH (NOLOCK) WHERE JobProfileID = @JobProfileID AND FilePath = @FilePath AND Active = 1 AND JobExecutionID <> @JobExecutionID) 
	BEGIN
		  UPDATE 
			SEIDR.JobExecution
		  SET 
			ExecutionStatusCode = 'X', 
			ExecutionStatusNameSpace = 'SEIDR',
			IsWorking = 0,
			InWorkQueue = 0,
			[TotalExecutionTimeSeconds] = COALESCE([TotalExecutionTimeSeconds], 0) + @ExecutionTimeSeconds
		  WHERE 
			JobExecutionID = @JobExecutionID 

		  RETURN 70
	END

	IF @Success = 0 
	AND (@ExecutionStatusCode <> 'CX' OR @ExecutionStatusNameSpace <> 'SEIDR')  
	--if Unsuccessful due to service cancel, don't need to check @canRetry or validate status.
	BEGIN
		SELECT 			
			@CurrentStatusCode = ExecutionStatusCode, 
			@CurrentNameSpace = ExecutionStatusNameSpace,
			@StepNumber = StepNumber,
			@RetryCount = RetryCount, 
			@RetryLimit = RetryLimit,
			@IsError = IsError --Running from Error state via Trigger Status
		FROM SEIDR.vw_JobExecution 
		WHERE JobExecutionID = @JobExecutionID
		AND CanRetry = 1
		
		IF @@ROWCOUNT > 0 AND @RetryCount <= @RetryLimit
		BEGIN			
			SET @CanRetry = 1 
			SET @ExecutionStatusCode = @CurrentStatusCode
			SET @ExecutionStatusNameSpace = @CurrentNameSpace
		END
		ELSE IF @ExecutionStatusCode is null
		OR @ExecutionStatusNameSpace is null
		OR NOT EXISTS(SELECT null 
					FROM SEIDR.ExecutionStatus
					WHERE IsError = 1
					AND ExecutionStatusCode = @ExecutionStatusCode
					AND [NameSpace] = @ExecutionStatusNameSpace)					
		BEGIN			
			SET @ExecutionStatusCode = 'F'
			SET @ExecutionStatusNameSpace = 'SEIDR'		
		END	

		IF @canRetry = 0 --Initial value, so CanRetry = 0 or @RetryCount. @RetryLimit
		AND @ExecutionStatusCode = @CurrentStatusCode 
		AND @ExecutionStatusNameSpace = @CurrentNameSpace --Cannot retry more, but we are working with a failure trigger - Current Status is the same as the error status we're moving to.
		BEGIN
			-- Trying to go from 'SEIDR.F' -> 'SEIDR.F' as a result of no more retries, switch to SEIDR.FF.
			SET @ExecutionStatusCode = 'FF'							
			IF NOT EXISTS(SELECT null FROM SEIDR.ExecutionStatus WITH (NOLOCK) WHERE ExecutionSTatusCode = 'FF' AND [NameSpace] = 'SEIDR')
			BEGIN
				INSERT INTO SEIDR.ExecutionStatus(ExecutionStatusCode, [NameSpace], [Description], IsError, IsComplete)
				VALUES('FF', 'SEIDR', 'Error Trigger Failure - FORCE STOP', 1, 1)
			END		
		END
			
		
	END
	ELSE IF @Success = 1
	BEGIN
		IF @ExecutionStatusCode is null
		OR @ExecutionStatusNameSpace is null
		OR NOT EXISTS(SELECT null 
					FROM SEIDR.ExecutionStatus
					WHERE IsError = 0
					AND ExecutionSTatusCode = @ExecutionStatusCode
					AND [NameSpace] = @ExecutionStatusNameSpace)
		BEGIN
			SET @ExecutionStatusCode = 'SC' --StepComplete
			SET @ExecutionStatusNameSpace = 'SEIDR'
		END					

		IF NOT EXISTS(SELECT null
						FROM SEIDR.JobProfile_Job jpj
						WHERE JobProfileID = @JobProfileID
						AND StepNumber = @StepNumber + 1
						AND Active = 1
						AND (TriggerExecutionStatusCode is null or TriggerExecutionSTatusCode = @ExecutionStatusCode) 
						AND(TriggerExecutionNameSpace is null or TriggerExecutionNameSpace = @ExecutionStatusNameSpace)
						--If no Trigger Branch matches the current branch, will not map to a new JobProfile_JobID from function logic
						AND (TriggerBranch is null OR TriggerBranch = @Branch) 
						)
		BEGIN			
			set @Complete = 1--If there's no step work that's going to match up, complete.
			
			IF 0 = (SELECT IsComplete
					FROM SEIDR.ExecutionStatus
					WHERE ExecutionSTatusCode = @ExecutionStatusCode AND [NameSpace] = @ExecutionStatusNameSpace)
			BEGIN
				SET @ExecutionStatusCode = 'C' --Default completion status.
				SET @ExecutionStatusNameSpace = 'SEIDR'
			END
		END
		ELSE IF 1 = (SELECT IsComplete
					FROM SEIDR.ExecutionStatus
					WHERE ExecutionSTatusCode = @ExecutionStatusCode AND [NameSpace] = @ExecutionStatusNameSpace)
		BEGIN
			SET @Complete = 1 --Job does not allow follow-up steps. Should be rare
		END						
		ELSE
			SET @StepNumber += 1
	END


	UPDATE SEIDR.JobExecution
	SET FilePath = @FilePath,
		FileSize = @FileSize,
		FileHash = @FileHash,
		METRIX_LoadBatchID=@METRIX_LoadBatchID,
		METRIX_ExportBatchID = @METRIX_ExportBatchID,
		IsWorking = 0,
		ForceSequence = CASE WHEN @Success = 1 then 0 else ForceSequence end, --If we finished step successfully, then set ForceSequence = 0 (in case later step has a different parent/sequence schedule)
		InWorkQueue = CASE WHEN @CanRetry = 1 then 1 --@CanRetry is only set to 1 when success is 0 (failure) and the JobProfile_Job allows retry.
							WHEN @Success = 1 then 1 - @Complete
							else 0 end,
		StepNumber = @StepNumber,
		RetryCount = CASE 
						WHEN @StepNumber <> StepNumber then 0 -- new step Number, reset retry count
						WHEN @CanRetry = 0 then RetryCount --going to an error status, leave as-is.
						else RetryCount + 1 --Will retry with the same status.
						end,
		ExecutionStatusCode = @ExecutionStatusCode,
		ExecutionStatusNameSpace = @ExecutionStatusNameSpace,
		LastExecutionStatusCode = ExecutionStatusCode,
		LastExecutionStatusNameSpace = ExecutionStatusNameSpace,
		ExecutionTimeSeconds = @ExecutionTimeSeconds + CASE WHEN @CanRetry = 1 AND RetryCount > 0 then ExecutionTimeSeconds else 0 end, 
		--If we're retrying, just keep adding to the ExecutionTime
		[TotalExecutionTimeSeconds] = COALESCE([TotalExecutionTimeSeconds], 0) + @ExecutionTimeSeconds --Running total across all steps.
	WHERE JobExecutionID = @JobExecutionID
	
	
	--WIll run another select when it's actually picked up for working anyway, 
	--but nothing else should be changing this record, so nolocks should be okay
	IF @Success =1 AND @Complete = 0
	BEGIN
		SELECT * 
		FROM SEIDR.vw_JobExecution WITH (NOLOCK) 
		WHERE JobExecutionID = @JobExecutionID
		AND InSequence = 1
	END
	else if @CanRetry = 1
	BEGIN
		SELECT *, DATEADD(minute, COALESCE(RetryDelay, 10), GETDATE()) [DelayStart]
		FROM SEIDR.vw_JobExecution WITH (NOLOCK)
		WHERE JobExecutionID = @JobExecutionID
	END

	IF @@ROWCOUNT = 0 AND (@CanRetry = 1 OR @Success = 1 AND @Complete = 0)
		UPDATE SEIDR.JobExecution WITH (ROWLOCK)
		SET InWorkQueue = 0
		WHERE JobExecutionID = @JobExecutionID AND InWorkQueue = 1

END
