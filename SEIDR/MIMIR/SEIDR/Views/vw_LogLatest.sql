
 CREATE VIEW [SEIDR].[vw_LogLatest]
 AS
 SELECT
	ID, l.JobExecutionID, status.ExecutionStatus, ThreadName, l.ThreadID, LogMessage, MessageType, LogTime, 
	jpj.StepNumber, jpj.Step,jpj.JobProfile_JobID,
	jpj.JobID, jpj.Job,
	jpj.JobProfileID, jpj.JobProfile, jpj.OrganizationID, 
	jpj.ProjectID, jpj.LoadProfileID, 
	jpj.CanRetry, jpj.RetryLimit, jpj.RetryDelay
FROM SEIDR.Log l
JOIN SEIDR.vw_JobProfile_Job jpj
	ON l.JobProfile_JobID = jpj.JobProfile_JobID
CROSS APPLY(SELECT Description [ExecutionStatus]
			FROM SEIDR.[ExecutionStatus] s
			JOIN SEIDR.[Log] li
				ON li.ID = (SELECT MAX(ID) FROM SEIDR.Log WHERE JobExecutionID = l.JobExecutionID AND LogMessage LIKE 'START - STATUS=%')
				AND REPLACE(li.LogMessage, 'START - STATUS=', '') = s.[NameSpace] + '.' + s.ExecutionStatusCode
			)status
WHERE  ID >= (SELECT MAX(ID) FROM SEIDR.Log WHERE JobExecutionID = l.JobExecutionID AND LogMessage LIKE 'START - STATUS=%')