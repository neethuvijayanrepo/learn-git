CREATE PROCEDURE [SEIDR].[usp_FileConcatenationJob_ss]
	@JobProfile_JobID int
AS
	SELECT SecondaryFile, HasHeader, SecondaryFileHasHeader, OutputPath
	FROM SEIDR.FileConcatenationJob
	WHERE JobProfile_JobID = @JobProfile_JobID
RETURN 0