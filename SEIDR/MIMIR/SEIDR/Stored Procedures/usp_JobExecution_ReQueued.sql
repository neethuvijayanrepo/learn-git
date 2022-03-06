
 CREATE PROCEDURE SEIDR.usp_JobExecution_ReQueued
	@JobExecutionID bigint
AS
BEGIN
	UPDATE SEIDR.JObExecution
	SET IsWorking = 0, InWorkQueue = 1
	WHERE JObExecutionID = @JobExecutionID
END