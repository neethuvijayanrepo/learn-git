CREATE PROCEDURE [SEIDR].[usp_JobProfile_SetStopAfterStepNumber]
	@JobProfileID int,
	@StopAfterStepNumber tinyint
AS
	UPDATE SEIDR.JobProfile
	SET StopAfterStepNumber = @StopAfterStepNumber
	WHERE JobProfileID = @JobProfileID
RETURN 0
