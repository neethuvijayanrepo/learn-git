
CREATE VIEW CONFIG.vw_JobProfileHistory
AS
	SELECT JobProfileID, JobProfileHistoryID, ChangeSummary as TriggerChangeSummary, TriggeringUser,
	'EXEC CONFIG.usp_JobProfile_Rollback @JobProfileID = ' + CONVERT(varchar(30), JobProfileID) + ', @JobProfileHistoryID = ' + CONVERT(varchar(30), JobProfileHistoryID)
	as [RollbackCommand],
		OrganizationID, ProjectID, LoadProfileID, 
		CONCAT(UserKey1, '|' + UserKey2) UserKey,
		RegistrationFolder, FileFilter, FileDateMask, RegistrationDestinationFolder,
		ScheduleID, ScheduleFromDate, ScheduleThroughDate, DC, AgeMinutes, AgeHours, AgeDays
	FROM SEIDR.JobProfileHistory