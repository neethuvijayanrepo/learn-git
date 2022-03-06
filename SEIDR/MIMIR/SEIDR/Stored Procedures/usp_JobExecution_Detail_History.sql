

CREATE PROCEDURE [SEIDR].[usp_JobExecution_Detail_History]
@JobExecutionID int
AS
BEGIN
	SELECT 
			jes.JobExecution_ExecutionStatusID,
			r.StepDescription,
			jes.StepNumber,
			es.Description [Status],
			jes.FilePath [StepExecutionFilePath], 
			jes.Success, 
			jes.RetryCount					
			
	FROM SEIDR.JobExecution_ExecutionStatus jes
    JOIN SEIDR.ExecutionStatus es
		ON jes.ExecutionStatusCode = es.ExecutionStatusCode
		AND jes.ExecutionStatusNameSpace = es.[NameSpace]
	JOIN [SEIDR].[vw_JobExecution_Rollback_Helper] r
		ON jes.JobExecution_ExecutionStatusID = r.JobExecution_ExecutionStatusID
	WHERE jes.JobExecutionID = @JobExecutionID
	ORDER BY jes.JobExecution_ExecutionStatusID
End