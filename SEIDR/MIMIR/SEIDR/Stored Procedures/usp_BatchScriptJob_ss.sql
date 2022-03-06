CREATE PROCEDURE [SEIDR].[usp_BatchScriptJob_ss]
	@JobProfile_JobID int
AS
	SELECT [BatchScriptPath], [Parameter3], [Parameter4]
	FROM SEIDR.BatchScriptJob
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND Active = 1 AND Valid = 1
RETURN 0
