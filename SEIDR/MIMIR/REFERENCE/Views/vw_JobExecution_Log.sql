
CREATE VIEW [REFERENCE].[vw_JobExecution_Log]
 AS
SELECT l.ID, h.JobExecution_ExecutionStatusID, h.JobExecutionID, h.ProcessingDate,
	h.StepNumber, h.ExecutionStatus, es.Description [StatusDescription], 
	DATEADD(hour, u.TimeOffset, h.DC) [JobExecutionHistoryDC],  
	jpj.JobProfile_JobID, jpj.[Step],
	l.LogMessage, l.ThreadID, DATEADD(hour, u.TimeOffset, l.LogTime) [LogTime], 
	h.Success, l.MessageType, 
	h.FilePath,
	jpj.JobID, jpj.Job,
	jpj.JobProfileID, jpj.JobProfile, jpj.OrganizationID, 
	jpj.ProjectID, jpj.LoadProfileID,  

	jpj.CanRetry, jpj.RetryLimit, jpj.RetryDelay
FROM (SELECT [SECURITY].[ufn_GetTimeOffset](null))u(TimeOffset) 
CROSS JOIN SEIDR.Log l WITH (NOLOCK)
JOIN SEIDR.JobExecution_ExecutionStatus h WITH (NOLOCK)
	ON l.JobExecutionID = h.JobExecutionID
	AND l.JobProfile_JobID = h.JobProfile_JobID
	AND h.IsLatestForExecutionStep = 1
JOIN SEIDR.vw_JobProfile_Job jpj WITH (NOLOCK)
	ON h.JobProfile_JobID = jpj.JobProfile_JobID
JOIN SEIDR.ExecutionStatus es WITH (NOLOCK)
	ON h.ExecutionStatusCode = es.ExecutionStatusCode
	AND h.ExecutionStatusNameSpace = es.[NameSpace]
WHERE ID >= (SELECT MAX(ID) FROM SEIDR.Log  WITH (NOLOCK) WHERE JobExecutionID = l.JobExecutionID AND JobProfile_JobID = l.JobProfile_JobID AND LogMessage LIKE 'START - STATUS=%')