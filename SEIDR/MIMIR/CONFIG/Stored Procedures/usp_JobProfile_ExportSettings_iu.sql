CREATE PROCEDURE [CONFIG].[usp_JobProfile_ExportSettings_iu]
	@JobProfileID int,
	@MetrixExportJob varchar(100),
	@ArchiveLocation varchar(500),
	@ExportJobNameSpace varchar(130) = 'METRIX_EXPORT',
	@StepNumber smallint = null,
	@Description varchar(100) = null,		
	@MetrixDatabaseLookupID int = null,
	@DatabaseLookup varchar(50) = 'METRIX',
	@CanRetry bit = null,
	@RetryLimit int = 0,
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
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job
	WHERE JobName = @MetrixExportJob
	AND JobNameSpace = @ExportJobNameSpace

	IF @@ROWCOUNT = 0
	BEGIN
		SELECT * 
		FROM SEIDR.Job
		WHERE JobName LIKE '%' + REPLACE(@MetrixExportJob, ' ', '%') + '%'
		OR JobNameSpace = @ExportJobNameSpace
		RAISERROR('Unable to identify Job', 16, 1)
		RETURN
	END

	IF @ArchiveLocation is null
	BEGIN
		RAISERROR('Must specify @ArchiveLocation', 16, 1)
		RETURN
	END	

	IF @MetrixDatabaseLookupID is null
	BEGIN
		SELECT @MetrixDatabaseLookupID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE Description = @DatabaseLookup

		IF @MetrixDatabaseLookupID IS NULL
		BEGIN
			SELECT * 
			FROM SEIDR.DatabaseLookup
			RAISERROR('Unable to determine @MetrixDatabaseLookupID', 16, 1)
			RETURN
		END
	END

	IF @Description is null
		SET @Description = 'METRIX EXPORT: ' + @ExportJobNameSpace + '.' +  @MetrixExportJob


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

	IF @@ERROR <> 0 OR @JobProfile_JobID IS NULL
	BEGIN
		RAISERROR('Unable to create step.', 16, 1)
		RETURN
	END

	IF EXISTS(SELECT null FROM METRIX.ExportSettings WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		UPDATE METRIX.ExportSettings 
		SET ArchiveLocation = @ArchiveLocation,
			MetrixDatabaseLookupID = @MetrixDatabaseLookupID
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE
	BEGIN
		INSERT INTO METRIX.ExportSettings(JobProfile_JobID, ArchiveLocation, MetrixDatabaseLookupID)
		VALUES(@JobProfile_JobID, @ArchiveLocation, @MetrixDatabaseLookupID)
	END

	SELECT * FROM METRIX.vw_ExportSettings


RETURN 0
