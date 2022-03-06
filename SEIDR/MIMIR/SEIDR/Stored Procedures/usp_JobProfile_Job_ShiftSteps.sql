CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_ShiftSteps]
	@JobProfileID int,
	@StepNumberStart tinyint,
	@Shift int = 1,
	@StatusFilter bit = 0, -- if true, limit to status/namespace combinations that match following parameters
	@TriggerExecutionStatus varchar(20) = null,
	@TriggerExecutionNameSpace varchar(128) = null
AS
BEGIN
	SET NOCOUNT ON

	UPDATE SEIDR.JobProfile_Job
	SET StepNumber += @Shift
	WHERE JobProfileID = @JobProfileID
	AND StepNumber >= @StepNumberStart
	AND 
	(
		@StatusFilter = 0 -- Can match or not, will still include
		OR 
		--Check matches
		(TriggerExecutionStatusCode is null and @TriggerExecutionStatus IS NULL OR TriggerExecutionStatusCode = @TriggerExecutionStatus)
		AND
		(TriggerExecutionNameSpace is null and @TriggerExecutionNameSpace is null OR TriggerExecutionNameSpace = @TriggerExecutionNameSpace)
	)

	exec SEIDR.usp_JobProfile_help @JobProfileID
END