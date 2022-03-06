CREATE PROCEDURE [UTIL].[usp_CleanData]
	@SafetyMode bit = 1
AS
	IF @Safetymode = 1
	BEGIN
		RAISERROR('Cannot call procedure in SafetyMode', 16, 1)
		RETURN
	END
	DECLARE @LastServiceActivity datetime
	SELECT @LastServiceActivity = MAX(LastLoad)
	FROM SEIDR.Job
	IF @LastServiceActivity > GETDATE() - 4
	BEGIN
		RAISERROR('Database has been active within the last few days. Not allowed to delete database data while active.', 16, 1)
		RETURN
	END


	TRUNCATE TABLE SEIDR.Log
	TRUNCATE TABLE SEIDR.BatchScriptJob --Parameter 3/4 freeform text, could have extra details
	TRUNCATE TABLE SEIDR.JobExecution_Note
	TRUNCATE TABLE SEIDR.JobExecutionCheckPoint
	--TRUNCATE TABLE SEIDR.JobProfileNote
	--TRUNCATE TABLE REFERENCE.Contact
	UPDATE SEIDR.JobExecution
	SET FilePath = null, FileHash = null, FileSize = null

	TRUNCATE TABLE SEIDR.JobExecution_ExecutionStatus
	

	UPDATE SEIDR.JobProfile
	SET DD = GETDATE(), Description = 'JobProfileID ' + CONVERT(varchar(30), JobProfileID),
		FileExclusionFilter = null, FileFilter = null,
		FileDateMask = null,
		RegistrationDestinationFolder = null,
		RegistrationFolder = null
	WHERE Active = 1

	TRUNCATE TABLE SEIDR.JobProfileHistory

	UPDATE SEIDR.JobProfile_Job
	SET Description = 'StepNumber ' + CONVERT(varchar(30), StepNumber) + ', JobID ' + CONVERT(varchar(30), JobID)

	UPDATE SEIDR.FTPAccount
	SET [Password] = null, PpkFileName = null, Description = 'FTP Account ' + CONVERT(varchar(30), FtpAccountID)

	UPDATE SEIDR.FileSystemJob
	SET Source = null,
		OutputPath = null

	UPDATE SEIDR.FTPJob
	SET LocalPath = null, 
		RemotePath = null,
		RemoteTargetPath = null

	UPDATE SEIDR.DatabaseLookup
	SET EncryptedPassword = null,
		Description = DatabaseName

	DELETE SEIDR.LoaderJob
	DELETE SEIDR.SSIS_Package
	
	DELETE SEIDR.DocMetaDataColumn
	DELETE SEIDR.DocMetaData
	
	DELETE SEIDR.QueueRejection
	
	UPDATE REFERENCE.Project 
	SET Description = 'ProjectID ' + CONVERT(varchar(30), ProjectID),
		FTP_RootFolderOverride = null,
		Metrix_RootFolderName_Override = null,
		Source_RootFolderOverride = null

	UPDATE REFERENCE.Organization
	SET Description = 'Organization ' + CONVERT(varchar(30), OrganizationID),
		FTP_RootFolder = null,
		Metrix_RootFolderName = null,
		Source_RootFolder =null
	
	DELETE SEIDR.PGPJob	
	DELETE SEIDR.JobProfile_Job_SettingsFile

	TRUNCATE TABLE SEIDR.FileSizeHistory

RETURN 0
