


CREATE VIEW [SEIDR].[vw_JobProfile_Job]
	AS 
	SELECT 
		jp.JobProfileID,
		jp.Description [JobProfile],
		jp.OrganizationID,  o.Description [Organization], 
		jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive], 
		jp.LoadProfileID, jp.UserKey1 as [UserKey], jp.UserKey1, jp.UserKey2,
		jpj.JobProfile_JobID, jpj.StepNumber, jpj.Description [Step], jpj.Branch, jpj.TriggerBranch,
		TriggerExecutionStatusCode, TriggerExecutionNameSpace, 
		jpj.SequenceScheduleID, s.Description [SequenceSchedule],
		CASE WHEN EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent WITH (NOLOCK) WHERE JobProfile_JobID = jpj.JobProfile_jobID) then 1 else 0 end [HasParent],
		jpj.CanRetry, jpj.RetryLimit, jpj.RetryDelay,
		COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID) [RequiredThreadID],
		j.JobID, j.JobName, j.JobNameSpace, j.Description [Job], j.ConfigurationTable, j.Loaded, FailureNotificationMail
	FROM SEIDR.JobProfile jp
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	JOIN SEIDR.JobProfile_Job jpj
		ON jp.JobProfileID = jpj.JobProfileID
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID
		AND s.Active = 1
	WHERE jp.Active = 1 AND jpj.Active = 1
