CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_SettingsFile_iu]
	@FilePath varchar(256),
	@jobProfileID int = null,
	@StepNumber smallint = null,
	@JobProfile_JobID int = null	
AS
	IF (@jobProfileID is not null AND @StepNumber is not null)
	BEGIN
		SELECT @JobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileiD = @jobProfileID AND StepNumber = @StepNumber AND Active = 1
	END
	IF @JobProfile_JobID is null
	BEGIN
		RAISERROR('Insufficient Information to determine @JobProfile_JobID', 16, 1)
		RETURN
	END

	IF NOT EXISTS(SELECT null FROM SEIDR.JobProfile_Job_SettingsFile WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		INSERT INTO SEIDR.JobProfile_Job_SettingsFile(JobProfile_JobID, SettingsFilePath)
		VALUES(@JobProfile_JobID, @FilePath)
	END
	ELSE
	BEGIN
		UPDATE SEIDR.JobProfile_Job_SettingsFile
		SET SettingsFilePath = @FilePath
		WHERE JobProfile_JobID = @JobProfile_JobID
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job_SettingsFile
	WHERE JobProfile_JobID = @JobProfile_JobID

RETURN 0
