


CREATE VIEW [SEIDR].[vw_FileConcatenationJob]
AS
	SELECT jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,
	HasHeader, 
	SecondaryFile, CONFIG.ufn_GetShortHandPath(SecondaryFile, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) ShortHandSecondaryFile,
	SecondaryFileHasHeader, 
	OutputPath,	CONFIG.ufn_GetShortHandPath(OutputPath, jp.OrganizationID, jp.ProjectID, jp.UserKey1, Jp.LoadProfileID) [ShortHandOutputPath],
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  
	o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive], 
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit,jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.FileConcatenationJob c
	JOIN SEIDR.JobProfile_Job jpj
		ON c.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID
		AND s.Active = 1
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE jpj.Active = 1
	AND jp.Active = 1