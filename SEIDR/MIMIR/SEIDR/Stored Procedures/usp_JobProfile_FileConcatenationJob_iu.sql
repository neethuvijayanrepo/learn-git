CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileConcatenationJob_iu]
	@JobProfileID int,
	@OutputPath varchar(500),
	@SecondaryFile varchar(500),
	@HasHeader bit = 1,
	@SecondaryFileHasHeader bit = 1,
	@StepNumber tinyint = null,		
	@Description varchar(100) = null,
	

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
	SET XACT_ABORT ON
	SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '')
	IF @Description is null
	BEGIN
		SET @Description = 'File Merge - Concatenation'
	END
	IF NULLIF(LTRIM(RTRIM(@OutputPath)), '') Is null
	BEGIN
		RAISERROR('Must Specify @OutputPath.', 16, 1)
		RETURN
	END
	
	IF NULLIF(LTRIM(RTRIM(@SecondaryFile)), '') Is null
	BEGIN
		RAISERROR('Must Specify @SecondaryFile.', 16, 1)
		RETURN
	END
	IF @HasHeader is null OR @SecondaryFileHasHeader is null
	BEGIN
		RAISERROR('@HasHeader and @SecondaryFileHasHeader must both be non-null.', 16, 1)
		RETURN
	END

	IF @OutputPath LIKE '%#%' OR @SecondaryFile LIKE '%#%'
	BEGIN
		SET @outputPath = CONFIG.ufn_ShortHandPath_Profile(@OutputPath, @JobProfileID)
		SET @SecondaryFile = CONFIG.ufn_ShortHandPath_Profile(@SecondaryFile, @JobProfileID)
	END	

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileConcatenationJob' 
	AND JobNameSpace = 'FileSystem'

	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = 0,
		@RetryLimit = null,
		@RetryDelay = null,
		@JobID = @JobID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@ThreadID = @ThreadID,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
	
	IF EXISTS(SELECT null FROM SEIDR.LoaderJob WHERE JobProfile_JObID = @JobProfile_JobID AND DD IS NULL)
	BEGIN
		UPDATE SEIDR.FileConcatenationJob
		SET HasHeader = @HasHeader,
			OutputPath = @OutputPath,
			SecondaryFile = @SecondaryFile,
			SecondaryFileHasHeader = @SecondaryFileHasHeader
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FileConcatenationJob(JobProfile_JobID, HasHeader, OutputPath, SecondaryFile, SecondaryFileHasHeader)
		VALUES(@JobProfile_JobID, @HasHeader, @OutputPath, @SecondaryFile, @SecondaryFileHasHeader)	
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FileConcatenationJob
	WHERE JobProfile_JobID = @JobProfile_JobID 
	
END