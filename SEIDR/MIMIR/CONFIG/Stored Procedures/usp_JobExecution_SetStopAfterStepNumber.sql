CREATE PROCEDURE [SEIDR].[usp_JobExecution_SetStopAfterStepNumber]
	@JobExecutionID bigint,
	@StopAfterStepNumber tinyint
AS
	DECLARE @Note varchar(2000) = 'Set StopAfterStepNumber = ' + ISNULL(CONVERT(VARCHAR(20), @StopAfterStepNumber), '(NULL)')
	
	exec SEIDR.usp_JobExecution_Note_i @JobExecutionID, @Note, @Technical = 1

	UPDATE SEIDR.JobExecution
	SET StopAfterStepNumber = @StopAfterStepNumber
	WHERE JobExecutionID = @JobExecutionID

	RETURN 0
