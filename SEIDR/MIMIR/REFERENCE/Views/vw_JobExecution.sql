


CREATE VIEW [REFERENCE].[vw_JobExecution]
AS
SELECT   
	JobExecutionID,
	je.UserKey1 [UserKey],
	je.UserKey1 + IIF(je.UserKey2 is null, '', '|' + je.UserKey2) [FullUserKey],
	je.ProcessingDate, DATENAME(dw, je.ProcessingDate) [ProcessingDayOfWeek], 
	UTIL.ufn_PathItem_GetName(je.FilePath) [FileName],
	je.FilePath,
	je.FileSize,
	je.StepNumber, je.StopAfterStepNumber,
	je.ExecutionStatusCode,
	je.ExecutionStatus, es.Description [ExecutionStatus Description], 
	es.IsError, es.IsComplete, je.RetryCount,
	DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), je.LU) [LastUpdate],
	EarliestNextRetry = CASE WHEN je.RetryCount > 0 AND je.isWorking = 0 AND es.IsComplete = 0 AND je.Active = 1 
							then DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), DATEADD(minute, jpj.RetryDelay, je.LU))
							END,
	src.Description [ScheduleRuleCluster],
	je.LoadProfileID, je.SpawningJobExecutionID,
	je.Active, je.Duplicate, je.[Manual], je.NotNeeded,
	je.OrganizationID,
	o.Description [Organization],
	je.ProjectID,
	p.Description [Project],
	p.CRCM, p.Modular, p.Active [ProjectActive],
	COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID) RequiredThreadID,
	COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID, CASE WHEN je.isWorking = 1 then l.ThreadID end) ThreadID,
	CASE WHEN je.InWorkQueue = 0 AND je.IsWorking = 0 then null else (DATEDIFF(hour, LU, GETDATE()) * 3) + pje.PriorityValue + pjp.PriorityValue + (DATEDIFF(day, je.ProcessingDate, GETDATE())/ 3) end [WorkPriority],  
	l.LogMessage [LatestLogMessage],
	l.LogTime [LatestLogTime],
	DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), l.LogTime) [OffsetLatestLogTime],
	ljpj.Description [LatestLoggedStep],
	je.JobProfile_JobID,
	jpj.Description [CurrentStepDescription], jpj.CanRetry, jpj.RetryLimit,
	je.ForceSequence, je.InWorkQueue, je.IsWorking, je.PrioritizeNow,
	j.JobName, j.ConfigurationTable,
	jp.JobProfileID, 
	jp.Description [JobProfile],
	[Expected] = CASE WHEN je.ScheduleRuleClusterID is not null then CONVERT(bit, 1)
						WHEN jp.DeliveryScheduleID is null then CONVERT(Bit, 1)
						WHEN (SELECT SEIDR.ufn_CheckSchedule(jp.DeliveryScheduleID, 
								CONVERT(datetime, je.ProcessingDate) + CASE WHEN je.ProcessingDate < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, 
								COALESCE(jp.ScheduleFromDate, jp.DC))) is not null 
							then CAST(1 as bit) 
						else 0 
						end,		
	CASE WHEN je.ScheduleRuleClusterID is not null then s.Description else ds.Description end [ExpectedSchedule],
	je.METRIX_LoadBatchID
FROM SEIDR.JobExecution je WITH (NOLOCK)
JOIN SEIDR.[Priority] pje  WITH (NOLOCK)
	ON je.[JobPriority] = pje.PriorityCode  
JOIN SEIDR.ExecutionStatus es WITH (NOLOCK)
	ON je.ExecutionStatusCode = es.ExecutionStatusCode
	AND je.ExecutionStatusNameSpace = es.[NameSpace]
JOIN SEIDR.JobProfile jp WITH (NOLOCK)
	ON je.JobProfileID = jp.JobProfileID	
JOIN SEIDR.[Priority] pjp  WITH (NOLOCK)
	ON jp.JobPriority = pjp.PriorityCode  
LEFT JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	ON je.JobProfile_JobID = jpj.JobProfile_JobID
LEFT JOIN SEIDR.Job j WITH (NOLOCK)
	ON jpj.JobId =j.JobID
LEFT JOIN REFERENCE.Organization o
	ON je.OrganizationID = o.OrganizationID
LEFT JOIN REFERENCE.Project p
	ON je.ProjectID = p.ProjectID
LEFT JOIN SEIDR.ScheduleRuleCluster src WITH (NOLOCK)
	On je.ScheduleRuleClusterID = src.ScheduleRuleClusterID
LEFT JOIN SEIDR.Schedule ds WITH (READPAST)
	ON jp.DeliveryScheduleID = ds.ScheduleID
LEFT JOIN SEIDR.Schedule s WITH (READPAST)
	ON jp.ScheduleID = s.ScheduleID
OUTER APPLY(SELECT TOP 1 ThreadID, LogMessage, JobProfile_JobID, LogTime
			FROM SEIDR.[Log] WITH (NOLOCK)
			WHERE JobExecutionID = je.JobExecutionID
			ORDER BY id DESC) l
LEFT JOIN SEIDR.JobProfile_Job ljpj
	ON l.JobProfile_JobID = ljpj.JobProfile_JobID