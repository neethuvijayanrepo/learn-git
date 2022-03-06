CREATE PROCEDURE [SEIDR].[usp_JobProfile_StagingToAndromedaExportStatusJob_iu]	
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,

	@CheckOrganization bit = 1,
	@CheckProject bit = 1,

	@IgnoreProcessingDate bit = 0,
	@RequireCurrentProcessingDate bit = 0,
	@MonitoredOnly bit = 1,
	@IgnoreUnusedProfiles bit = 1,

	@LoadBatchTypeList varchar(300),

	@DatabaseLookupID int = null,
	@DatabaseLookup varchar(50) = 'METRIX_STAGING',
	
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
	
	SET XACT_ABORT ON	   
	DECLARE @METRIX_OPERATION bit = 0
 
	IF @DatabaseLookupID is null
	BEGIN
		SELECT @DatabaseLookupID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE Description = @DatabaseLookup
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('DatabaseLookUp/DatabaseLookupID required. "%s" did not identify a valid LookupID.', 16, 1, @DatabaseLookup)
			RETURN
		END
		RAISERROR('"%s": Match to DatabaseLookupID %d', 0, 0, @DatabaseLookup, @DatabaseLookupID) WITH NOWAIT
	END
	ELSE IF NOT EXISTS(SELECT * FROM SEIDR.DatabaseLookup WHERE DatabaseLookupID = @DatabaseLookupID)
	BEGIN
		RAISERROR('DatabaseLookupID required. %d Did not match up to an existing Lookup.', 16, 1, @DatabaseLookupID)
		RETURN
	END

	DECLARE @validate UTIL.udt_Varchar500 --(ID int primary key, [Value] varchar(500))
	IF CHARINDEX(';', @LoadBatchTypeList, 1) > 0
	BEGIN
		INSERT INTO @validate(ID, [Value])
		SELECT ID, UTIL.ufn_CleanField([Value])
		FROM UTIL.ufn_SplitVarchar500(';', @LoadBatchTypeList)	
	END
	ELSE IF CHARINDEX(',',@LoadBatchTypeList,  1) > 0
	BEGIN
		INSERT INTO @validate(ID, [Value])
		SELECT ID, UTIL.ufn_CleanField([Value])
		FROM UTIL.ufn_SplitVarchar500(',', @LoadBatchTypeList)
	END
	ELSE -- No splitting.
		INSERT INTO @validate(ID, [Value])
		SELECT 1, UTIL.ufn_CleanField(@LoadBatchTypeList) 

	IF EXISTS(SELECT null FROM @validate v WHERE NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = v.Value AND Inbound = 1))
	BEGIN
		SELECT distinct [Value] [LoadBatchTypeCode]
		FROM @validate v 
		WHERE NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = v.Value AND Inbound = 1)
		RAISERROR('Load Batch Type Codes should exist as Inbound (for Loading) keys in the REFERENCE.UserKey table - Delimiter should be ";" or ","', 16, 1)
		RETURN
	END
		
	IF @Description is null 
	BEGIN
		SET @Description = 'Check Export Status'
		IF @CheckOrganization = 1
		BEGIN
			SELECT @Description += ' for Organization "' + o.Description + '"'
			FROM REFERENCE.Organization o WITH (NOLOCK)
			JOIN SEIDR.JobProfile jp WITH (NOLOCK)
				ON o.OrganizationID = jp.organizationID
			WHERE jp.JobProfileID = @JobProfileID

			IF @CheckProject = 1
			BEGIN		
				SELECT @Description += ', Project "' + p.Description + '"'
				FROM REFERENCE.Project p WITH (NOLOCK)
				JOIN SEIDR.JobProfile jp WITH (NOLOCK)
					ON p.ProjectID = jp.ProjectID
				WHERE jp.JobProfileID = @JobProfileID

				IF @@ROWCOUNT = 0
				BEGIN
					RAISERROR('@CheckProject = 1, but Job Profile is not associated with a ProjectID.', 16, 1)
					RETURN
				END
			END
		END
		ELSE IF @CheckProject = 1
		BEGIN		
			SELECT @Description += ' for Project "' + p.Description + '"'
			FROM REFERENCE.Project p WITH (NOLOCK)
			JOIN SEIDR.JobProfile jp WITH (NOLOCK)
				ON p.ProjectID = jp.ProjectID
			WHERE jp.JobProfileID = @JobProfileID

			IF @@ROWCOUNT = 0
			BEGIN
				RAISERROR('@CheckProject = 1, but Job Profile is not associated with a ProjectID.', 16, 1)
				RETURN
			END
		END
	END
	

	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'StagingToAndromedaExportStatusJob' 
	AND JobNameSpace = 'METRIX_EXPORT'

	
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
	

	IF EXISTS(SELECT null FROM SEIDR.StagingToAndromedaExportStatusJob WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		UPDATE SEIDR.StagingToAndromedaExportStatusJob
		SET CheckProject = @CheckProject,
			CheckOrganization = @CheckOrganization,
			IgnoreProcessingDate = @ignoreProcessingDate,
			RequireCurrentProcessingDate = @RequireCurrentPRocessingDate,
			MonitoredOnly = @MonitoredOnly,
			IgnoreUnusedProfiles = @IgnoreUnusedProfiles,
			LoadBatchTypeList = @LoadBatchTypeList,
			DatabaseLookupID = @DatabaseLookupID
		WHERE JobProfile_JobID = @JobProfile_JobID 
		
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.StagingToAndromedaExportStatusJob(JobProfile_JobID, CheckOrganization, CheckProject, 
		IgnoreProcessingDate, RequireCurrentProcessingDate, 
		MonitoredOnly, IgnoreUnusedProfiles, LoadBatchTypeList, DatabaseLookupID)
		VALUES(@JobProfile_JobID, @CheckOrganization, @CheckProject,
		@IgnoreProcessingDate, @RequireCurrentProcessingDate,
		@MonitoredOnly, @IgnoreUnusedProfiles, @LoadBatchTypelist, @DatabaseLookupID)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.StagingToAndromedaExportStatusJob
	WHERE JobProfile_JobID = @JobProfile_JobID
END