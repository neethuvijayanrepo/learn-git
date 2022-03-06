
CREATE VIEW [REFERENCE].[vw_JobExecutor_Threads]
AS
	SELECT 
	n.Number [ThreadID],
	je.JobExecutionID, je.JobName, je.OffsetLatestLogTime,
	je.FullUserKey, je.ProcessingDate, je.ProcessingDayOfWeek, 
	je.OrganizationID, je.Organization, je.ProjectID, je.Project,
	je.FileName, je.FilePath, je.FileSize, 
	je.StepNumber, je.CurrentStepDescription, je.RetryCount
	FROM (SELECT MAX(ThreadID) [EstimatedMaxThreadID] 
			FROM SEIDR.Log l1 WITH (NOLOCK) 
			WHERE ID > (SELECT MAX(ID) FROM SEIDR.Log WITH (NOLOCK)) - 1000) e
	CROSS APPLY UTIL.ufn_GetRange(1, e.EstimatedMaxThreadID) n
	LEFT JOIN REFERENCE.vw_JobExecution je
		ON je.ThreadID = n.Number
		AND je.IsWorking = 1
		AND je.Active = 1
		AND je.JobProfile_JobID is not null