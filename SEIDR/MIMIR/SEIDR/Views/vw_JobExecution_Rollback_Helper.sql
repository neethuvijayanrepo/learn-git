CREATE VIEW SEIDR.vw_JobExecution_Rollback_Helper
AS
	SELECT JobExecution_ExecutionStatusID, jpj.JobProfileID,
			h.JobExecutionID, h.StepNumber, h.JobProfile_JobID, 
			jpj.Description [StepDescription], jpj.JobID,
			h.ExecutionStatus, h.ProcessingDate, h.FilePath, Success, h.RetryCount, jpj.CanRetry, jpj.RetryLimit, IsLatestForExecutionStep, h.Branch, h.PreviousBranch,
			'exec SEIDR.usp_JobExecution_Rollback @JobExecutionID = ' + CONVERT(varchar(30), h.JobExecutionID) + ', @StepNumber = ' + CONVERT(varchar(10), h.StepNumber)
			+ CASE WHEN IsLatestForExecutionStep = 1 then '' else ', @RequireOriginalStepActive = 0, @JobExecution_ExecutionStatusID = ' + CONVERT(varchar(30), h.JobExecution_ExecutionStatusID)
			end as [RollbackCommand],
			RollbackHint = 'Roll back to Step Number ' + CONVERT(varchar(10), h.StepNumber) + ', ExecutionStatus "' + h.ExecutionStatus 
						+ CASE WHEN h.ExecutionStatus <> je.ExecutionStatus OR h.StepNumber <> je.StepNumber 
								then '" (from Step Number ' + CONVERT(varchar(10), je.StepNumber) + ', ExecutionStatus "' + je.ExecutionStatus + '")' else '"' end
						+ CASE WHEN IsLatestForExecutionStep = 0 then ', do not check that it is the latest for the step. Use this exact history record.'
							else ', use the history which is the "LatestForExecutionStep" - the most recent attempt.'
							end
						+ CHAR(13) + CHAR(10) + CASE WHEN h.FilePath is not null then 'FilePath = "' + h.FilePath + '"' else 'Remove FilePath (Set NULL).' end
						+ CASE WHEN h.Branch <> je.Branch then CHAR(13) + CHAR(10) + 'Set Branch = "' + h.Branch + '"' else '' end
						+ CASE WHEN h.PreviousBranch <> je.PreviousBranch then CHAR(13) + CHAR(10) + 'Set PreviousBranch = "' + h.PreviousBranch + '"' else '' end
						+ CHAR(13) + CHAR(10) + CASE WHEN ISNULL(h.FilePath, '') <> ISNULL(je.FilePath, '') then 'Current FilePath = "' + ISNULL(je.FilePath, '(NULL)') + '"' else 'Matches Current FilePath' end
	FROM SEIDR.JobExecution_ExecutionStatus h  WITH (NOLOCK)
	JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON h.JobProfile_JobID = jpj.JobProfile_JobID
		AND jpj.Active = 1
	JOIN SEIDR.JobExecution je WITH (NOLOCK)
		ON h.JobExecutionID = je.JobExecutionID
	WHERE je.Active = 1