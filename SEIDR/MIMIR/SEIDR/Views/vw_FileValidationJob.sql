

CREATE VIEW [SEIDR].[vw_FileValidationJob]
AS
	SELECT FileValidationJobID, jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,
	SkipLines, HasHeader, DoMetaDataConfiguration, CurrentMetaDataVersion, 
	Delimiter, SizeThreshold, SizeThresholdDayRange,SizeThresholdWarningMode, 
	NotificationList, HasTrailer, TextQualifier,RemoveTextQual, TextQualifyColumnNumber,
	MinimumColumnCountForMerge, KeepOriginal, OverrideExtension, LineEnd_CR, LineEnd_LF,	
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  
	o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.FileValidationJob fv
	JOIN SEIDR.JobProfile_Job jpj
		ON fv.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID
		AND s.Active = 1
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	WHERE jpj.Active = 1
	AND jp.Active = 1