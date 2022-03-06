CREATE PROCEDURE [SEIDR].[usp_JobProfile_ExportSettings_iu]
	@JobProfileID int,
	@JobName varchar(130),
	@StepNumber tinyint = null,	
	@Description varchar(100) = null,
	@ArchiveLocation varchar(500) = null,	
	@MetrixDatabaseLookupID int = null,
	@MetrixDatabaseLookup varchar(50) = 'METRIX',

	@VendorName varchar(130) = null,
	@RemoveVendorName bit = 0,
	@ExportType varchar(150) = null,
	@RemoveExportType bit = 0,
	@ImportType varchar(150) = null,
	@RemoveImportType bit = 0,

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
	DECLARE @JobID int

	IF @JobName is null
	BEGIN
		SELECT TOP 1 @JobID = JobID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @JobProfileID
		AND StepNumber = @StepNumber
		AND Active = 1
		AND (@TriggerExecutionStatus is null AND TriggerExecutionStatusCode is null OR @TriggerExecutionStatus = TriggerExecutionStatusCode)
		AND (@TriggerExecutionNameSpace is null AND TriggerExecutionNameSpace is null OR @TriggerExecutionNameSpace = TriggerExecutionNameSpace)
		IF @@ROWCOUNT > 0
			RAISERROR('Grabbing JobID from existing configuration...', 16, 1)
		ELSE
		BEGIN
			RAISERROR('Unable to identify a JobID', 16, 1)
			RETURN
		END

		SELECT @JobName = JobName
		FROM SEIDR.Job 
		WHERE JobID = @JobID
	END
	ELSE
	BEGIN
		SELECT @JobID = JobID
		FROM SEIDR.Job
		WHERE JobName = @JobName
		AND JobNameSpace = 'METRIX_EXPORT'

		IF @JobID is null
		BEGIN
			RAISERROR('Must specify a valid Job. (Provided: %s)', 16, 1, @JobName)
			RETURN
		END
	END	
	DECLARE @VendorID smallint = null
	IF @RemoveVendorName = 0 AND @VendorName is not null
		SELECT @VendorID = VendorID
		FROM METRIX.Vendor WITH (NOLOCK)
		WHERE Description = @VendorName

	DECLARE @ExportTypeID tinyint = null, @ImportTypeID tinyint = null,  @ValidateImportTypeID tinyint = null
	IF @RemoveImportType = 0 AND @ImportType IS NOT NULL
		SELECT @ImportTypeID = ImportTypeID 
		FROM METRIX.ImportType WITH (NOLOCK)
		WHERE Description = @ImportType
	
		
	IF @RemoveExportType = 0 AND @ExportType IS NOT NULL
		SELECT @ExportTypeID = ExportTypeID, 
			@ValidateImportTypeID = ImportTypeID
		FROM METRIX.ExportType WITH (NOLOCK)
		WHERE Description = @ExportType

	IF @ImportTypeID IS NOT NULL
	AND (@ValidateImportTypeID is null OR @ValidateImportTypeID <> @ImportTypeID)
	BEGIN
		SELECT ExportTypeID, ex.Description [ExportType], ex.ImportTypeID, im.Description [ImportType]
		FROM METRIX.ExportType ex
		LEFT JOIN METRIX.ImportType im
			ON ex.ImportTypeID = im.ImportTypeID
		WHERE ExportTypeID = @ExportTypeID 
		OR ex.ImportTypeID = @ImportTypeID

		RAISERROR('Invalid @ImportTypeID (%d) - Import type specified by Export Type ID %d does not match (%d)', 16, 1, @ImportTypeID, @ExportTypeID, @ValidateImportTypeID)
		RETURN		
	END
	IF @ImportTypeID IS NULL AND @ValidateImportTypeID IS NOT NULL
	BEGIN
		RAISERROR('Setting @ImportTypeID based on @ExportType(%s): %d', 0, 0, @ExportType, @ValidateImportTypeID)
		SET @ImportTypeID = @ValidateImportTypeID
	END

	IF @MetrixDatabaseLookupID is null AND @MetrixDatabaseLookup is not null
	BEGIN
		SELECT TOP 1 @MetrixDatabaseLookupID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE Description = @MetrixDatabaseLookup
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Could not identify a DatabaseLookupID for description "%s"', 16, 1, @MetrixDatabaseLookup)
			RETURN
		END
	END
	
	/*
	IF @ArchiveLocation is null
	AND @MetrixDatabaseLookupID IS NULL
	BEGIN
		RAISERROR('Should specify an ArchiveLocation or Metrix Database Lookup', 16, 1)
		RETURN
	END*/

	SET @Description = COALESCE(UTIL.ufn_CleanField(@Description), @JobName)
	DECLARE @JobProfile_JobID int
	
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

	IF EXISTS(SELECT null FROM METRIX.ExportSettings WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		UPDATE es
		SET ArchiveLocation = @ArchiveLocation,
			MetrixDatabaseLookupID = @MetrixDatabaseLookupID,
			VendorID = @VendorID,
			ExportTypeID = @ExportTypeID,
			ImportTypeID = @ImportTypeID		
		FROM METRIX.ExportSettings es
		WHERE JobProfile_JobID = @JobProfile_JobID
	END	
	ELSE
		INSERT INTO METRIX.ExportSettings(JobProfile_JobID, ArchiveLocation, MetrixDatabaseLookupID, VendorID, ExportTypeID, ImportTypeID)
		VALUES(@JobProfile_JobID, @ArchiveLocation, @MetrixDatabaseLookupID, @VendorID, @ExportTypeID, @ImportTypeID)

	SELECT * 
	FROM SEIDR.JobProfile_Job
	WHERE JobProfile_JobID = @JobProfile_JobID

	SELECT * 
	FROM METRIX.ExportSettings
	WHERE JobProfile_JobID = @JobProfile_JobID

	RETURN 0
END

