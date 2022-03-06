CREATE PROCEDURE [SEIDR].[usp_StagingToAndromedaExportStatusJobSettings_ss]
	@JobProfile_JobID int
AS
	SELECT *
	FROM SEIDR.StagingToAndromedaExportStatusJob 
	WHERE JobProfile_JobID = @JobProfile_JobID
	--AND Active = 1 --	1-1, so let JobProfile_Job drive active.
RETURN 0
