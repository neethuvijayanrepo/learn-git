
CREATE PROCEDURE [SEIDR].[usp_JobProfile_FixWidthConversionJob_iu]
	@JobProfileID int,
	@SettingsFilePath varchar(500),	
	@StepNumber tinyint = null,		
	@Description varchar(100) = null,
	@CanRetry bit = 1,
	@RetryLimit int = 18,
	@RetryDelay int = 10,
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
	SET @SettingsFilePath = UTIL.ufn_CleanField(@SettingsFilePath)
	IF @SettingsFilePath IS NULL
	BEGIN
		RAISERROR('Must provide Settings FilePath.', 16, 1)
		RETURN
	END
	IF @SettingsFilePath LIKE '%#%'
	BEGIN
		RAISERROR('Applying ShortHand to OutputFolder...', 0, 0)
		SET @SettingsFilePath = CONFIG.ufn_ShortHandPath_Profile(@SettingsFilePath, @JobProfileID)		
	END
	
	SET @Description = UTIL.ufn_CleanField(@Description)
	IF @Description is null
		SET @Description = 'Fix Width Conversion Job'

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FixWidthConversionJob' 
	AND JobNameSpace = 'FileSystem'

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
		@JobProfile_JobID = @JobProfile_JobID out,
		@ThreadID = @ThreadID,
		@FailureNotificationMail  = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
	
	IF EXISTS(SELECT null FROM SEIDR.LoaderJob WHERE JobProfile_JObID = @JobProfile_JobID AND DD IS NULL)
	BEGIN
		UPDATE [SEIDR].[JobProfile_Job_SettingsFile]
		SET SettingsFilePath = @SettingsFilePath
		WHERE JobProfile_JobID = @JobProfile_JobID 
		
	END
	ELSE
	BEGIN
		INSERT INTO [SEIDR].[JobProfile_Job_SettingsFile](JobProfile_JobID, SettingsFilePath)
		VALUES(@JobProfile_JobID, @SettingsFilePath)
	END

	
	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM [SEIDR].[JobProfile_Job_SettingsFile]
	WHERE JobProfile_JobID = @JobProfile_JobID 

END