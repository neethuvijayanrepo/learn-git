
CREATE PROCEDURE [SEIDR].[usp_JobExecution_u_LiveVox]
	@JobExecutionID	BIGINT,
	@ExportBatchID BIGINT =NULL,    
 	@CampaignID smallint = NULL  
	--Default value is provided so that caller doesn't have to conditional pass DBNULL
AS
BEGIN
	IF(@ExportBatchID IS NOT NULL)
	BEGIN
		UPDATE SEIDR.JobExecution
		SET METRIX_ExportBatchID=@ExportBatchID,
		ExecutionStatusCode='M',  
		UserKey =  @CampaignID   
		WHERE JobExecutionID = @JobExecutionID
	END
	ELSE
	BEGIN
		UPDATE SEIDR.JobExecution
		SET NotNeeded=1,
		DD=GETDATE()
		WHERE JobExecutionID = @JobExecutionID
	END
END	