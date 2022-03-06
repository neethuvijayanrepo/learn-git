CREATE VIEW [SEIDR].[vw_DemoMapJob]
	AS 
	SELECT m.DemoMapID, jpj.JobProfile_JobID, 
	jpj.Description, 	
	jpj.StepNumber,	
	j.JobName,
	m.Delimiter,
	m.OutputDelimiter,
	m.FilePageSize,	
	m.FileMapID,
	m.FileMapID as [PackageID],
	OutputFolder,
	CONFIG.ufn_GetShortHandPath_Profile(m.OutputFolder, jp.JobProfileID) ShortHandOutputFolder,

	m._InsuranceBalanceUnavailable,
	m._InsuranceDetailUnavailable,
	m._PartialDemographicLoad,
	m._PatientBalanceUnavailable,
	m.Enable_OOO,
	m.DoAPB,
	m.HasHeaderRow,
	m.FileMapDatabaseID,
	db.Description [FileMapDatabase],
	m.PayerLookupDatabaseID,
	pdb.Description [PayerLookupDatabase],
	m.OOO_InsuranceBalanceValidation,
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.DemoMapJob m
	JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON m.JobProfile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp  WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID
	JOIN SEIDR.DatabaseLookup db WITH (NOLOCK)
		ON m.FileMapDatabaseID = db.DatabaseLookupID
	JOIN SEIDR.DatabaseLookup pdb WITH (NOLOCK)
		ON m.PayerLookupDatabaseID = pdb.DatabaseLookupID
	JOIN REFERENCE.Organization o WITH (NOLOCK)
		ON jp.OrganizationID = o.OrganizationID
	JOIN SEIDR.Job j WITH (NOLOCK)
		ON jpj.JobID = j.JobID
	LEFT JOIN SEIDR.Schedule s WITH (NOLOCK)
		ON jpj.SequenceScheduleID = s.scheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Project p WITH (NOLOCK)
		ON jp.ProjectID = p.ProjectID
	WHERE jpj.Active = 1
	AND jp.Active = 1