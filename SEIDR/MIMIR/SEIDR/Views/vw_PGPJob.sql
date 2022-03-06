CREATE VIEW [SEIDR].[vw_PGPJob]
AS
	SELECT j.PGPJobID, jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,  
	o.PGPOperationID, o.PGPOperationName [PGPOperation], o.PGPOperationDescription, 
	j.SourcePath, CONFIG.ufn_GetShortHandPath_Profile(SourcePath, jp.JobProfileID) [ShortHandSourcePath],
	j.OutputPath, CONFIG.ufn_GetShortHandPath_Profile(OutputPath, jp.JobProfileID) [ShortHandOutputPath],	
	j.PublicKeyFile, j.PrivateKeyFile, j.KeyIdentity, j.PassPhrase,
	jpj.JobProfileID, jp.Description [JobProfile], 
	jp.OrganizationID, org.Description [Organization], 
	jp.ProjectID, proj.[Description] as [Project], proj.CRCM, proj.Modular, proj.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.PGPJob j
	JOIN SEIDR.PGPoperation o
		ON j.PGPoperationID = o.PGPOperationID
	JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON j.JobProfile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID 
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.ScheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Project proj
		ON jp.ProjectID = proj.ProjectID
	LEFT JOIN REFERENCE.Organization org
		ON jp.OrganizationID = org.OrganizationID
	WHERE jp.Active = 1 AND jpj.Active = 1