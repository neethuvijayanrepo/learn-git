CREATE PROCEDURE [SEIDR].[usp_JobProfile_DemoMapJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@JobName varchar(128),
	@FileMapID int,
	@FileMapDatabaseID int = null,
	@FileMapDatabase varchar(128) = 'DataServices',
	@OutputFolder varchar(256) = null,
	@Delimiter char(3) = '|',
	@OutputDelimiter char(3) = '|',
	@PayerLookupDatabaseID int = null, -- For IsSelfPay 
	@PayerLookupDatabase varchar(128) = 'Andromeda_Staging',
	--ToDo: @CodePage
	--@PayerFacilityID int = null,
	@Description varchar(256) = null,
	@DoAPB bit = null,
	@Enable_OOO bit = 0,
	@FilePageSize int = null,
	@OOO_InsuranceBalanceValidation bit = null,
	@HasHeaderRow bit = null,

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
	IF @Description is null
		SET @Description = 'Demographic Map: ' + @JobName

	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = @JobName
	AND JobNameSpace = 'DemoMap'

	IF @@ROWCOUNT = 0
	BEGIN
		SELECT * FROM SEIDR.Job WHERE JobNameSpace = 'DemoMap'
		RAISERROR('Invalid DMAP Job: %s', 16, 1, @JobName)
		RETURN 50
	END

	IF @OutputFolder LIKE '%#%'
	BEGIN
		SET @outputFolder = CONFIG.ufn_ShortHandPath_Profile(@OutputFolder, @JobProfileID)
	END


	IF @DoAPB is null
	BEGIN
		SELECT @DoAPB = Modular
		FROM SEIDR.JobProfile jp WITH (NOLOCK)
		JOIN REFERENCE.Project p
			ON jp.ProjectID = p.ProjectID
		
		IF @@ROWCOUNT = 0
			SET @DoAPB = 0
	END
	
	IF @FileMapDatabaseID is null
	BEGIN
		SELECT @FileMapDatabaseID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE Description = @FileMapDatabase
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Unable to identify File Mapping Database from description "%s"', 16, 1, @FileMapDatabase)
			RETURN
		END
	END

	IF @PayerLookupDatabaseID is null
	BEGIN
		SELECT @PayerLookupDatabaseID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE Description = @PayerLookupDatabase
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Unable to identify Andromeda_Staging/ PayerMaster lookup Database from description "%s"', 16, 1, @PayerLookupDatabase)
			RETURN
		END
	END
	
	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @JobName,
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
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
		iF @@ERROR <> 0
		BEGIN
			RETURN
		END
	

	IF EXISTS(SELECT null FROM SEIDR.DemoMapJob WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		UPDATE SEIDR.DemoMapJob
		SET 
			FileMapID = @FileMapID,
			FileMapDatabaseID = @FileMapDatabaseID,
			delimiter = @Delimiter,
			OutputDelimiter =@OutputDelimiter,
			PayerLookupDatabaseID = @PayerLookupDatabaseID , -- For IsSelfPay lookups and _load table metadata
			--PayerFacilityID = @PayerFacilityID, -- Replace organization level lookup - include each payer code with each facilityKey that is configured in the database.
			DoAPB = @DoAPB,
			Enable_OOO = @Enable_OOO,
			FilePageSize = @FilePageSize,
			OutputFolder = @OutputFolder,
			OOO_InsuranceBalanceValidation = COALESCE(@OOO_InsuranceBalanceValidation, OOO_InsuranceBalanceValidation),
			HasHeaderRow = ISNULL(@HasHeaderRow, HasHeaderRow)
		WHERE JobProfile_JobID = @JobProfile_JobID 
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.DemoMapJob(JobProfile_JobID, FileMapID, FileMapDatabaseID, Delimiter, OutputDelimiter, PayerLookupDatabaseID,
			DoAPB, Enable_OOO, FilePageSize, OutputFolder, 
			OOO_InsuranceBalanceValidation, HasHeaderRow)
		VALUES(@JobProfile_JobID, @FileMapID, @FileMapDatabaseID, @Delimiter, @OutputDelimiter, @PayerLookupDatabaseID,
			@DoAPB, @Enable_OOO, @FilePageSize, @OutputFolder, 
			COALESCE(@OOO_InsuranceBalanceValidation, 1), ISNULL(@HasHeaderRow, 1))		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.DemoMapJob
	WHERE JobProfile_JobID = @JobProfile_JobID
END