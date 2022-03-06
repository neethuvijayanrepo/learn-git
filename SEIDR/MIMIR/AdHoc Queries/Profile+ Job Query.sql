SELECT * 
FROM SEIDR.vw_JobExecution je 
JOIN SEIDR.Job j
	ON je.JobID = j.JobID
WHERE je.JobProfileID = 12

SELECT jp.JobProfileID, jp.Description [JobProfile], jp.RegistrationValid, jp.ScheduleValid, jp.JobPriority,
	jpj.StepNumber,jpj.TriggerExecutionNameSpace, jpj.TriggerExecutionStatusCode,
	jpj.CanRetry, jpj.RequiredThreadID, 
	j.JobID, j.JobNameSpace, j.JobName, j.Description, j.SingleThreaded,
	jp.SuccessNotificationMail, jpj.FailureNotificationMail
FROM SEIDR.JobProfile jp
JOIN SEIDR.JobProfile_Job jpj
	ON jp.JobProfileID = jpj.JobProfileID
	AND jpj.Active = 1
JOIN SEIDR.Job j
	ON jpj.JobID = j.JobID
WHERE jp.Active = 1 
ORDER BY jp.JobProfileID, jpj.StepNumber ASC, jpj.TriggerExecutionNameSpace, jpj.TriggerExecutionStatusCode