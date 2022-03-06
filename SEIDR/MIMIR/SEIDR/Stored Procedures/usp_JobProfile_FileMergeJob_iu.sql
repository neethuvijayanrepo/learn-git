CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileMergeJob_iu]
	@JobProfileID int,
	
	@MergeFile varchar(500),
	@OutputFilePath varchar(500),
	@LeftKey1 varchar(128),


	@StepNumber tinyint = null,

	@Overwrite bit = 1,
	@InnerJoin bit = 1,

	@RemoveDuplicateColumns bit = 1,
	@RemoveExtraMergeColumns bit = 1,

	@CaseSensitive bit = 0,
	@PreSorted bit = 0,

	--@TextQualifier varchar(1) = '"',
	@HasTextQualifier bit = 1,
	@KeepDelimiter bit = 0, --Switch to |

	@LeftInputHasHeader bit = 1,
	@RightInputHasHeader bit = 1,
	@IncludeHeader bit = 1,

	@LeftKey2 varchar(128) = null,
	@LeftKey3 varchar(128) = null,
	
	@RightKey1 varchar(128) = null,
	@RightKey2 varchar(128) = null,
	@RightKey3 varchar(128) = null,	


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
	IF @MergeFile LIKE '%#%'
		SET @MergeFile = CONFIG.ufn_ShortHandPath_Profile(@MergeFile, @JobProfileID)
	ELSE IF @MergeFile is null
	BEGIN
		RAISERROR('Must provide a @MergeFile path to do the merge with.', 16, 1)
		RETURN
	END
	IF @OutputFilePath LIKE '%#%'
		SET @OutputFilePath = CONFIG.ufn_ShortHandPath_Profile(@OutputFilePath, @JobProfileID)
	ELSE IF @OutputFilePath is null
	BEGIN
		RAISERROR('Must provide an @OutputFilePath for writing the result to.', 16, 1)
		RETURN
	END

	IF @LeftKey1 is null
	BEGIN
		IF @LeftKey2 IS NOT NULL
		BEGIN
			SET @LeftKey1 = @LeftKey2
			SET @RightKey1 = @RightKey2
			SET @Leftkey2 = @LeftKey3
			SET @RightKey2 = @RightKey3
			SET @LeftKey3 = null
			SET @RightKey3 = null
			RAISERROR('Shifting Keys...', 0, 0)
		END
		ELSE IF @LeftKey3 IS NOT NULL
		BEGIN
			SET @LeftKey1 = @LeftKey3
			SET @LeftKey3 = null
			
			SET @RightKey1 = @RightKey3
			SET @RightKey3 = null

			RAISERROR('Shifting Keys (@LeftKey3 -> @LeftKey1)...', 0,0)
		END
		ELSE
		BEGIN
			RAISERROR('Must provide at least @LeftKey1 to be able to merge.', 16, 1)
			RETURN
		END
	END
	
	DECLARE @Description varchar(100) = 'File Merge with ' + UTIL.ufn_PathItem_GetName(@MergeFile)
	

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileMergeJob' 
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
	

	IF EXISTS(SELECT null FROM SEIDR.FileMergeJob WHERE JobProfile_JobID = @JobProfile_JobID )
	BEGIN
		UPDATE SEIDR.FileMergeJob
		SET 
			OutputFilePath = @OutputFilePath,
			MergeFile = @MergeFile,
			LeftKey1 = @LeftKey1,
			InnerJoin = @InnerJoin,
			Overwrite = @Overwrite,
			KeepDelimiter = @KeepDelimiter,
			HasTextQualifier = @HasTextQualifier,
			RemoveDuplicateColumns = @RemoveDuplicateColumns,
			RemoveExtraMergeColumns = @RemoveExtraMergeColumns,
			CaseSensitive = @CaseSensitive,
			PreSorted = @PreSorted,
			LeftInputHasHeader = @LeftInputHasHeader,
			RightInputHasHeader = @RightInputHasHeader,
			IncludeHeader = @IncludeHeader,
			LeftKey2 = @LeftKey2,
			LeftKey3 = @LeftKey3,
			RightKey1 = @rightKey1, 
			RightKey2 = @rightKey2,
			RightKey3 = @RightKey3			
		WHERE JobProfile_JobID = @JobProfile_JobID 
		
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FileMergeJob(JobProfile_JobID,OutputFilePath, MergeFile, InnerJoin,
			Overwrite, RemoveDuplicateColumns, RemoveExtraMergeColumns, CaseSensitive,
			PreSorted, LeftInputHasHeader, RightInputHasHeader, IncludeHeader,
			LeftKey1, LeftKey2, LeftKey3, RightKey1, RightKey2, RightKey3,
			KeepDelimiter, HasTextQualifier)
		VALUES(@JobProfile_JobID, @OutputFilePath, @MergeFile, @InnerJoin,
			@Overwrite, @RemoveDuplicateColumns, @RemoveExtraMergeColumns, @CaseSensitive,
			@PreSorted, @LeftInputHasHeader, @RightInputHasHeader, @IncludeHeader,
			@LeftKey1, @LeftKey2, @LeftKey3, @RightKey1, @RightKey2, @RightKey3,
			@KeepDelimiter, @HasTextQualifier)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FileMergeJob
	WHERE JobProfile_JobID = @JobProfile_JobID 
RETURN 0
