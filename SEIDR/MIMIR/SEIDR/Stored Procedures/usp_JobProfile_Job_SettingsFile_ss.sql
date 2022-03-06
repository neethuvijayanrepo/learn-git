CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_SettingsFile_ss]
	@JobProfile_JobID int
AS
	SELECT *
	FROM SEIDR.JobProfile_Job_SettingsFile
	WHERE JobProfile_JobID = @JobProfile_JobID
RETURN 0
