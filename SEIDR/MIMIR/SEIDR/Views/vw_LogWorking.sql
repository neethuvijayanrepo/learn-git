CREATE VIEW SEIDR.vw_LogWorking
 AS
 SELECT
	*
	FROM SEIDR.vw_LogLatest -- Maintain as subset of the LogLatest view
	WHERE JobExecutionID IN (SELECT JObExecutionID FROM SEIDR.JobExecution WHERE IsWorking = 1 AND Active = 1)