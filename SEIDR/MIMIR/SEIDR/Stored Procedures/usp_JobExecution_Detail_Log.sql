
CREATE PROCEDURE [SEIDR].[usp_JobExecution_Detail_Log](
@JobExecutionID int,
@JobExecution_ExecutionStatusID int
)
AS
BEGIN
SELECT 
	JobExecution_ExecutionStatusID,
	ID, MessageType, 
	ThreadID, 
	LogMessage, 
	LogTime  
FROM SEIDR.vw_LogStepLatest
WHERE JobExecutionID = @JobExecutionID AND JobExecution_ExecutionStatusID = @JobExecution_ExecutionStatusID
End