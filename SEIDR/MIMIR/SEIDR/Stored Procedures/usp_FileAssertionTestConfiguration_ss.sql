CREATE PROCEDURE [SEIDR].[usp_FileAssertionTestConfiguration_ss]
	@JobProfile_JobID int
AS
BEGIN
	SELECT *
	FROM SEIDR.FileAssertionTestJob
	WHERE JobProfile_JobID = @JobProfile_JobID
END
GO