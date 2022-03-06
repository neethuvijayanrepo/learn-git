CREATE PROCEDURE [SEIDR].[usp_JobExecutionError_d]
	@JobExecutionID bigint,
	@StepNumber tinyint = null,
	@JobProfile_JobID int = null,
	@ExtraID int = null
AS
	IF @StepNumber is null AND @JobProfile_JobID is null
	BEGIN
		RAISERROR('Deleting current step errors.', 0, 0) WITH NOWAIT;
		DELETE e
		FROM SEIDR.JobExecutionError e
		JOIN SEIDR.JobExecution je
			ON e.JobExecutionID = je.JobExecutionID
			AND e.StepNumber = je.StepNumber
		WHERE e.JobExecutionID = @JobExecutionID
		AND (@ExtraID is null or e.ExtraID = @ExtraID or e.ExtraID is null)
	END
	ELSE
	BEGIN
		RAISERROR('Deleting specified step/configuration errors.', 0, 0) WITH NOWAIT;
		DELETE e
		FROM SEIDR.JobExecutionError e
		WHERE JobExecutionID = @JobExecutionID
		AND (@ExtraID is null or e.ExtraID = @ExtraID or e.ExtraID is null)
		AND (@StepNumber is null or e.StepNumber = @StepNumber)
		AND (@JobProfile_JobID is null or e.JobProfile_JobID = @JobProfile_JobID)
	END
RETURN 0
