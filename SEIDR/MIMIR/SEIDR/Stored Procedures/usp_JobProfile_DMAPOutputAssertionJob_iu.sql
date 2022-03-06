CREATE PROCEDURE [SEIDR].[usp_JobProfile_DMAPOutputAssertionJob_iu]	
	@JobProfileID int,
	
	@ExpectedOutputFile varchar(500),

	@StepNumber tinyint = null,

	@CheckColumnNameMatch bit = 0,
	@CheckColumnOrderMatch bit = 0,

	@SkipColumns varchar(4000) = null,

	@CanRetry bit = null,
	@RetryLimit int = 50,
	@RetryDelay int = null,
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null,
	
	@FailureNotificationMail varchar(500) = null,
	@SequenceSchedule varchar(300) = null,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
BEGIN
	IF @ExpectedOutputFile LIKE '%#%'
		SET @ExpectedOutputFile = CONFIG.ufn_ShortHandPath_Profile(@ExpectedOutputFile, @JobProfileID)
	ELSE IF @ExpectedOutputFile is null
	BEGIN
		RAISERROR('Must provide an @ExpectedOutputFile for comparing against.', 16, 1)
		RETURN
	END

	
	DECLARE @Description varchar(100) = 'DMAP File Output Assertion with ' + UTIL.ufn_PathItem_GetName(@ExpectedOutputFile)
	

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'DMAPOutputAssertionJob' 
	AND JobNameSpace = 'DemoMap'

	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = @CanRetry,
		@RetryLimit = @RetryLimit,
		@RetryDelay = @RetryDelay,
		@JobID = @JobID,
		@ThreadID=@ThreadID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0 
		RETURN
	

	IF EXISTS(SELECT null FROM SEIDR.FileAssertionTestJob WHERE JobProfile_JobID = @JobProfile_JobID )
	BEGIN
		UPDATE SEIDR.FileAssertionTestJob
		SET 
			ExpectedOutputFile = @ExpectedOutputFile,
			CheckColumnNameMatch = @CheckColumnNameMatch,
			CheckColumnOrderMatch = @CheckColumnOrderMatch,
			SkipColumns = @SkipColumns
		WHERE JobProfile_JobID = @JobProfile_JobID 
		
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FileAssertionTestJob(JobProfile_JobID, ExpectedOutputFile,	CheckColumnNameMatch, CheckColumnOrderMatch, SkipColumns)
		VALUES(@JobProfile_JobID, @ExpectedOutputFile,	@CheckColumnNameMatch, @CheckColumnOrderMatch, @SkipColumns)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FileAssertionTestJob
	WHERE JobProfile_JobID = @JobProfile_JobID 
	RETURN 0
END