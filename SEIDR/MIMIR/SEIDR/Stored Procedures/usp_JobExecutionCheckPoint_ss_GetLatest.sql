CREATE PROCEDURE [SEIDR].[usp_JobExecutionCheckPoint_ss_GetLatest]
	@JobExecutionID bigint,
	@JobID int = null,
	@JobProfile_JobID int = null
AS
	IF @JobID is null
		SELECT TOP 1 * FROM SEIDR.JobExecutionCheckPoint WHERE JobExecutionID = @JobExecutionID ORDER BY CheckPointID DESC
	ELSE
		SELECT TOP 1 *
		FROM SEIDR.JobExecutionCheckPoint
		WHERE JobExecutionID = @JobExecutionID	
		AND (@JobProfile_JobID is null or JobProfile_JobID = @JobProfile_JobID)
		AND @JobID = JobID
		ORDER BY CheckPointID DESC
RETURN 0
