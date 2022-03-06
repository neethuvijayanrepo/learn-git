CREATE PROCEDURE [SEIDR].[usp_JobProfile_EdiConversionJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	
	@CodePage int = null,
	@OutputFolder varchar(500) = null,
	@KeepOriginal bit = 1,
		
	@CanRetry bit = 1,
	@RetryLimit int = 50,
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
	SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '')
	IF @Description is null
	BEGIN
		SET @Description = 'EDI Conversion Job'
	END
	ELSE IF @Description NOT LIKE '%EDI%'
		SET @Description += ' - EDI Conversion Job'
	
	IF @OutputFolder LIKE '%#%'
		SET @OutputFolder = CONFIG.ufn_ShortHandPath_Profile(@OutputFolder, @JobProfileID)

	DECLARE @JobID int , @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'EDI Conversion Job' 
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
		@ThreadID=@ThreadID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
	
	--Possible toDo: Generic document table.
	IF EXISTS(SELECT null FROM SEIDR.EDIConversion WHERE JobProfile_JobID = @JobProfile_JobID )
	BEGIN
		UPDATE SEIDR.EDIConversion
		SET OutputFolder = @OutputFolder,
			KeepOriginal = @KeepOriginal,
			[CodePage] = @CodePage
		WHERE JobProfile_JobID = @JobProfile_JobID 		
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.EDIConversion(JobProfile_JobID, OutputFolder, KeepOriginal, [CodePage])
		VALUES(@JobProfile_JobID, @OutputFolder, @KeepOriginal, @CodePage)
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.EDIConversion
	WHERE JobProfile_JobID = @JobProfile_JobID
END