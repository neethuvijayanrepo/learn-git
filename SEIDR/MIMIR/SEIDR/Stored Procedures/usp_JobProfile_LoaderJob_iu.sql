CREATE PROCEDURE [SEIDR].[usp_JobProfile_LoaderJob_iu]
	@JobProfileID int,
	@OutputFolder varchar(500),
	-- Name of file. Will try to concatenate with OutputFolder to set initial OutputFilePath. Or to set date masked OutputFilename based on processing date
	@OutputFileName varchar(128) = null, 
	@PackageID int = null,
	@PackageCategory varchar(50) = null,	
	@SetCategoryThreadID bit = null,
	@ServerName varchar(128) = null,
	@PackagePath varchar(500) = null,
	@PackageName varchar(128) = null, --For new packages
	@FacilityID smallint = null,
	@StepNumber tinyint = null,		
	@Description varchar(100) = null,
	@ServerInstanceName varchar(100) = NULL,
	@AndromedaServer varchar(50) = NULL,
	@DatabaseName varchar(128) = null, -- for SSIS variable. E.g., DataServices versus DataServices_Dev
	@DatabaseConnectionManager varchar(128) = null,
	@RemoveDatabaseConnectionManager bit = 0,
	@DatabaseConnection_DatabaseLookup varchar(128),	
	@Misc varchar(200) = null,
	@RemoveMisc1 bit = 0,
	@Misc2 varchar(200) = null,
	@RemoveMisc2 bit = 0,
	@Misc3 varchar(200) = null,
	@RemoveMisc3 bit = 0,
	@SecondaryFilePath varchar(4000) = null,
	@RemoveSecondaryFilePath bit = 0,
	@TertiaryFilePath varchar(4000) = null,
	@RemoveTertiaryFilePath bit = 0,
	@CanRetry bit = 1,
	@RetryLimit int = 18,
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
	IF @ServerInstanceName is null
		SELECT @ServerInstanceName = ServerName 
		FROM SEIDR.DatabaseLookup 
		WHERE Description = 'DataServices'
	IF @AndromedaServer is null
		SELECT @AndromedaServer = ServerName
		FROM SEIDR.databaseLookup
		WHERE Description = 'REPORT'		
		 
	SET XACT_ABORT ON


	IF @PackageID IS NULL
	BEGIN
		SELECT @PackageID = PackageID
		FROM SEIDR.SSIS_Package
		WHERE Category = @PackageCategory
		AND (ServerName = @ServerName OR ServerName is null AND @ServerName is null)
		AND PackagePath = @PackagePath
		IF @@ROWCOUNT = 0
		BEGIN
			IF NULLIF(@PackageName,'') is null
			BEGIN
				RAISERROR('Package does not exist. Please provide @PackageName', 16, 1)
				RETURN
			END
			IF @PackagePath LIKE '\[a-z0-9]%' AND @ServerName IS NULL
			BEGIN
				RAISERROR('Package path looks like it''s for a server, but ServerName is not specified. Suggested Default: Location of DataServices', 16, 1)
				RETURN
			END
			INSERT INTO SEIDR.SSIS_Package(Category, Name, ServerName, PackagePath)
			VALUES(@PackageCategory, @PackageName, @ServerName, @PackagePath)
			SELECT @PackageID = SCOPE_IDENTITY()
		END

		IF @Description is null
			SET @Description = 'PACKAGE CALL - ' + @PackageName
	END
	ELSE IF NOT EXISTS(SELECT null FROM SEIDR.SSIS_Package WHERE PackageID = @PackageID)
	BEGIN
		RAISERROR('@PackageID not valid: %d', 16, 1, @PackageID)
		RETURN
	END
	ELSE IF @Description is null
	BEGIN
		SELECT @Description = 'PACKAGE CALL - ' + Name, @PackageCategory = Category
		FROM SEIDR.SSIS_Package 
		WHERE PackageID = @PackageID
	END

	IF @OutputFolder LIKE '%#%'
	BEGIN
		RAISERROR('Applying ShortHand to OutputFolder...', 0, 0)
		SET @OutputFolder = CONFIG.ufn_ShortHandPath_Profile(@OutputFolder, @JobProfileID)		
	END

	IF @SecondaryFilePath LIKE '%#%'
	BEGIN
		RAISERROR('Applying Shorthand to SecondaryFilePath...', 0,0)
		SET @SecondaryFilePath = CONFIG.ufn_ShortHandPath_Profile(@SecondaryFilePath, @JobProfileID)
	END
	IF @TertiaryFilePath LIKE '%#%'
	BEGIN
		RAISERROR('Applying Shorthand to TertiaryFilePath...', 0,0)
		SET @TertiaryFilePath = CONFIG.ufn_ShortHandPath_Profile(@TertiaryFilePath, @JobProfileID)
	END
	
	DECLARE @DatabaseConnection_DatabaseLookupID int = null
	IF @DatabaseConnectionManager is not null
	BEGIN
		SELECT @DatabaseConnection_DatabaseLookupID = DatabaseLookupID
		FROM SEIDR.DatabaseLookup
		WHERE [Description] = @DatabaseConnection_DatabaseLookup

		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Database Lookup "%s" not found.', 16, 1, @DatabaseConnection_DatabaseLookup)
			RETURN			
		END
	END
	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'PreProcessJob' 
	AND JobNameSpace = 'PreProcess'

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
		@FailureNotificationMail  = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
	
	IF EXISTS(SELECT null FROM SEIDR.LoaderJob WHERE JobProfile_JObID = @JobProfile_JobID AND DD IS NULL)
	BEGIN
		UPDATE SEIDR.LoaderJob
		SET FacilityID = @FacilityID,
			PackageID = @PackageID,
			ServerInstanceName = @ServerInstanceName,
			AndromedaServer = @AndromedaServer,
			OutputFolder = @OutputFolder,
			LU = GETDATE(),
			OutputFileName = @OutputFileName,
			DatabaseName = @DatabaseName,
			Misc = IIF(@RemoveMisc1 = 1, null, ISNULL(@Misc, Misc)),
			Misc2 = IIF(@RemoveMisc2 = 1, null, ISNULL(@Misc2, Misc2)),
			Misc3 = IIF(@RemoveMisc3 = 1, null, ISNULL(@Misc3, Misc3)),
			SecondaryFilePath = IIF(@RemoveSecondaryFilePath = 1, null, ISNULL(@SecondaryFilePath, SecondaryFilePath)),
			TertiaryFilePath = IIF(@RemoveTertiaryFilePath = 1, null, ISNULL(@TertiaryFilePath, TertiaryFilePath)),
			DatabaseConnectionManager = IIF(@RemoveDatabaseConnectionManager = 1, null, ISNULL(@DatabaseConnectionManager, DatabaseConnectionManager)),
			DatabaseConnection_DatabaseLookupID = ISNULL(@DatabaseConnection_DatabaseLookupID, DatabaseConnection_DatabaseLookupID)
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND DD IS NULL
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.LoaderJob(JobProfile_JobID, FacilityID, PackageID, ServerInstanceName,
			AndromedaServer, OutputFolder, OutputFileName, DatabaseName,
			Misc, Misc2, Misc3, SecondaryFilePath, TertiaryFilePath, DatabaseConnectionManager, DatabaseConnection_DatabaseLookupID)
		VALUES(@JobProfile_JobID,@FacilityID, @PackageID, @ServerInstanceName,
			@AndromedaSErver, @OutputFolder, @OutputFileName, @DatabaseName,
			@Misc, @Misc2, @Misc3, @SecondaryFilePath, @TertiaryFilePath, @DatabaseConnectionManager, @DatabaseConnection_DatabaseLookupID)	
	END

	
	
	IF @SetCategoryThreadID is null
		IF @ThreadID is null
			SET @SetCategoryThreadID = 0
		ELSE
			SET @SetCategoryThreadID = 1

	IF @SetCategoryThreadID = 1
	BEGIN
		exec SEIDR.usp_PackageCategory_SetThreadID @PackageCategory, @ThreadID
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.LoaderJob
	WHERE JobProfile_JobID = @JobProfile_JobID 

	
	SELECT *
	FROM SEIDR.SSIS_Package 
	WHERE PackageID = @PackageID
END