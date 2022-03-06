

CREATE VIEW [REFERENCE].[vw_JobExecutionHistory]
as
	SELECT JobExecution_ExecutionStatusID,
		jes.JobExecutionID,
		jes.ProcessingDate,
		jes.Success,
		jes.RetryCount,
		DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), jes.DC) [HistoryLogTime],
		--jes.DC [HistoryLogTime],
		jes.ExecutionTimeSeconds,
		UTIL.ufn_PathItem_GetName(jes.FilePath) [FileName],
		je.LoadProfileID,
		je.JobProfileID,
		je.METRIX_LoadBatchID,
		je.UserKey1,
		je.UserKey2,
		je.OrganizationID,
		o.Description [Organization],
		je.ProjectID,
		p.Description [Project],
		p.CRCM,
		p.Modular,
		p.FromDate,
		p.ThroughDate,
		jes.StepNumber,
		jes.JobProfile_JobID,
		jpj.Description [JobProfile_Job],
		jpj.JobID,
		j.JobName,
		jpj.TriggerExecutionStatusCode,
		jpj.TriggerExecutionNameSpace,
		jes.ExecutionStatus [JobExecutionTriggeringStatus],
		jes.FileHash,
		jes.FileSize, 
		jes.FilePath
	FROM SEIDR.JobExecution_ExecutionStatus jes
	JOIN SEIDR.JobProfile_Job jpj
		ON jes.JobProfile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	JOIN SEIDR.JobExecution je
		ON jes.JobExecutionID = je.JobExecutionID
	LEFT JOIN REFERENCE.Organization o
		ON je.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON je.ProjectID = p.ProjectID
	WHERE jes.IsLatestForExecutionStep = 1
	AND je.active = 1