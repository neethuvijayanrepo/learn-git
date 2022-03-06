

CREATE VIEW [SEIDR].[vw_JobLog]
AS

SELECT
ID, l.ThreadID, /*ThreadType,*/ je.JobProfileID, jp.Description [JobProfile], je.JobExecutionID, je.ProcessingDate, 
	jpj.JobProfile_JobID, jpj.StepNumber, j.JobID, j.Description [Job], MessageType, [LogTime], LogMessage
FROM SEIDR.Log l
--LEFT
 JOIN SEIDR.JobExecution je
	ON l.JobExecutionID = je.JobExecutionID
--LEFT
 JOIN SEIDR.JobProfile jp
	ON je.JobProfileID = jp.JobProfileID
--LEFT 
JOIN SEIDR.JobProfile_Job jpj
	ON l.JobProfile_JobID = jpj.JobProfile_JobID
--LEFT 
JOIN SEIDR.Job j
	ON jpj.JobID = j.JobID