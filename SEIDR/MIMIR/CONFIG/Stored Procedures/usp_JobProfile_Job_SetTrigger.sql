
CREATE PROCEDURE CONFIG.usp_JobProfile_Job_SetTrigger
	@JobProfile_JobID int = null,
	@JobProfileID int = null,
	@StepNumber smallint = null,
	@CurrentTriggerExecutionStatusCode varchar(2) = null,
	@CurrentTriggerExecutionNameSpace varchar(50) = null,
	@TriggerExecutionStatusCode varchar(2),
	@TriggerExecutionNameSpace varchar(50)
AS
BEGIN
	IF @JobProfile_JobID IS NULL
	BEGIN
		SELECT @JobProfile_JobID = JobProfile_JobID 
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileiD = @JobProfileID
		AND StepNumber = @StepNumber
		AND Active = 1
		AND ISNULL(@TriggerExecutionStatusCode, '') = ISNULL(TriggerExecutionStatusCode, '')
		AND ISNULL(@TriggerExecutionNameSpace, '') = ISNULL(TriggerExecutionNameSpace, '')
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Unable to identify step. Consider passing @JobProfile_JobID instead.', 16, 1)
		END
	
	END

	UPDATE SEIDR.JobProfile_Job
	SET TriggerExecutionStatusCode = @TriggerExecutionStatusCode,
		TriggerExecutionNameSpace = @TriggerExecutionNameSpace
	WHERE JobProfile_JobID = @JobProfile_JobID

	SELECT * 
	FROM SEIDR.vw_JobProfile_Job
	WHERE JobProfile_JobID = @JobProfile_JobID
END