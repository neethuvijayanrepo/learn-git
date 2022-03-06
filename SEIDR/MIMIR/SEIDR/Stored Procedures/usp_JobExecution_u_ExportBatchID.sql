CREATE PROCEDURE [SEIDR].[usp_JobExecution_u_ExportBatchID]
	@JobExecutionID	BIGINT,
	@METRIX_ExportBatchID BIGINT,
	@ExecutionStatusCode varchar(2) output --Treat as OUTPUT ONLY	
AS
	SET @ExecutionStatusCode = 'M' --For updating the CALLER, not to allow the caller to control

	UPDATE SEIDR.JobExecution
	SET METRIX_ExportBatchID=@METRIX_ExportBatchID,
		ExecutionStatusCode= @ExecutionStatusCode
	WHERE JobExecutionID = @JobExecutionID	
