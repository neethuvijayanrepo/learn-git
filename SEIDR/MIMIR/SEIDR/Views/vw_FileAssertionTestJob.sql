CREATE VIEW [SEIDR].[vw_FileAssertionTestJob] AS
	SELECT jpj.JobProfile_JobID, 
	jpj.Description, 
	jpj.StepNumber,
	j.JobName, j.JobNameSpace,
	fs.ExpectedOutputFile, 
	CONFIG.ufn_GetShortHandPath(ExpectedOutputFile, jp.OrganizationID, jp.ProjectID, UserKey1, jp.LoadProfileID) [ShortHandExpectedOutputFile], 		
	fs.CheckColumnNameMatch,
	fs.CheckColumnOrderMatch,
	fs.SkipColumns,
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM [SEIDR].[FileAssertionTestJob] fs
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