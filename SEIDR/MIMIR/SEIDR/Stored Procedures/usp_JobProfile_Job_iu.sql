CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_iu]
	@JobProfileID int,
	@StepNumber smallint out,
	@Description varchar(100),
	@TriggerExecutionStatus varchar(50),
	@TriggerExecutionNameSpace varchar(128),
	@CanRetry bit,
	@RetryLimit int,
	@RetryDelay int,
	@JobID int,
	@JobProfile_JobID int out,
	@ThreadID int,
	@FailureNotificationMail varchar(500),
	@RetryCountBeforeFailureNotification smallint,
	@SequenceSchedule varchar(300),
	@Branch varchar(30),
	@TriggerBranch varchar(30),
	@UserName varchar(260) = null
AS
BEGIN	
	DECLARE @Author varchar(260) = @UserName
	IF @UserName is null
		SET @userName = SUSER_NAME()
	ELSE IF @userName <> SUSER_NAME()
		SET @UserName += '(' + SUSER_NAME() + ')'

	IF @RetryLimit is null OR @CanRetry = 0
		SET @RetryLimit = 0

	IF @RetryLimit <= 0 --No retries, clean parameters for retrying.
	BEGIN
		SET @RetryLimit = 0
		SET @CanRetry = 0
		SET @RetryDelay = null
		SET @RetryCountBeforeFailureNotification = null
	END
	ELSE 
	BEGIN
		SET @CanRetry = 1 --Null but with @RetryLimit specified.
		IF EXISTS(SELECT null FROM SEIDR.Job WITH (NOLOCK) WHERE JobID = @JobID AND AllowRetry = 0)
		BEGIN
			SET @CanRetry = 0
			SET @RetryLimit = 0
			SET @RetryDelay = null
			SET @RetryCountBeforeFailureNotification = null
		END
		ELSE 
		BEGIN
		IF @RetryDelay IS NULL
			SELECT @RetryDelay = DefaultRetryTime --If RetryDelay is still null after this, and CanRetry = 1, then will default to 10 minutes in JobExecution_SetStatus
			FROM SEIDR.Job WITH (NOLOCK)
			WHERE JobID = @JobID
			IF @RetryCountBeforeFailureNotification IS NULL 
			OR @RetryCountBeforeFailureNotification > @RetryLimit
			BEGIN
				SET @RetryCountBeforeFailureNotification = @RetryLimit
				RAISERROR('Retry Count Before Failure Notification set to %d (@RetryLimit)', 0,0, @RetryLimit)
			END
			IF @RetryCountBeforeFailureNotification < 0
				SET @RetryCountBeforeFailureNotification = 0
		END
	END

	SET @SequenceSchedule = UTIL.ufn_CleanField(@SequenceSchedule)
	SET @Description = UTIL.ufn_CleanField(@Description)

	DECLARE @SequenceScheduleID int = null
	IF @SequenceSchedule is not null
	BEGIN
		SELECT @SequenceScheduleID = ScheduleID
		FROM SEIDR.Schedule
		WHERE Description = @SequenceSchedule
		AND Active = 1 
		--AND ForSequenceControl = 1
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Invalid Schedule for Sequence Control: %s', 16, 1, @SequenceSchedule)
			RETURN 80
		END
	END

	IF @TriggerExecutionNameSpace = 'SEIDR' AND @TriggerExecutionStatus = 'FF'
	BEGIN
		RAISERROR('Invalid Trigger ExecutionStatus - SEIDR.FF Cannot be used as a Trigger Execution Status.', 16, 1)
		RETURN
	END

	DECLARE @AutoMessage varchar(2000)
	DECLARE @Update bit = 0
	IF @Branch is null AND @TriggerBranch IS NOT NULL
	BEGIN
		RAISERROR('Shifting to Branch "%s" - cannot shift from specific branch to unspecified.', 0, 0, @TriggerBranch)
		SET @Branch = @TriggerBranch
	END
	ELSE IF @Branch is null
		SET @Branch = 'MAIN'



	IF @StepNumber is not null
	BEGIN

		IF EXISTS(SELECT null 
					FROM SEIDR.JobProfile_Job 
					WHERE JobProfileID = @JobProfileID 
					AND StepNumber = @StepNumber 
					AND Active = 1
					AND (@TriggerExecutionStatus is null AND TriggerExecutionStatusCode is null OR TriggerExecutionStatusCode = @TriggerExecutionStatus)
					AND (@TriggerExecutionNameSpace is null AND TriggerExecutionNameSpace is null OR TriggerExecutionNameSpace = @TriggerExecutionNameSpace)
					AND (@TriggerBranch is null AND TriggerBranch is null OR TriggerBranch = @TriggerBranch)
					AND Branch = @Branch				
					--AND (@PreviousJobProfile_JobID is null AND PreviousJobProfile_JobID is null OR @PreviousJobProfile_JobID = PreviousJobProfile_JobID)
					and JobID <> @JobID)
		BEGIN
			RAISERROR('Current step configuration is for a different job. Cannot continue.', 16, 1)
			RETURN
		END
		UPDATE SEIDR.JobProfile_Job
		SET @JobProfile_JobID = JobProfile_JobID,
			CanRetry = @CanRetry,
			RetryLimit = @RetryLimit,
			RetryDelay = @RetryDelay,
			RequiredThreadID = @ThreadID,
			FailureNotificationMail = @FailureNotificationMail,
			Description = NULLIF(COALESCE(@Description, Description), ''), --Only set to null if we pass ''
			SequenceScheduleID = @SequenceScheduleID,
			LastUpdate = GETDATE(),
			UpdatedBy = @UserName,
			RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
		WHERE JobProfileID = @JobProfileID 
		AND StepNumber = @StepNumber
		AND (@TriggerExecutionStatus is null AND TriggerExecutionStatusCode is null OR TriggerExecutionStatusCode = @TriggerExecutionStatus)
		AND (@TriggerExecutionNameSpace is null AND TriggerExecutionNameSpace is null OR TriggerExecutionNameSpace = @TriggerExecutionNameSpace)
		--AND (@PreviousJobProfile_JobID is null AND PreviousJobProfile_JobID is null OR @PreviousJobProfile_JobID = PreviousJobProfile_JobID)
		AND (@TriggerBranch is null AND TriggerBranch is null OR TriggerBranch = @TriggerBranch)
		AND Branch = @Branch
		AND JobID = @JobID
		AND Active = 1
		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO SEIDR.JobProfile_Job(JobProfileID, StepNumber, Description, JobID, CanRetry, RetryLimit, RetryDelay, 
				RequiredThreadID, TriggerExecutionStatusCode, TriggerExecutionNameSpace, FailureNotificationMail, SequenceScheduleID,
				CreatedBy, TriggerBranch, Branch, RetryCountBeforeFailureNotification)
			VALUES(@JobProfileID, @StepNumber, @Description, @JobID, @CanRetry, @RetryLimit, @RetryDelay,
				@ThreadID, @TriggerExecutionStatus, @TriggerExecutionNameSpace, @FailureNotificationMail, @SequenceScheduleID,
				@UserName, @TriggerBranch, @Branch, @RetryCountBeforeFailureNotification)
			
			SELECT @JobProfile_JobID = SCOPE_IDENTITY()			
		END		
		ELSE
			SET @update = 1

	END
	ELSE
	BEGIN		
		IF EXISTS(SELECT null 
					FROM SEIDR.JobProfile_Job WITH (NOLOCK)
					WHERE JobProfileID = @JobProfileID
					--AND StepNumber >=  @StepNumber - 2
					AND JobID = @JobID
					AND Description = @Description
					AND Active = 1)
		BEGIN
			SELECT * 
			FROM SEIDR.vw_JobProfile_Job WITH (NOLOCK)
			WHERE JobProfileID = @JobProfileID
			RAISERROR('Description/Job combination indicate that the procedure may have been rerun with the same parameters... Please specify @StepNumber explicitly.', 16, 1)
			RETURN
		END

		SELECT @StepNumber = ISNULL(MAX(StepNumber), 0) + 1
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @JobProfileID
		AND Active = 1

		
		INSERT INTO SEIDR.JobProfile_Job(JobProfileID, StepNumber, Description, JobID, CanRetry, RetryLimit, RetryDelay, FailureNotificationMail, SequenceScheduleID, CreatedBy,
		TriggerBranch, Branch, RetryCountBeforeFailureNotification)
		VALUES(@JobProfileID, @StepNumber, @Description, @JobID, @CanRetry, @RetryLimit, @RetryDelay, @FailureNotificationMail, @SequenceScheduleID, @UserName, 
		@TriggerBranch, @Branch, @RetryCountBeforeFailureNotification)
			
		SELECT @JobProfile_JobID = SCOPE_IDENTITY()	
	END
	IF @JobProfile_JobID IS NULL
	BEGIN
		RAISERROR('Could not identify step.', 16, 1)
		RETURN
	END

	IF @update = 1
		SET @AutoMessage = 'Update Step Number '
	ELSE 
		SET @AutoMessage = 'Add new Step at StepNumber '
	SET @AutoMessage += CONVERT(varchar(20), @StepNumber)

	SELECT @AutoMessage += ', Job "' 
				+ j.JobNameSpace + '.' + j.JobName
				+ '" - ' + @Description
	FROM SEIDR.Job j
	WHERE JobID = @JobID

	exec SEIDR.usp_JobProfileNote_i @JobProfileID, @AutoMessage, 1, @Author
END