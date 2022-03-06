CREATE PROCEDURE [SEIDR].[usp_JobProfile_FTPJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	@FTPAccountID int = null,
	@FTPAccount varchar(256) = null,
	@FTPOperation varchar(40) = 'RECEIVE',
	
	@LocalPath varchar(500) = null,
	@RemotePath varchar(1000) = null,
	@RemoteTargetPath varchar(1000) = null,

	@Overwrite bit = 1, 
	@Delete bit = null,
	@DateFlag bit = NULL,

	@CanRetry bit = 1,
	@RetryLimit int = 500,
	@RetryDelay int = 15,
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null,
	
	@FailureNotificationMail varchar(500) = null,
	@SequenceSchedule varchar(300) = null,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@SafetyMode bit = 1,
	@RetryCountBeforeFailureNotification smallint = null
AS
BEGIN	
	SET XACT_ABORT ON
	IF @FTPAccountID is null AND 1 <> (SELECT COUNT(*) FROM SEIDR.FTPAccount WHERE Description = @FTPAccount AND DD IS NULL)
	BEGIN
		RAISERROR('Invalid @FTPAccountID/@FTPAccount: %d/%s', 16, 1, @FTPAccountID, @FTPAccount)
		RETURN 50
	END

	IF @DateFlag is null
	BEGIN
		IF @FTPOperation LIKE '%REGISTER'
			SET @DateFlag = 0
		ELSE 
			SET @DateFlag = 1
	END
	ELSE IF @FTPOperation NOT LIKE '%REGISTER' AND @DateFlag = 0 AND @SafetyMode = 1
	BEGIN
		RAISERROR('Really intend to pass DateFlag = 0 for operation %s? If so, pass @SafetyMode = 0.', 16, 1, @FTPOperation)
		RETURN 50
	END
	

	IF @FTPOperation = 'SEND' AND @Delete = 1
	BEGIN
		RAISERROR('OVERRIDDING @DELETE = 1 FOR "SEND" OPERATION', 1, 1)
		SET @Delete = 0
	END
	IF @RemotePath is null 
	BEGIN
		RAISERROR('@RemotePath is required.', 16, 1)
		RETURN
	END
	IF @FTPoperation = 'MOVE_REMOTE'
	BEGIN
		SET @Delete = 0
		SET @LocalPath = null
		IF @RemoteTargetPath is null
		BEGIN
			RAISERROR('@RemoteTargetPath is required for MOVE_REMOTE.', 16, 1)
			RETURN
		END
	END	
	ELSE IF @RemoteTargetPath is not null
	BEGIN
		RAISERROR('@RemoteTargetPath only applies to operation "MOVE_REMOTE".', 16, 1)
		RETURN
	END

	IF @FTPOperation = 'SYNC_REGISTER' AND (@StepNumber <> 1 
											OR @StepNumber is null 
											and EXISTS(SELECT null 
														FROM SEIDR.JobProfile_Job   WITH (NOLOCK)
														WHERE JobProfileID = @JobProfileID)
											)
	BEGIN
		RAISERROR('SYNC_REGISTER should be the first step of a profile.', 16, 0) WITH NOWAIT
		RETURN 50
	END
	IF @Delete is null 
	BEGIN
		IF @FTPOperation IN ('SYNC_REGISTER', 'SYNC_LOCAL')
		BEGIN
			SET @Delete = 1
			RAISERROR('%s - setting @Delete = 1 (delete extra files from local during sync process)', 0, 0, @FTPOperation)
		END
		ELSE
		BEGIN
			SET @Delete = 0
		END
	END

	IF @FTPAccountID IS NULL
	BEGIN
		SELECT @FTPAccountID = FTPAccountID
		FROM SEIDR.FTPAccount
		WHERE Description = @FTPAccount AND DD IS NULL
		IF @@ROWCOUNT <> 1
		BEGIN
			SELECT *
			FROM SEIDR.FTPAccount
			WHERE Description = @FTPAccount AND DD IS NULL

			RAISERROR('INVALID @FTPACCOUNT: %s', 16, 1, @FTPAccount)
			RETURN 50
		END
	END
	ELSE IF @FTPAccount IS NOT NULL
	BEGIN
		SELECT * FROM SEIDR.FTPAccount WHERE FTPAccountID = @FTPAccountID AND Description = @FTPAccount AND DD IS NULL
		IF @@ROWCOUNT <> 1
		BEGIN		
			RAISERROR('Invalid @FTPAccountID/@FTPAccount: %d/%s', 16, 1, @FTPAccountID, @FTPAccount)
			RETURN 50
		END
	END
	DECLARE @ProjectID smallint, @OrganizationID int

	IF @Description IS null
		SELECT @Description = Description + ' FTP - ' + @FTPOperation, @ProjectID = ProjectID, @OrganizationID = ISNULL(OrganizationID, 0)
		FROM SEIDR.JobProfile  WITH (NOLOCK)
		WHERE JobProfileID = @JobProfileID
	ELSE
		SELECT @ProjectID = ProjectID, @OrganizationID = ISNULL(OrganizationID, 0)
		FROM SEIDR.JobProfile WITH (NOLOCK)
		WHERE JobProfileID = @JobProfileID

	IF @OrganizationID < 0 AND @FTPOperation <> 'RECEIVE'
	BEGIN
		RAISERROR('Invalid OrganizationID for FTP Operation: %d', 16, 1, @OrganizationID)
		RETURN 70
	END

	IF exists(SELECT null
			FROM SEIDR.FTPAccount fa
			WHERE fa.FTPAccountID = @FTPAccountID
			AND (REFERENCE.ufn_Check_Project_Organization(fa.ProjectID, @OrganizationID) = 0
				OR REFERENCE.ufn_Check_Project_Organization(@ProjectID, fa.OrganizationID) = 0
				)
			)
	BEGIN
		RAISERROR('@FTPAccount Project/Organization Combination does not match with Profile', 16, 1)
		RETURN
	END
	

	IF @ProjectID is not null 
	AND EXISTS(SELECT null FROM SEIDR.FTPAccount WHERE FTPAccountID = @FTPAccountID AND ProjectID <> @ProjectID)
	BEGIN
		RAISERROR('@FTPaccountID does not match expected ProjectID: %d', 16, 1, @ProjectID)
		RETURN 70
	END

	IF 0 = (SELECT [REFERENCE].[ufn_CompareOrganization](ISNULL(OrganizationID, -1), @OrganizationID, 1) 
		FROM SEIDR.FTPAccount 
		WHERE FTPAccountID = @FTPAccountID)
	BEGIN
		RAISERROR('@FTPAccountID does not match Expected OrganizationID: %d', 16, 1, @OrganizationID)
		RETURN 70
	END
	ELSE IF @OrganizationID = 0 
	AND @OrganizationID <> (SELECT ISNULL(OrganizationID, -1) FROM SEIDR.FTPaccount WHERE FTPAccountID = @FTPAccountID)
	BEGIN
		RAISERROR('@Profile OrganizationID is 0, but FTPAccount is non zero.', 16, 1)
		RETURN 70
	END


	IF @LocalPath LIKE '%#%' 
	BEGIN
		RAISERROR('Replacing @LocalPath (%s) ', 0, 0, @LocalPath) WITH NOWAIT

		SELECT @LocalPath = CONFIG.ufn_ShortHandPath_Profile(@LocalPath, @JobProfileID)

		RAISERROR('Replaced @LocalPath: %s', 0, 0, @LocalPath) WITH NOWAIT
	END



	DECLARE @FTPOperationID int
	SELECT @FTPOperationID = FTPOperationID
	FROM SEIDR.FTPOperation
	WHERE OperationName = @FTPOperation
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Invalid FTP Operation: %s', 16, 1, @FTPOperation)
		RETURN 50
	END
	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FTPJob' 
	AND JobNameSpace = 'FTP'

	
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
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
		

	IF EXISTS(SELECT null FROM SEIDR.FTPJob WHERE JobProfile_JObID = @JobProfile_JobID AND Active = 1)
	BEGIN
		UPDATE SEIDR.FTPJob
		SET LocalPath = @LocalPath,
			RemotePath = @RemotePath,
			RemoteTargetPath = @RemoteTargetPath,
			Overwrite = @OverWrite,
			[Delete] = @Delete,
			DateFlag = @DateFlag,
			FTPOperationID = @FTPOperationID,
			FTPAccountID = @FTPAccountID
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND Active = 1
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FTPJob(JobProfile_JobID, LocalPath, RemotePath, RemoteTargetPath, 
			FTPoperationID, FTPAccountID,
			Overwrite, [Delete], DateFlag)
		VALUES(@JobProfile_JobID, @LocalPath, @RemotePath, @RemoteTargetPath,
			@FTPOperationID, @FTPAccountID,
			@Overwrite, @Delete, @DateFlag)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FTPJob
	WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
END