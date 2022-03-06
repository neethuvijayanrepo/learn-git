



CREATE VIEW [SEIDR].[vw_SettingsFile] AS
	SELECT jpj.JobProfile_JobID, 
	jpj.Description, 
	jpj.StepNumber,
	j.JobName, j.JobNameSpace,
	fs.SettingsFilePath, 
	CONFIG.ufn_GetShortHandPath(SettingsFilePath, jp.OrganizationID, jp.ProjectID, UserKey1, jp.LoadProfileID) [ShortHandSettingsFilePath], 		
	UTIL.ufn_PathItem_GetName(SettingsFilePath) [SettingsFile],	
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM [SEIDR].[JobProfile_Job_SettingsFile] fs
	JOIN SEIDR.JobProfile_Job jpj
		ON fs.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.scheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE jpj.Active = 1
	AND jp.Active = 1