CREATE VIEW [METRIX].[vw_ExportSettings]
	AS 
	SELECT  jpj.JobProfile_JobID, j.JobName, j.JobNameSpace,
	es.ExportSettingsId,
	jpj.Description, 
	jpj.StepNumber,
	es.ArchiveLocation,
	es.MetrixDatabaseLookupID, 
	db.Description [MetrixDatabaseLookup],
	es.VendorID, v.Description [VendorName],
	es.ExportTypeID, ex.Description [ExportType],
	es.ImportTypeID, im.Description [ImportType],
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.Job j
	JOIN SEIDR.JobProfile_Job jpj
		ON j.JobID = jpj.JobID
		--ON fs.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID
	LEFT JOIN METRIX.ExportSettings es
		ON jpj.JobProfile_JobID = es.JobProfile_JobID
	LEFT JOIN METRIX.Vendor v
		ON es.VendorID = v.VendorID
	LEFT JOIN METRIX.ExportType ex
		ON es.ExportTypeID = ex.ExportTypeID
	LEFT JOIN METRIX.ImportType im
		ON es.ImportTypeID = im.ImportTypeID
	LEFT JOIN SEIDR.DatabaseLookup db
		ON es.MetrixDatabaseLookupID = db.DatabaseLookupID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.scheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE jpj.Active = 1
	AND jp.Active = 1
	AND (j.JobNameSpace = 'METRIX_EXPORT' OR es.ExportSettingsId IS NOT NULL)
GO

