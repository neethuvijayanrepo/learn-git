CREATE PROCEDURE [SEIDR].[usp_JobExecution_ss_Cancel]	
AS
	DECLARE @JobID table (JobExecutionID int)

	UPDATE TOP (1) je
	SET ExecutionStatusCode = 'SA'
	OUTPUT INSERTED.JobExecutionID INTO @JobID(JobExecutionID)
	FROM SEIDR.JobExecution je
	WHERE ExecutionStatus = 'SEIDR.SX'

	SELECT * 
	FROM SEIDR.JobExecution je
	JOIN @JobID j
		ON je.JobExecutionID = j.JobExecutionID
RETURN 0
