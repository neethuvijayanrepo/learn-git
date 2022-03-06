CREATE VIEW [CONFIG].[vw_JobExecution_NextStep]
	AS SELECT je.JobExecutionID, 
		je.JobProfileID, je.ProcessingDate,
		je.Branch, je.StepNumber, je.ExecutionStatus,		
		es.ExecutionStatus PotentialNextExecutionStatus, es.IsError,
		jpj.JobProfile_JobID,
		jpj.Description [PotentialNextStep], 
		CASE WHEN es.IsError = 1 then je.StepNumber else je.StepNumber + 1 end [PotentialNextStepNumber],
		jpj.Branch [PotentialNextBranch],
		j.JobName [PotentialNextJob]
	FROM [SEIDR].[JobExecution] je	
	CROSS JOIN SEIDR.ExecutionStatus es
	JOIN SEIDR.JobProfile_Job jpj
		ON jpj.JobProfile_JobID = SEIDR.ufn_GetJobProfile_JobID(je.JobProfileID, 
																CASE WHEN es.IsError = 1 then je.StepNumber else je.StepNumber + 1 end, 
																es.ExecutionStatusCode, 
																es.[NameSpace], 
																je.Branch)
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	JOIN SEIDR.JobProfile_Job jpjc --If there's a next step, needs to be a current step.
		ON je.JobProfile_JobID = jpjc.JobProfile_JobID
	JOIN SEIDR.Job jc
		ON jpjc.JobID = jc.JobID
	WHERE es.IsComplete = 0
	AND es.[NameSpace] IN ('SEIDR', jc.JobNameSpace)
		
