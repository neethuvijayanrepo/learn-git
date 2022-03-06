
CREATE PROCEDURE [SEIDR].[usp_JobExecution_Rollback]
	@JobExecutionID bigint,
	@StepNumber smallint, 
	@RequireOriginalStepActive bit = 1, 
	@JobExecution_ExecutionStatusID bigint = null
AS	
	IF NOT EXISTS(SELECT null FROM SEIDR.vw_JobExecutionHistory WHERE JobExecutionID = @JobExecutionID AND StepNumber = @StepNumber)
	BEGIN
		RAISERROR('No valid step available from the history.', 16, 1)
		RETURN
	END

	IF @RequireOriginalStepActive = 1
	AND NOT EXISTS(SELECT null
					FROM SEIDR.vw_JobExecutionHistory h
					jOIN SEIDR.JobProfile_Job jpj
						ON h.JobProfile_JobID = jpj.JobProfile_JobID
					WHERE jpj.Active = 1
					AND h.JobExecutionID = @JobExecutionID
					AND h.StepNumber = @StepNumber
					AND (h.IsLatestForExecutionStep = 1 and @JobExecution_ExecutionStatusID is null 
						or JobExecution_ExecutionStatusID = @JobExecution_ExecutionStatusID) 
					)
	BEGIN
		RAISERROR('JobProfile_Job configuration is no longer active.', 16, 1)
		RETURN
	END

	UPDATE je
	SET ExecutionStatusCode = s.ExecutionStatusCode,
		ExecutionStatusNameSpace = s.ExecutionStatusNameSpace,
		StepNumber = @StepNumber,
		FilePath = s.FilePath,
		FileHash = s.FileHash,
		FileSize = s.FileSize,
		Branch = s.Branch,
		PreviousBranch = s.PreviousBranch,
		ProcessingDate = s.ProcessingDate,
		PrioritizeNow = CASE WHEN InWorkQueue = 1 then 1
							else PrioritizeNow end
	FROM SEIDR.JobExecution je
	JOIN SEIDR.JobExecution_ExecutionStatus s
		ON je.JobExecutionID = s.JobExecutionID
		AND s.StepNumber = @StepNumber
		AND 
		(
			IsLatestForExecutionStep = 1 and @JobExecution_ExecutionStatusID is null 
			or JobExecution_ExecutionStatusID = @JobExecution_ExecutionStatusID
		) 
	WHERE je.JobExecutionID = @JobExecutionID
	
	IF @@ROWCOUNT = 0 OR @@ERROR <> 0
	BEGIN
		RAISERROR('Unable to update JobExecution.', 16, 3)
		RETURN
	END

	
	SELECT * FROM SEIDR.vw_JobExecution WITH (NOLOCK) WHERE JobExecutionID = @JobExecutionID
RETURN 0
