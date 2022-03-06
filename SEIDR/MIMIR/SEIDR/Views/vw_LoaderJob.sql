 CREATE VIEW [SEIDR].[vw_LoaderJob]
 AS
 SELECT
	lj.JobProfile_JobID, jpj.Description, jpj.StepNumber,
	p.Category [PackageCategory], p.Name [PackageName], p.ServerName, p.PackagePath, p.PackageID,
	lj.OutputFolder, CONFIG.ufn_GetShortHandPath(lj.OutputFolder, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) [ShortHandOutputFolder],
	lj.OutputFileName, 
	lj.SecondaryFilePath, CONFIG.ufn_GetShortHandPath(lj.SecondaryFilePath, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) [ShortHandSecondaryFilePath],
	lj.TertiaryFilePath, CONFIG.ufn_GetShortHandPath(lj.TertiaryFilePath, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) [ShortHandTertiaryFilePath],

	lj.Misc, lj.Misc2, lj.Misc3,

	lj.DatabaseConnectionManager, 
	lj.DatabaseConnection_DatabaseLookupID,
	db.Description [DatabaseConnection_DatabaseLookup],

	lj.ServerInstanceName, lj.AndromedaServer, lj.FacilityID, lj.DatabaseName,
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, proj.[Description] as [Project], proj.CRCM, proj.Modular, proj.Active [ProjectActive], 
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
 FROM SEIDR.LoaderJob lj
 JOIN SEIDR.SSIS_Package p
	ON lj.PackageID = p.PackageID
JOIN SEIDR.JobProfile_Job jpj
	ON lj.JobProfile_JobID = jpj.JobProfile_JobID
LEFT JOIN SEIDR.Schedule s
	ON jpj.SequenceScheduleID = s.ScheduleID
JOIN SEIDR.JobProfile jp 
	ON jpj.JobProfileID = jp.JobProfileID	
LEFT JOIN REFERENCE.Project proj
	ON jp.ProjectID = proj.ProjectID
LEFT JOIN REFERENCE.Organization o
	ON jp.OrganizationID = o.OrganizationID
LEFT JOIN SEIDR.DatabaseLookup db
	ON lj.DatabaseConnection_DatabaseLookupID = db.DatabaseLookupID	
WHERE jpj.Active = 1
	AND jp.Active = 1
	AND lj.IsValid = 1