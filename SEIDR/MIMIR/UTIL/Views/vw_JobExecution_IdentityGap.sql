CREATE VIEW UTIL.vw_JobExecution_IdentityGap
AS
SELECT je.JobExecutionID, NextID, NextID - JobExecutionID - 1 [GapRecordCount], JobExecutionID + 1 [GapStart], NextID - 1 [GapEnd]
FROM SEIDR.JobExecution je
CROSS APPLY(SELECT MIN(JobExecutionID) NextID
			FROM SEIDR.JobExecution 
			WHERE JObExecutionID > je.JobExecutionID) r2
WHERE 1=1
--AND NOT EXISTS(SELECT null FROM SEIDR.JobExecution WHERE JobExecutionID = je.JobExecutionID + 1)
AND NextID > JobExecutionID + 1