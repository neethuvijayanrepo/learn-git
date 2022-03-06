



CREATE VIEW [SEIDR].[vw_SpawnJob]
AS
	SELECT SpawnJobID, jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,
	
	jp2.JobProfileID [TargetJobProfileID], jp2.Description [TargetJobProfile],
	sp.SourceFile, CONFIG.ufn_GetShortHandPath_Profile(sp.SourceFile, jp.JobProfileID) ShortHandSourceFile,
	jp2.UserKey1 [TargetUserKey1], jp2.UserKey2 [TargetUserKey2],	
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.SpawnJob sp
	JOIN SEIDR.JobProfile_Job jpj
		ON sp.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	JOIN SEIDR.JobPRofile jp2
		ON sp.JobProfileID = jp2.JobProfileID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleiD = s.ScheduleID
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE jpj.Active = 1
	AND jp.Active = 1
	AND sp.Active = 1