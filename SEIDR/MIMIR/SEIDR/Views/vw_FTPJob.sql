



CREATE VIEW [SEIDR].[vw_FTPJob]
AS
	SELECT j.FTPJobID, jpj.JobProfile_JobID, jpj.Description, jpj.StepNumber,  
	p.Protocol, a.TransferResumeSupport, o.OperationName [FTPOperation], 
	j.LocalPath, CONFIG.ufn_GetShortHandPath_Profile(LocalPath, jp.JobProfileID) [ShortHandLocalPath],
	j.RemotePath,
	j.RemoteTargetPath,
	j.[Delete], j.Overwrite, j.DateFlag,	
	a.FTPAccountID,	a.Description [FTPAccount], 
	CASE WHEN a.DD IS NULL AND a.OrganizationID IS NOT NULL then CAST(1 as bit) else CAST(0 as bit) end [FTPAccount_IsValid],
	a.[Server], a.Port, a.UserName, a.OrganizationID [FTPAccount_OrganizationID], a.ProjectID [FTPAccount_ProjectID], 
	jpj.JobProfileID, jp.Description [JobProfile], 
	jp.OrganizationID, org.Description [Organization], 
	jp.ProjectID, proj.[Description] as [Project], proj.CRCM, proj.Modular, proj.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.FTPJob j
	JOIN SEIDR.FTPAccount a
		ON j.FTPAccountID = a.FTPAccountID		
	JOIN SEIDR.FTPProtocol p
		ON a.FTPProtocolID = p.FTPProtocolID
	JOIN SEIDR.FTPoperation o
		ON j.FTPoperationID = o.FTPOperationID
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