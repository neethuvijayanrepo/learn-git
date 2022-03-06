
CREATE VIEW REFERENCE.[vw_JobExecution_Note]
	AS 
	SELECT 
	[n].[NoteID], 
	[n].[JobExecutionID], 
	[n].[StepNumber], 
	
	[jpj].[JobProfile_JobID], 
	[h].[ExecutionStatusCode], 	
	[h].[ExecutionStatus], 
	[h].[ProcessingDate],  

	[n].[NoteText], 
	DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null),[n].[DC]) [NoteCreationTime], 
	[n].[UserName], 
	u.DisplayName,
	[n].[NoteSequence], [n].[IsLatest],
	[n].[StepNoteSequence], [n].[StepIsLatest], 
	je.StepNumber [CurrentExecutionStep],
	CONVERT(bit, IIF(n.StepNumber = je.StepNumber, 1, 0)) [ForCurrentExecutionStep],
	[je].[JobProfileID], 
	jp.Description [JobProfile],

	[je].[UserKey], 
	[je].[UserKey1], 
	[je].[UserKey2], 
	[je].[ExecutionStatus] [CurrentExecutionStatus], 
	[je].[IsWorking], 

	[h].[FilePath],  
	[h].[Success], 
	[h].[RetryCount], 
	[h].[ExecutionTimeSeconds],  
	
	[j].[JobID], 
	[j].[JobName], 
	[j].[Description], 
	[j].[JobNameSpace]
	FROM SEIDR.JobExecution_Note n
	LEFT JOIN SECURITY.[User] u
		ON n.UserName = u.UserName
	JOIN SEIDR.JobExecution je
		ON n.JobExecutionID = je.JobExecutionID
	JOIN SEIDR.JobExecution_ExecutionStatus h
		ON h.JobExecutionID = n.JobExecutionID
		AND h.StepNumber = n.StepNumber
		AND h.IsLatestForExecutionStep = 1
	LEFT JOIN SEIDR.JobProfile_Job jpj
		ON jpj.JobProfile_JobID = COALESCE(n.JobProfile_JobID, h.JobProfile_JobID)
	LEFT JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	INNER JOIN SEIDR.JobProfile jp
		ON je.JobProfileID = jp.JobProfileID