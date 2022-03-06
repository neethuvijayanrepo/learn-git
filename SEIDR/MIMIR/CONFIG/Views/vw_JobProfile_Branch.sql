CREATE VIEW [CONFIG].[vw_JobProfile_Branch]
	AS SELECT  
		je.JobProfileID, 
		jp.Description [JobProfile], 
		jp.UserKey1 [UserKey], 
		jp.OrganizationID, 
		o.Description [Organization],
		je.JobProfile_JobID,
		je.Branch, 
		je.Description [Step], 
		jc.JobName [JobName],
		je.StepNumber, 
		COALESCE(jes.IsError,0) [ErrorHandler],				
		es.ExecutionStatus PotentialNextExecutionStatus, 
		CONVERT(bit, CASE WHEN jpj.JobProfile_JobID IS NULL THEN 0 ELSE 1 END) MatchFound,
		es.Description [PotentialNextExecutionStatusDescription], 
		es.IsError [PotentialNextStatusIsError],
		jpj.JobProfile_JobID [NextJobProfile_JobID],
		CASE WHEN jpj.JobProfile_JobID IS NULL THEN '(NO MATCH FOUND)' ELSE jpj.Description end [NextStep], 
		CASE WHEN es.IsError = 1 then je.StepNumber 
			else je.StepNumber + 1 
			end [PotentialNextStepNumber],
		jpj.Branch [PotentialNextBranch],
		j.JobName [PotentialNextJobName]
	FROM [SEIDR].[JobProfile_Job] je	
	JOIN SEIDR.Job jc
		ON je.JobID = jc.JobID	
	JOIN SEIDR.JobProfile jp
		ON je.JobProfileID = jp.JobProfileID
		AND jp.Active = 1
	JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN SEIDR.ExecutionStatus jes
		ON je.TriggerExecutionNameSpace = jes.[NameSpace]
		AND je.TriggerExecutionStatusCode = jes.ExecutionStatusCode
		AND jes.IsError = 1
	CROSS JOIN SEIDR.ExecutionStatus es
	LEFT JOIN SEIDR.JobProfile_Job jpj
		ON jpj.JobProfile_JobID = SEIDR.ufn_GetJobProfile_JobID(je.JobProfileID, 
																CASE WHEN es.IsError = 1 then je.StepNumber else je.StepNumber + 1 end, 
																es.ExecutionStatusCode, 
																es.[NameSpace], 
																je.Branch)
	LEFT JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID			
	WHERE es.IsComplete = 0
	AND es.[NameSpace] IN ('SEIDR', jc.JobNameSpace, jpj.TriggerExecutionNameSpace)
	AND je.Active = 1
	--AND (es.isError = 0 OR jpj.JobProfile_JobID IS NOT NULL)
	AND es.ExecutionStatus NOT IN ('SEIDR.S', 'SEIDR.R', 'SEIDR.SP', 'SEIDR.M', 
									'SEIDR.CX', 'SEIDR.X', 'SEIDR.PD') 	 
	-- Statuses for execution kick off shouldn't need to be looked at.
	--Also shouldn't need to worry about CX at this time either.
GO