CREATE PROCEDURE SEIDR.usp_JobExecution_Requeue
	@JobExecutionID bigint
AS
BEGIN
	
	UPDATE SEIDR.JobExecution
	SET IsWorking =0 
	WHERE JobExecutionID = @JobExecutionID
	
	SELECT * FROM SEIDR.vw_JobExecution WHERE JobExecutionID = @JobExecutionID AND CanQueue = 1 AND InSequence = 1

END