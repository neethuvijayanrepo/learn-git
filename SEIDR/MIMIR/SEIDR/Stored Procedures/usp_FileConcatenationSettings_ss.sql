CREATE PROCEDURE [SEIDR].[usp_FileConcatenationSettings_ss]
	@JobProfile_JobID int
AS
	SELECT SecondaryFile [SecondaryFilePath], HasHeader, SecondaryFileHasHeader, OutputPath
	FROM SEIDR.FileConcatenationJob
	WHERE JobProfile_JobID = @JobProfile_JobID
RETURN 0