CREATE VIEW [SEIDR].[vw_StagingToAndromedaExportStatusJob]
AS
	SELECT jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,  
	j.CheckProject, j.CheckOrganization,
	j.IgnoreProcessingDate,
	j.RequireCurrentProcessingDate,
	j.MonitoredOnly,
	j.IgnoreUnusedProfiles,
	j.LoadBatchTypeList,
	j.DatabaseLookupID,
	db.Description [DatabaseLookup],
 
	jpj.JobProfileID, jp.Description [JobProfile], 
	jp.OrganizationID, org.Description [Organization], 
	jp.ProjectID, proj.[Description] as [Project], proj.CRCM, proj.Modular, proj.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.StagingToAndromedaExportStatusJob j	
	JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON j.JobProfile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID 
	JOIN SEIDR.DatabaseLookup db
		ON j.DatabaseLookupID = db.DatabaseLookupID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Project proj
		ON jp.ProjectID = proj.ProjectID
	LEFT JOIN REFERENCE.Organization org
		ON jp.OrganizationID = org.OrganizationID
	WHERE jp.Active = 1 AND jpj.Active = 1