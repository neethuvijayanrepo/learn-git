CREATE PROCEDURE [SEIDR].[usp_JobProfile_SpawnJob_iu]
	@JobProfileID int,
	/*
		Specifies step number that this step is going to be run in for the profile. 
		
		If null, will take the next unused step, and always perform an insert.

		If not null, will also check if the step is in use first (based on StepNumber + TriggerExecutionStatus), and attempt to perform an update if so.
		Update will only go through if the existing step is for the same JobID
	*/
	@StepNumber tinyint = null,
	/*
		Child JobProfile - when the SpawnJob step runs, it will create a JobExecution for each profile record in the table. File information will be set depending on
		either the JobExecution (null sourceFile), or the specified SourceFile
	*/
	@TargetJobProfileID int = null,
	/*
		SourceFile: For use with @TargetJobProfileID - specifies what filepath to create new job executions with. Can be date masked.
		
		Note: If @SourceFile is null, then the JobExecution for the target profile will be created with whatever FilePath the JobExecution 
		is currently pointing to
	*/
	@SourceFile varchar(500) = null,	
	/*
		If 1, Set DD on SpawnJob configuration records for the JobProfile_JobID that are not included in parameter set. 
		If 0, only add missing records

		If we need to clean up configurations and want to be safe about only affecting this step, we can do the following:
		1. Call with an empty #SpawnJobConfig and @DeactivateMissing = 0 ( Temp table created in child scope does not persist to parent scope.
		2. Delete from the temp table anything that doesn't need to be active
		3. Call this procedure again but with @DeactivateMissing = 1
	*/
	@DeactivateMissing bit = 0, 
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null,	
	@FailureNotificationMail varchar(500) = null,
	@SequenceSchedule varchar(300) = null,
	@CanRetry bit = 1, -- No files found, or network issues
	@RetryLimit int = 30,
	@RetryDelay int = 20,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
BEGIN 

	IF @TargetJobProfileID IS NOT NULL
	BEGIN
		RAISERROR('Single Profile Mode.', 0, 0);
		IF REFERENCE.ufn_CompareProfile_Organization(@JobProfileID, @TargetJobProfileID) = 0
		BEGIN
			RAISERROR('Profile Organizations do not match.', 16, 1)
			RETURN
		END
		IF @SourceFile LIKE '%#%'
		BEGIN
			RAISERROR('Replacing @SourceFile (%s)', 0, 0, @SourceFile) WITH NOWAIT

			SELECT @SourceFile = CONFIG.ufn_ShortHandPath_Profile(@SourceFile, @JobProfileID)

			RAISERROR('Replaced @SourceFile: %s', 0, 0, @SourceFile) WITH NOWAIT
		END
	END
	ELSE IF OBJECT_ID('tempdb..#SpawnJobConfig') IS NULL
	BEGIN		
		RAISERROR('Multi Target Profile Mode: Please Populate #SpawnJobConfig with the following:
CREATE TABLE #SpawnJobConfig(ID int identity(1,1) Primary key, JobProfileID int, SourceFile varchar(500) COLLATE DATABASE_DEFAULT null ) 

Then Insert any JobProfiles and masked file paths to be used.', 16, 1);
		RETURN
	END
	ELSE
	BEGIN
		RAISERROR('Multi Profile Mode. Ignoring @SourceFile, using #SpawnJobConfig.', 0, 0);
		IF EXISTS(SELECT null 
					FROM #SpawnJobConfig
					WHERE REFERENCE.ufn_CompareProfile_Organization(@JobProfileID, JobProfileID) = 0)
		BEGIN
			SELECT null 
			FROM #SpawnJobConfig
			WHERE REFERENCE.ufn_CompareProfile_Organization(@JobProfileID, JobProfileID) = 0
			RAISERROR('Profile Organizations do not match in selection.', 16, 1)
			RETURN
		END

		IF EXISTS(SELECT null FROM #SpawnJobConfig WHERE SourceFile LIKE '%#%')
		BEGIN
			UPDATE c
			SET SourceFile = CONFIG.ufn_ShortHandPath_Profile(@SourceFile, @JobProfileID)
			FROM #SpawnJobConfig c
			WHERE SourceFile LIKE '%#%'			
		END


	END
	IF OBJECT_ID('tempdb..#SpawnJobConfig') is null
		CREATE TABLE #SpawnJobConfig(ID int identity(1,1) Primary key, JobProfileID int, SourceFile varchar(500) COLLATE DATABASE_DEFAULT null ) --for VS checks

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'SpawnJob' 
	--AND JobNameSpace = 'FileSystem' --
	
	IF @StepNumber is null
	AND @JobID = (SELECT TOP 1 JobID FROM SEIDR.JobProfile_Job WHERE JobProfileID = @JobProfileID and Active = 1 ORDER BY stepNumber desc)
	BEGIN		
		SELECT TOP 1 @StepNumber = StepNumber
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @JobProfileID
		AND Active = 1
		AND JobID = @JobID
		ORDER BY stepNumber desc
		IF @@ROWCOUNT > 0
			RAISERROR('No StepNumber specified, set to %d - current last step is already SpawnJob.', 0, 0, @StepNumber)
	END
	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = 'Spawn Job Executions for Linked Profile(s)',
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
		@Branch = @Branch,
		@TriggerBranch = @TriggerBranch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification

	IF @@ERROR <> 0
	BEGIN
		RETURN
	END

	IF @TargetJobProfileID IS NOT NULL
	BEGIN
		IF @DeactivateMissing = 1
		BEGIN
			UPDATE SEIDR.SpawnJob
			SET DD = GETDATE()
			WHERE JObProfile_JobID = @JobProfile_JobID
			AND Active = 1 AND (JobProfileID <> @TargetJobProfileID OR ISNULL(SourceFile, '') <> ISNULL(@SourceFile, ''))
		END

		IF NOT EXISTS(SELECT null 
						FROM SEIDR.SpawnJob 
						WHERE JobProfile_JobID = @JobProfile_JobID 
						AND JobProfileID = @TargetJObProfileID
						AND ISNULL(SourceFile,'') = ISNULL(@SourceFile, '') 
					)
		BEGIN
			INSERT INTO SEIDR.SpawnJob(JobProfile_JobID, JobProfileID, SourceFile)
			VALUES(@JobProfile_JobID, @TargetJobProfileID, @SourceFile)
		END 
		ELSE
		BEGIN
			UPDATE SEIDR.SpawnJob
			SET DD = null--, SourceFile = @SourceFile
			WHERE Jobprofile_JobID = @JobProfile_JobID 
			AND JobProfileID = @TargetJobProfileID
			AND ISNULL(SourceFile,'') = ISNULL(@SourceFile, '')
		END
		
		/*
		INSERT INTO #SpawnJobConfig(JobProfileID, sourceFile)
		SELECT JobProfileID, SourceFile
		FROM SEIDR.SpawnJob 
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND Active = 1
		*/
	END
	ELSE
	BEGIN
		IF @DeactivateMissing = 1
		BEGIN
			UPDATE sp
			SET DD = GETDATE()
			FROM SEIDR.SpawnJob sp
			WHERE JoBProfile_JobID = @JobProfile_JobID
			AND Active = 1
			AND NOT EXISTS(SELECT null 
							FROM #SpawnJobConfig
							WHERE JobProfileID = sp.JobProfileID
							AND ISNULL(SourceFile, '') = ISNULL(sp.SourceFile, '')
							)			
		END

		UPDATE j
		SET DD = null --, SourceFile = t.SourceFile
		FROM SEIDR.SpawnJob j
		JOIN #SpawnJobConfig t
			ON j.JobProfileID = t.JobProfileID
			AND ISNULL(j.SourceFile,'') = ISNULL(t.SourceFile, '')
		WHERE j.JobProfile_JobID = @JobProfile_JobID

		INSERT INTO SEIDR.SpawnJob(JobProfile_JobID, JobProfileID, SourceFile)
		SELECT @JobProfile_JobID, JobProfileID, SourceFile
		FROM #SpawnJobConfig c
		WHERE NOT EXISTS(SELECT null 
						FROM SEIDR.SpawnJob 
						WHERE JobProfile_JobID = @JobProfile_JobID 
						AND JobProfileID = c.JobprofileID
						AND ISNULL(SourceFile,'') = ISNULL(c.SourceFile, '')
						)

		
		INSERT INTO #SpawnJobConfig(JobProfileID, sourceFile)
		SELECT JobProfileID, SourceFile
		FROM SEIDR.SpawnJob sp
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND Active = 1
		AND NOT EXISTS(SELECT null 
						FROM #SpawnJobConfig
						WHERE JobProfileID = sp.JobProfileID
						AND ISNULL(SourceFile, '') = ISNULL(sp.SourceFile, '')
						)
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job
	WHERE JobProfile_JobID = @JobProfile_JObID

	SELECT * FROM SEIDR.SpawnJob WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1

	IF @TargetJobProfileID is null
		SELECT JobProfileID [#JobProfileID], SourceFile [#SourceFile] 
		FROM #SpawnJobConfig
END