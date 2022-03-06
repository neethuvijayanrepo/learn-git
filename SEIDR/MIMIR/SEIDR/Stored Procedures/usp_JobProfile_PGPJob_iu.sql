CREATE PROCEDURE [SEIDR].[usp_JobProfile_PGPJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	
	@SourcePath varchar(500) = null,
	@OutputPath varchar(500) = null,
	@PublicKeyFile varchar(500) = null,
	@PrivateKeyFile varchar(500) = null,
	@KeyIdentity varchar(500) = null,
	@PassPhrase varchar(500) = null,
	@PGPOperation varchar(15) = 'HELP',
		
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
	SET @PGPOperation = UPPER(@PgpOperation)

	DECLARE @PGPOperationID int
	SELECT @PgpOperationID = PGPOperationID
	FROM SEIDR.PGPOperation
	WHERE PGPOperationName = @PGPOperation
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT *, 
		CASE WHEN PgpOperationName IN('SIGN', 'DECRYPT') then 0 else 1 end [RequirePublicKeyFile],
		CASE WHEN PgpOperationName = 'ENCRYPT' then 0 else 1 end [RequirePrivateKeyFile]				
		FROM SEIDR.PGPOperation
		IF @PGPOperation <> 'HELP'
			RAISERROR('Invalid PGPOperation: %s', 16, 1, @PGPOperation)
		RETURN
	END
	SET @PublicKeyFile = NULLIF(LTRIM(RTRIM(@PublicKeyFile)), '')
	SET @PrivateKeyFile = NULLIF(LTRIM(RTRIM(@PrivateKeyFile)), '')
	
	--SELECT * FROM SEIDR.PgpOperation
	IF @PGPOperation = 'GenerateKey' AND (@PublicKeyFile IS NULL OR @PrivateKeyFile is null)
	BEGIN
		RAISERROR('Key Generation requires @PublicKeyFile and @PrivateKeyFile paths - This operation is for CREATING a pair of key files. The public key is for encrypting, while the private key is for signing and decrypting, and should generally not be shared.', 16, 1)
		RETURN
	END
	ELSE IF @PGPOperation LIKE '%ENCRYPT' AND @PublicKeyFile is null
	BEGIN
		RAISERROR('%s requires @PublicKeyFile', 16, 1, @PGPOperation)
		RETURN
	END
	ELSE IF (@PgpOperation = 'Decrypt' OR @PgpOperation LIKE 'Sign%') AND @PrivateKeyFile is null
	BEGIN
		RAISERROR('%s requires @PublicKeyFile', 16, 1, @PgpOperation)
		RETURN
	END

	IF @Description is null
	BEGIN
		SET @Description = 'PGP ' + @PGPOperation
	END
	
	IF @SourcePath LIKE '%#%' OR @OutputPath LIKE '%#%'
	BEGIN
		RAISERROR('Expanding ShortHand Paths for PGP. @Source: %s, @Output: %s', 0, 0, @SourcePath, @OutputPath)
		SELECT @SourcePath = CONFIG.ufn_ShortHandPath_Profile(@SourcePath, @JobProfileID),
				@OutputPath = CONFIG.ufn_ShortHandPath_Profile(@outputPath, @JobProfileID)
		RAISERROR('Expanded ShortHand Paths for PGP. @Source: %s, @Output: %s', 0, 0, @SourcePath, @OutputPath)
	END


	
	DECLARE @JobID int , @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'PGPJob' 
	AND JobNameSpace = 'PGP'

	
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
	

	IF EXISTS(SELECT null FROM SEIDR.PGPJob WHERE JobProfile_JObID = @JobProfile_JobID AND Active = 1)
	BEGIN
		UPDATE SEIDR.PGPJob
		SET SourcePath = @SourcePath,
			OutputPath = @OutputPath,			
			PGPOperationID = @PGPOperationID,
			PublicKeyFile = @PublicKeyFile,
			PrivateKeyFile = @PrivateKeyFile,
			KeyIdentity = @KeyIdentity,
			PassPhrase = @PassPhrase
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND Active = 1
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.PGPJob(JobProfile_JobID, SourcePath, OutputPath, PGPOperationID,
			PublicKeyFile, PrivateKeyFile, KeyIdentity, PassPhrase)			
		VALUES(@JobProfile_JobID, @SourcePath, @OutputPath,@PgpOperationID,
			@PublickeyFile, @PrivateKeyFile, @KeyIdentity, @PassPhrase)
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.PGPJob
	WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
END