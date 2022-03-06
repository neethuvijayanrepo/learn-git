CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileSizeCheckJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	--@Description varchar(100) = null,
	
	@DaysBack tinyint = null,
	@CheckLargeFiles bit = null,
	@EmptyFileSize int = null,
	@StandardDeviationMultiplier float = null,
	@IgnoreSunday bit = null,
	@IgnoreEmptyFileSunday bit = null,
	@IgnoreMonday bit = null,
	@IgnoreEmptyFileMonday bit = null,
	@IgnoreTuesday bit = null,
	@IgnoreEmptyFileTuesday bit = null,
	@IgnoreWednesday bit = null,
	@IgnoreEmptyFileWednesday bit = null,
	@IgnoreThursday bit = null,
	@ignoreEmptyFileThursday bit = null,
	@IgnoreFriday bit = null,
	@IgnoreEmptyFileFriday bit = null,
	@IgnoreSaturday bit = null,
	@IgnoreEmptyFileSaturday bit = null,
	--@CanRetry bit = null --NOTE: Must manually override when the size check fails, so do not allow setting to 1. Do not include as a parameter.

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
	SET XACT_ABORT ON;
	SET NOCOUNT ON
	--SET @Description = UTIL.ufn_CleanField(@Description)
	
	
	DECLARE @MaxDaysBack int = 730,--Give a couple years back to start
			@DefaultDaysBack int = 200
	IF @DaysBack is null
	BEGIN
		RAISERROR('Auto Set @DaysBack = %d', 0, 0, @DefaultDaysBack)
		SET @DaysBack = @DefaultDaysBack
	END
	ELSE IF @DaysBack NOT BETWEEN 1 AND @MaxDaysBack
	BEGIN
		RAISERROR('Invalid @DaysBack: %d - must be between 1 and %d', 16, 1, @DaysBack, @MaxDaysBack)
		RETURN
	END


	IF @emptyFileSize < 0
	BEGIN
		SET @EmptyFileSize = null
	END

	IF @StandardDeviationMultiplier IS NOT NULL
	AND (@StandardDeviationMultiplier <= 0 OR @StandardDeviationMultiplier >= 100)
	BEGIN	
		DECLARE @flt varchar(60) = CONVERT(varchar(60), @StandardDeviationMultiplier)
		RAISERROR('@StandardDeviationMultiplier(%s) must be greater than 0 and less than 100.', 16, 1, @flt)
		RETURN
	END

	DECLARE @Description varchar(100)
	IF @StandardDeviationMultiplier > 4
	BEGIN		
		DECLARE @percent int = CONVERT(int, @StandardDeviationMultiplier)
		SET @Description = 'File Size Check by Percentage(' + CONVERT(varchar(10), @percent) + '%) Threshold - ' + CONVERT(varchar(30), @DaysBack) + ' Days Back' 		
		SET @StandardDeviationMultiplier = @percent
		RAISERROR('PERCENTAGE MODE: %d%%', 0, 0)
	END
	ELSE
	BEGIN
		DECLARE @fltMsg varchar(10) = CONVERT(varchar(10), @StandardDeviationMultiplier)
		IF @StandardDeviationMultiplier = CONVERT(int, @StandardDeviationMultiplier)
		BEGIN
			SET @fltMsg = 'x' + @fltMsg + '.0'
		END
		ELSE
			SET @fltMsg = 'x' + @fltMsg
		SET @Description = 'File Size Check by Standard Deviation(' + @fltMsg + ') - ' + CONVERT(varchar(30), @DaysBack) + ' Days Back' 
		RAISERROR('STANDARD DEVIATION MODE: %s', 0, 0, @fltMsg)
	END



	DECLARE @JobID int, @JobProfile_JobID int

	SELECT @JobID = JobID
	FROM SEIDR.Job
	WHERE JobName = 'FileSizeCheckJob'
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
		@ThreadID=@ThreadID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch, 
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0 
		RETURN
	
	IF @IgnoreSunday = 1 AND @IgnoreEmptyFileSunday is null
		SET @IgnoreEmptyFileSunday = 1
	IF @IgnoreMonday = 1 AND @IgnoreEmptyFileMonday is null
		SET @IgnoreEmptyFileMonday = 1
	IF @IgnoreTuesday = 1 AND @IgnoreEmptyFileTuesday is null
		SET @IgnoreEmptyFileTuesday = 1
	IF @IgnoreWednesday = 1 AND @IgnoreEmptyFileWednesday IS NULL
		SET @ignoreEmptyFileWednesday = 1
	IF @IgnoreThursday = 1 AND @IgnoreEmptyFileThursday is null
		SET @ignoreEmptyFileThursday = 1
	IF @IgnoreFriday = 1 AND @IgnoreEmptyFileFriday is null
		SET @ignoreEmptyFileFriday = 1
	IF @IgnoreSaturday = 1 AND @IgnoreEmptyFileSaturday is null
		SET @IgnoreEmptyFileSaturday = 1


	DECLARE @FileSizeCheckJobID int
	-- Possible ToDo: Add a HolidayScheduleID to Project or Organization? Then can add a bit to skip days that match the holiday schedule. 
	-- Should be okay to manually deal with holiday exceptions until something like that is done, though.
	IF NOT EXISTS(SELECT null FROM SEIDR.FileSizeCheckJob WHERE JobProfile_jobID = @JobProfile_JobID)
	BEGIN
		INSERT INTO SEIDR.FileSizeCheckJob(JobProfile_JobID,
			DaysBack, 
			EmptyFileSize, 
			StandardDeviationMultiplier, 
			CheckLargeFiles,
			IgnoreSunday, IgnoreEmptyFileSunday,
			IgnoreMonday, IgnoreEmptyFileMonday,
			IgnoreTuesday, IgnoreEmptyFileTuesday,
			IgnoreWednesday, IgnoreEmptyFileWednesday,
			IgnoreThursday, IgnoreEmptyFileThursday,
			IgnoreFriday, IgnoreEmptyFileFriday,
			IgnoreSaturday, IgnoreEmptyFileSaturday)
		VALUES(@JobProfile_JobID,
			ISNULL(@DaysBack, 30), 
			ISNULL(@EmptyFileSize, 0), 
			ISNULL(@StandardDeviationMultiplier, 1), 
			ISNULL(@CheckLargeFiles, 0),
			ISNULL(@IgnoreSunday, 0),		ISNULL(@IgnoreEmptyFileSunday, 0),
			ISNULL(@IgnoreMonday, 0),		ISNULL(@IgnoreEmptyFileMonday, 0),
			ISNULL(@IgnoreTuesday, 0),		ISNULL(@IgnoreEmptyFileTuesday, 0),
			ISNULL(@IgnoreWednesday, 0),	ISNULL(@IgnoreEmptyFileWednesday, 0),
			ISNULL(@IgnoreThursday, 0),		ISNULL(@IgnoreEmptyFileThursday, 0),
			ISNULL(@IgnoreFriday, 0),		ISNULL(@IgnoreEmptyFileFriday, 0),
			ISNULL(@IgnoreSaturday, 0),		ISNULL(@IgnoreEmptyFileSaturday, 0))
		
		SELECT @FileSizeCheckJobID = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT @FileSizeCheckJobID = FileSizeCheckJobID
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID

		UPDATE SEIDR.FileSizeCheckJob
		SET EmptyFileSize = ISNULL(@EmptyFileSize, EmptyFileSize),
			DaysBack = ISNULL(@DaysBack, DaysBack),
			StandardDeviationMultiplier = ISNULL(@StandardDeviationmultiplier, StandardDeviationMultiplier),
			CheckLargeFiles = ISNULL(@CheckLargeFiles, CheckLargeFiles),
			IgnoreSunday = ISNULL(@IgnoreSunday, IgnoreSunday),
			IgnoreMonday = ISNULL(@ignoreMonday, IgnoreMonday),
			IgnoreTuesday = ISNULL(@IgnoreTuesday, IgnoreTuesday),
			IgnoreWednesday = ISNULL(@IgnoreWednesday, IgnoreWednesday),
			IgnoreThursday = ISNULL(@IgnoreThursday, IgnoreThursday),
			IgnoreFriday = ISNULL(@IgnoreFriday, IgnoreFriday),
			IgnoreSaturday = ISNULL(@IgnoreSaturday, IgnoreSaturday),
			
			IgnoreEmptyFileSunday = ISNULL(@IgnoreEmptyFileSunday, IgnoreEmptyFileSunday),
			IgnoreEmptyFileMonday = ISNULL(@IgnoreEmptyFileMonday, IgnoreEmptyFileMonday),
			IgnoreEmptyFileTuesday = ISNULL(@IgnoreEmptyFileTuesday, IgnoreEmptyFileTuesday),
			IgnoreEmptyFileWednesday = ISNULL(@IgnoreEmptyFileWednesday, IgnoreEmptyFileWednesday),
			IgnoreEmptyFileThursday = ISNULL(@IgnoreEmptyFileThursday, IgnoreEmptyFileThursday),
			IgnoreEmptyFileFriday = ISNULL(@IgnoreEmptyFileFriday, IgnoreEmptyFileFriday),
			IgnoreEmptyFileSaturday = ISNULL(@IgnoreEmptyFileSaturday, IgnoreEmptyFileSaturday),
			LU = GETDATE(),
			Editor = SUSER_NAME()
		WHERE FileSizeCheckJobID = @FileSizeCheckJobID
	END


	SELECT @Description [Description], * 
	FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
	WHERE FileSizeCheckJobID = @FileSizeCheckJobID
	
		
	RETURN 0	
END
