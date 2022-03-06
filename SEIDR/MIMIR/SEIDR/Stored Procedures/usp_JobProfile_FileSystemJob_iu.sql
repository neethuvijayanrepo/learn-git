CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileSystemJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	@Source varchar(500) = null,
	@OutputPath varchar(500) = null,
	@Filter varchar(230) = null,	
	@UpdateExecutionPath bit = null,
	@Overwrite bit = 0,
	@FileOperation varchar(15) = 'COPY',
	@LoadProfileID int = null,
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
	IF @CanRetry is null
	BEGIN
		IF @Source is null --JobExecution filepath, or no source needed. Should be able to do first time unless there's an issue. Manual intervention would generally be better then
			SET @CanRetry = 0
		ELSE
			SET @CanRetry = 1
	END
	IF @FileOperation LIKE '%METRIX' AND @CanRetry = 1
	BEGIN
		RAISERROR('Forcing CanRetry = 0', 1,1) WITH NOWAIT --Will auto requeue if there's an issue.
		SET @CanRetry = 0
	END

	DECLARE @RequireOutputPath bit, @RequireSource bit
	
	SELECT @RequireOutputPath = RequireOutputPath, @RequireSource = RequireSource -- SELECT *
	FROM SEIDR.FileSystemOperation  WITH (NOLOCK)
	WHERE FileSystemOperationCode = @FileOperation	
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Invalid @FileOperation: ', 16, 1, @FileOperation)
		RETURN 50
	END
	SET @FileOperation = UPPER(@FileOperation)	
	 
	IF @FileOperation = 'UNZIP' AND @OutputPath is null
	AND (
		@StepNumber = 1
		OR NOT EXISTS(SELECT null FROM SEIDR.JobProfile_Job WHERE JobProfileID = @JobProfileID AND Active = 1)
		)
	BEGIN
		SELECT @OutputPath = RegistrationDestinationFolder
		FROM SEIDR.JobProfile WITH (NOLOCK)
		WHERE JobProfileID = @JobProfileID
		IF @OutputPath is not null
		BEGIN
			IF RIGHT(@OutputPath, 1) <> '\'
				SET @OutputPath = @OutputPath + '\'
			RAISERROR('@StepNumber = 1. Auto update @OutputPath using RegistrationDestination ... "%s"', 0, 0, @OutputPath) WITH NOWAIT
		END		
	END

	DECLARE @METRIX_OPERATION bit = 0

	IF @FileOperation LIKE '%\_METRIX' ESCAPE '\'
	BEGIN		
		SET @METRIX_OPERATION= 1
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
		--Code added in c# to use the LoadProfileID from the JobExecution (JobProfile at time of execution) if value is null.
		--If no value found, will reutrn an exception to indicate such.
		/*
		IF @LoadProfileID IS NULL
		BEGIN
			SELECT @LoadProfileID = LoadProfileID
			FROM SEIDR.JobProfile
			WHERE JobProfileID = @JobProfileID
			IF @@ROWCOUNT > 0
				RAISERROR('Taking Metrix LoadProfile from JobProfile...', 0, 0) WITH NOWAIT
		END


		IF @LoadProfileID is null 
		BEGIN
			RAISERROR('LoadProfileID required, but not provided.', 16, 1)
			RETURN
		END	
		
		IF @RetryLimit < 400
			SET @RetryLimit = 400
		*/
	END
	ELSE IF @RequireOutputPath = 1 AND NULLIF(LTRIM(RTRIM(@OutputPath)), '') IS NULL
	BEGIN
		RAISERROR('OutputPath required, but not provided.', 16, 1)
		RETURN
	END
	IF @Source IS NOT NULL AND (@Source LIKE '<\%' OR @Source LIKE '<[a-z]:%')
	BEGIN
		RAISERROR('Invalid @Source: %s', 16, 1, @Source)
		RETURN
	END
	IF @OutputPath IS NOT NULL AND (@OutputPath LIKE '<\%' OR @OutputPath LIKE '<[a-z]:%')
	BEGIN
		RAISERROR('Invalid @OutputPath: %s', 16, 1, @OutputPath)
		RETURN
	END

	DECLARE @OrganizationID int, @ProjectID smallint, @UserKey varchar(50), @ShortHandLoadProfileID int
	SELECT @OrganizationID = OrganizationID, 
			@ProjectID = ProjectID, 
			@UserKey = UserKey1, 
			@ShortHandLoadProfileID = COALESCE(@LoadProfileID, LoadProfileID) -- If passed explicitly, use that instead.
	FROM SEIDR.JobProfile WITH (NOLOCK)
	WHERE JobProfileID = @JobProfileID

	IF @Source LIKE '%#%' OR @OutputPath LIKE '%#%'
	BEGIN
		RAISERROR('Replacing @Source (%s) and @OutputPath (%s)', 0, 0, @Source, @OutputPath) WITH NOWAIT

		SELECT @Source = CONFIG.ufn_ShortHandPath(@Source, @OrganizationID, @ProjectID, @UserKey, @ShortHandLoadProfileID),
				@OutputPath = CONFIG.ufn_ShortHandPath(@OutputPath, @OrganizationID, @ProjectID, @UserKey, @ShortHandLoadProfileID)

		RAISERROR('Replaced @Source: %s, @OutputPath: %s', 0, 0, @Source, @OutputPath) WITH NOWAIT
	END

	


	IF @UpdateExecutionPath is null
	BEGIN
		IF @OutputPath LIKE '%INPUT%' 
		OR @METRIX_OPERATION = 1
		OR @Source IS NULL AND @OutputPath is null
		OR @RequireOutputPath = 0 AND @Source is null
		OR @FileOperation LIKE '%DELETE%' AND @Source IS NOT NULL -- If indicating a specific location for delete, don't update path unless explicitly specified.
			SET @UpdateExecutionPath = 0
		ELSE
			SET @UpdateExecutionPath = 1
	END

	IF @Description is null 	
	OR LEN(@Description) >= 30 
		AND REPLACE(REPLACE(@Description, 'RENAME', 'MOVE'), 'MOVING', 'MOVE') 
			NOT LIKE '%' + REPLACE(REPLACE(REPLACE(@FileOperation, '_', '%'),'DEST', ''), 'METRIX', '') + '%'
	OR LEN(@Description) < 30 AND @FileOperation NOT IN ('CHECK', 'EXIST') AND @Description LIKE '%CHECK%'
	BEGIN
		DECLARE @Alert bit = 0
		IF @Description is not null
			SET @Alert = 1

		IF @OutputPath is not null 
		OR @RequireOutputPath = 0 AND @RequireSource = 1 AND @Source IS NOT NULL --Source could come from JobExecution FilePath if not provided here.
		BEGIN					
			DECLARE @end varchar(100)
			
			IF @RequireOutputPath = 0 AND @RequireSource = 1 
			BEGIN
				SELECT @end = UTIL.ufn_PathItem_GetName(@Source)
			END
			ELSE
			BEGIN
				SELECT @End = 
						right(@OutputPath,case when charindex('\',reverse(@OutputPath), 1) > 1 then charindex('\',reverse(@OutputPath), 1)- 1 
												WHEN charindex('\',reverse(@OutputPath), 1) =  1 then charINDEX('\', REVERSE(@OutputPath), 2) - 1
												else len(@OutputPath) end)
			

				IF RIGHT(@end, 1) = '\'			
					SELECT @end = LEFT(@End, LEN(@End) - 1)				
			END
			
			DECLARE @File bit = 0
			DECLARE @Folder varchar(40) = '  [FOLDER]'
			IF @End LIKE '%.%'
			BEGIN
				SET @Folder = ''
				SET @File = 1				
			END			
			ELSE IF @End = '*'
				SET @End = ''
			
			IF @End LIKE '%<%'
				SET @End = '"' + @End + '"'

			IF @RequireOutputPath = 0 AND @RequireSource = 1 
			BEGIN
				IF @Source LIKE '%_SourceFiles%' 
					SET @Folder = '  [SOURCE FOLDER]'
				ELSE IF @Source LIKE '%PREPROCESS%'
					SET @Folder = '  [PREPROCESS FOLDER]'
				ELSE IF @Source LIKE '%AndromedaFilesSandbox%'
					SET @Folder = '  [SANDBOX FOLDER]'
				ELSE IF @Source LIKE '%AndromedaFilesSandbox'
					SET @Folder = '  [METRIX FOLDER]'
				ELSE IF @Source LIKE '%\FTP%' AND @End <> 'FTP'
					SEt @Folder = '  [FTP]'
				ELSE IF @Source LIKE '%\TABULAR\INPUT\%' OR @Source LIKE '%Proclaim%' OR @Source LIKE '%VORTEXML%'
					SET @Folder = '  [PROCLAIM FOLDER]'
			END
			ELSE if @OutputPath LIKE '%_SourceFiles%'
				SET @Folder = '  [SOURCE FOLDER]'
			ELSE IF @OutputPath LIKE '%PREPROCESS%'
				SET @Folder = '  [PREPROCESS FOLDER]'
			ELSE IF @OutputPath LIKE '%AndromedaFilesSandbox%'
				SET @Folder = '  [SANDBOX FOLDER]'
			ELSE IF @OutputPath LIKE '%AndromedaFiles%'
				SET @Folder = '  [METRIX FOLDER]'
			ELSE IF @outputPath LIKE '%\FTP%' AND @End <> 'FTP'
				SET @Folder = '  [FTP]'
			ELSE IF @Outputpath LIKE '%\Tabuler\Input\%' OR @OutputPath LIKE '%PROCLAIM%'  OR @OutputPath LIKE '%VORTEXML%'
				SET @Folder = '  [PROCLAIM FOLDER]'
			
			IF @File = 1
				SET @Folder = REPLACE(@Folder, 'FOLDER]', ']')


			SET @Description = 'FileSystem: ' + @FileOperation + ' -> ' + @End + @Folder
			IF @File = 1 AND @outputPath NOT LIKE '%\INPUT\<%'
			AND (@RequireOutputPath = 1 OR @RequireSource = 0)
			AND 
			(
				REPLACE(@OutputPath, '_', '') LIKE '%\<%*%' 
				OR REPLACE(@outputPath, '_', '') LIKE '%\<[YMD]%><[YMD]%><[YMD]%>' 
			)
			BEGIN
				SET @Description = @Description + ' (Parent Folder: ' + UTIL.ufn_PathItem_GetName(REPLACE(@OutputPath, '\' + UTIL.ufn_PathItem_GetName(@OutputPath), '')) + ')'						
			END
			
		END
		ELSE IF @ShortHandLoadProfileID is not null
		AND (@METRIX_OPERATION = 0 OR @LoadProfileID IS NOT NULL) --Only meaningful for Metrix operation if passed a specific @LoadProfielID
			SET @Description = 'FileSystem: ' + @FileOperation + ' -> LoadProfile Input (' + CONVERT(varchar, @ShortHandLoadProfileID) + ').'		
		ELSE 
			SET @Description = 'FileSystem: ' + @FileOperation 

		IF  @updateExecutionPath = 1 AND @FileOperation IN ('CHECK', 'EXIST', 'CREATE_DUMMY') 
			SET @Description = @Description + '. Set as the JobExecution.FilePath'
			
		IF @Alert = 1
		BEGIN
		--	DECLARE @Description varchar(300) = 'test'
			RAISERROR('The @Description parameter given appears to be misleading. 
NOTE: you can skip setting @Description(or pass NULL) to have an auto-generated Description: "%s"', 16, 1, @Description)
			RETURN
		END
	END

	
	
	

	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileSystemJob' 
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
	

	IF EXISTS(SELECT null FROM SEIDR.FileSystemJob WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1)
	BEGIN
		UPDATE SEIDR.FileSystemJob
		SET Source = @Source,
			OutputPath = @OutputPath,
			Filter = @Filter,
			Operation = @FileOperation,
			UpdateExecutionPath = @UpdateExecutionPath,
			Overwrite = @Overwrite,
			LoadProfileID = @LoadProfileID,
			DatabaseLookupID = @DatabaseLookupID
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND Active = 1
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.FileSystemJob(JobProfile_JobID, Source, OutputPath, Filter, Operation,
			UpdateExecutionPath, Overwrite, LoadProfileID, DatabaseLookupID)
		VALUES(@JobProfile_JobID, @Source, @OutputPath, @filter, @FileOperation,
			@UpdateExecutionPath, @Overwrite, @LoadProfileID, @DatabaseLookupID)		
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.FileSystemJob
	WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
END