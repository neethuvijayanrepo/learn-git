
CREATE VIEW [SEIDR].[vw_BatchScriptJob]
	AS 
	SELECT 
		BatchScriptJobID,  
		jpj.JobProfile_JobID, 
		jpj.Description, 
		jpj.StepNumber,
		BatchScriptPath,
		'(JobExecution.FilePath)' as Parameter1,
		'(JobExecution.JobExecutionID)' as Parameter2,
		Parameter3,
		Parameter4,
		job.Valid,
		jpj.CanRetry, jpj.RetryLimit, jpj.RetryDelay,
		jpj.RequiredThreadID, 
		jpj.FailureNotificationMail, 
		jpj.RetryCountBeforeFailureNotification,
		jp.JobProfileID, 
		jp.Description [JobProfile], 
		jp.OrganizationID, 
		o.Description [Organization], 
		jp.ProjectID, 
		p.[Description] as [Project], 
		p.CRCM, 
		p.Modular, 
		p.Active [ProjectActive],
		jp.UserKey1, 
		jp.UserKey2,
		jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch
	FROM SEIDR.BatchScriptJob job
	JOIN SEIDR.JobProfile_Job jpj
		ON job.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE job.Active = 1
	AND jpj.Active = 1
	AND jp.Active = 1
