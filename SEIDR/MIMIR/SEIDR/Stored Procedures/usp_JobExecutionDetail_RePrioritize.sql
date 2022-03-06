CREATE PROCEDURE [SEIDR].[usp_JobExecutionDetail_RePrioritize]	
	@BatchSize int = 5
AS
BEGIN
	
	CREATE TABLE #ExecutionIDList(JobExecutionID bigint primary key)

	UPDATE TOP (@BatchSize) je
	SET PrioritizeNow = 0,
		JobPriority = COALESCE(np.PriorityCode, je.JobPriority)
	OUTPUT INSERTED.JobExecutionID INTO #ExecutionIDList(JobExecutionID)
	FROM SEIDR.JobExecution je	
	JOIN SEIDR.[Priority] p
		ON je.JobPriority = p.PriorityCode
	JOIN SEIDR.JobProfile jp
		ON je.JobProfileID = jp.JobProfileID
	JOIN SEIDR.[Priority] p2
		ON jp.JobPriority = p2.PriorityCode
	OUTER APPLY(SELECT TOP 1 *
				FROM SEIDR.[Priority]
				WHERE PriorityValue > p.PriorityValue
				AND PriorityValue <= p2.PriorityValue + 2 --Limit how much the job execution priority can auto increase
				ORDER BY PriorityValue ASC)np
	WHERE InWorkQueue = 1
	AND PrioritizeNow = 1

	SELECT * 
	FROM SEIDR.vw_JobExecution
	WHERE JobExecutionID IN (SELECT JobExecutionID FROM #ExecutionIDList) --Remove any DelayStart, increase workPriority

	RETURN 0
END