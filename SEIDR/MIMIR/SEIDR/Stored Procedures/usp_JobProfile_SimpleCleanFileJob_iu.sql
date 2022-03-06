CREATE PROCEDURE [SEIDR].[usp_JobProfile_SimpleCleanFileJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,	
	
	@Description varchar(100) = null,	
	@AddTrailer bit = 0,
	@BlockSize int = null,
	@KeepOriginal bit = 1,
	@Extension varchar(30) = 'CLN',
	@Line_MaxLength int = null,
	@Line_MinLength int = null,
	@CodePage int = null,
	@LineEnd_CR bit = 1,
	@LineEnd_LF bit = 1,
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null, 	
	@FailureNotificationMail varchar(500)= null,
	@SequenceSchedule varchar(300) = null,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
	SET XACT_ABORT ON
	IF @Description is null
		SET @Description = 'Simple File Cleaning'
	
	IF @Extension is null
	BEGIN
		RAISERROR('@Extension cannot be null.', 16, 1)
		RETURN
	END

	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'SimpleCleanJob' 
	AND JobNameSpace = 'FileSystem'
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Could not identify @JobID', 16, 1)
		RETURN
	END
	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = 0,
		@RetryLimit = 1, --Not nullable, but we don't care about it.
		@RetryDelay = null,
		@JobID = @JobID,
		@ThreadID=@ThreadID,
		@FailureNotificationMail = @FailureNotificationMail,
		@JobProfile_JobID = @JobProfile_JobID out,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN

	IF EXISTS(SELECT null FROM SEIDR.SimpleCleanFileJob WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		UPDATE SEIDR.SimpleCleanFileJob
		SET AddTrailer = @AddTrailer,
			Extension = @Extension,
			[CodePage] = @CodePage,
			[BlockSize] = @BlockSize,
			Line_MaxLength = @Line_MaxLength,
			Line_MinLength = @Line_MinLength,
			LineEnd_CR = @LineEnd_CR,
			LineEnd_LF = @LineEnd_LF,
			KeepOriginal = @KeepOriginal			
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.SimpleCleanFileJob(JobProfile_JobID, AddTrailer, [BlockSize], [CodePage], Extension, 
			Line_MaxLength, Line_MinLength, LineEnd_CR, LineEnd_LF, KeepOriginal)
		VALUES(@JobProfile_JobID, @AddTrailer, @BlockSize, @CodePage, @Extension,
			@Line_MaxLength, @Line_MinLength, @LineEnd_CR, @LineEnd_LF, @KeepOriginal)
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job 
	WHERE JobProfile_JobID = @JobProfile_JobID

	SELECT * 
	FROM SEIDR.SimpleCleanFileJob
	WHERE JobProfile_JobID = @JobProfile_JobID

RETURN 0
