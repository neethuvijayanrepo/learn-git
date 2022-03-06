CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileValidationJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@NotificationList varchar(300) = null,
	@HasHeader bit,
	@MinimumColumnCountForMerge int = 0,
	@Description varchar(100) = Null,
	@DoMetaDataConfiguration bit = null,
	@HasTrailer bit = 0,
	@SkipLines int = 0,
	@RemoveTextQual bit = 0,
	@SizeThresholdWarningMode bit = 0,
	@TextQualifyColumnNumber int = null,
	@TextQualifier varchar(5) = '',
	@Delimiter varchar(1) = '|',
	@SizeThreshold int = null,
	@SizeThresholdDayRange tinyint = null,
	@KeepOriginal bit = 1,
	@OverrideExtension varchar(10) = null,
	@LineEnd_CR bit = 1,
	@LineEnd_LF bit = 1,
	@CanRetry bit = 0,
	@RetryLimit int = null,
	@RetryDelay int = 30,
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null,
	
	@FailureNotificationMail varchar(500) = null,
	@SequenceSchedule varchar(300) = null,
	--@JobProfile_JobID int = null output,
	--@PreviousJobProfile_JobID  int = null
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
BEGIN
	SET XACT_ABORT ON
	--SET @TextQualifier = [UTIL].[ufn_CleanField](@TextQualifier)
	IF @TextQualifyColumnNumber is not null and @TextQualifier is null
	BEGIN
		RAISERROR('Invalid Text Qualifier when @TextQualifyColumnNumber is not null.', 16, 1)
		RETURN
	END
	IF @SizeThreshold is not null and @SizeThresholdDayRange is null
		SET @SizeThresholdDayRange = 45
	IF @LineEnd_CR = 0 AND @LineEnd_LF = 0
	BEGIN
		RAISERROR('A line ending must be specified. For CRLF ending, @LineEnd_CR and @LineEnd_LF should both be set to 1.', 16, 1)
		RETURN
	END 
	SET @NotificationList = [UTIL].[ufn_CleanField](@NotificationList)
	IF @SizeThreshold is not null AND @NotificationList is null
	BEGIN
		RAISERROR('Notification list required with a @SizeThreshold.', 16, 1)
		RETURN
	END

	SET @OverrideExtension = [UTIL].[ufn_CleanField](@OverrideExtension)
	IF LEFT(@OverrideExtension, 1) = '.'
	BEGIN
		RAISERROR('Removing leading "." from override extension...', 0, 0)
		SET @OverrideExtension = SUBSTRING(@OverrideExtension, 2, LEN(@OverrideExtension))
		print @OverrideExtension
	END

	SET @Description = [UTIL].[ufn_CleanField](@Description)
	IF @Description is null
		sET @Description = 'File Cleaning/Validation'

	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileValidationJob' 
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
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@JobProfile_JobID = @JobProfile_JobID out,
		--@PreviousJobProfile_JobID = @PreviousJobProfile_JobID
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification

	IF @@ERROR <> 0
		RETURN
	
	IF EXISTS(SELECT null FROM SEIDR.FileValidationJob WHERE JobProfile_JObID = @JobProfile_JobID)
	BEGIN
		UPDATE SEIDR.FileValidationJob
		SET SkipLines = @SkipLines,
			HasHeader = @HasHeader,
			TextQualifier = @TextQualifier,
			DoMetaDataConfiguration = ISNULL(@DoMetaDataConfiguration, DoMetaDataConfiguration),
			Delimiter = @Delimiter,
			SizeThreshold = @SizeThreshold,
			SizeThresholdDayRange = @SizeThresholdDayRange,
			NotificationList = @NotificationList,
			HasTrailer = @HasTrailer,
			RemoveTextQual = @RemoveTextQual,
			SizeThresholdWarningMode = @SizeThresholdWarningMode,
			TextQualifyColumnNumber = @TextQualifyColumnNumber,
			MinimumColumnCountForMerge = @MinimumColumnCountForMerge,
			KeepOriginal = @KeepOriginal,
			OverrideExtension = @OverrideExtension,
			LineEnd_CR = @LineEnd_CR,
			LineEnd_LF = @LineEnd_LF
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FileValidationJob(JobProfile_JobID, SkipLines, HasHeader,TextQualifier,
			DoMetaDataConfiguration, Delimiter, SizeThreshold, SizeThresholdDayRange,
			NotificationList, HasTrailer, RemoveTextQual,
			SizeThresholdWarningMode, textQualifyColumnNumber, MinimumColumnCountForMerge,
			KeepOriginal, OverrideExtension, LineEnd_CR, LineEnd_LF)
		VALUES(@JobProfile_JobID,@SkipLines, @HasHeader, @TextQualifier,
			1, @Delimiter, @SizeThreshold, @SizeThresholdDayRange,
			@NotificationList, @HasTrailer, @RemoveTextQual,
			@SizeThresholdWarningMode, @TextQualifyColumnNumber, @MinimumColumnCountForMerge,
			@KeepOriginal, @OverrideExtension, @LineEnd_CR, @LineEnd_LF)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FileValidationJob
	WHERE JobProfile_JobID = @JobProfile_JobID
END