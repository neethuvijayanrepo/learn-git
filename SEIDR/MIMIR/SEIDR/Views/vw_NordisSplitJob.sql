

CREATE VIEW [SEIDR].[vw_NordisSplitJob]
AS
	SELECT jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,  
	jpj.JobProfileID, jp.Description [JobProfile], 
	jp.OrganizationID, org.Description [Organization], 
	jp.ProjectID, proj.[Description] as [Project], proj.CRCM, proj.Modular, proj.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	JOIN SEIDR.JobProfile jp WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID 
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Project proj
		ON jp.ProjectID = proj.ProjectID
	LEFT JOIN REFERENCE.Organization org
		ON jp.OrganizationID = org.OrganizationID
	WHERE jp.Active = 1 AND jpj.Active = 1
	AND jpj.JobID = (SELECT JobID FROM SEIDR.Job WHERE JobNameSpace = 'FileSystem' AND JobName = 'NordisSplitJob')