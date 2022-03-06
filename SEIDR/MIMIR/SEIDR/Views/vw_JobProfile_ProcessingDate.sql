


CREATE VIEW [SEIDR].[vw_JobProfile_ProcessingDate]
AS
SELECT jp.JobProfileID, jp.Description, 
jp.OrganizationID,  o.Description [Organization], 
jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
jp.LoadProfileID, 
IIF(je.JobExecutionID is not null, je.UserKey1, jp.userKey1) [UserKey1], 
IIF(je.JobExecutionID is not null, je.UserKey2, jp.UserKey2) [UserKey2], 
d.[Date] [ProcessingDate], DATENAME(dw, d.Date) [ProcessingDayOfWeek], 
CONVERT(varchar, d.[Date], 0) ProcessingDateExpanded, 
je.JobExecutionID, je.FilePath, je.ExecutionStatus, 
je.StepNumber, jpj.Description [CurrentStep], j.JobName [CurrentJobName],
ISNULL(es.Description, '[MISSING JOB EXECUTION]') [ExecutionStatusDescription], 
COALESCE(es.IsError, -1) [IsError], COALESCE(es.IsComplete, 0) [IsComplete], je.RetryCount, je.LU  [JobExecution_LastUpdate],
Case WHEN je.JobExecutionID is null then jp.JobPriority else je.JobPriority + '-' + jp.JobPriority end [JobPriority],
[DaysBack] =  DATEDIFF(dd, d.Date, GETDATE()),
[Today] = IIF(d.Date = CONVERT(date, GETDATE()), CONVERT(bit, 1), 0),
--Expected = 0 + je.JobExecutionID is not null? Surprise!
[Expected] = CASE WHEN RegistrationValid = 1 AND jp.DeliveryScheduleID IS NOT NULL
		then CASE WHEN (SELECT SEIDR.ufn_CheckSchedule(jp.DeliveryScheduleID, 
				CONVERT(datetime, d.Date) + CASE WHEN d.Date < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, 
				COALESCE(jp.ScheduleFromDate, jp.DC))) is not null then CAST(1 as bit) else 0 end
		else CAST(1 as bit) end, --If running from schedule, only include in this view when schedule matches. File watch expects daily by default
 [CanQueue] = CONVERT(bit,   
    CASE   
	WHEN je.JobExecutionID is null then CAST(null as bit)
     WHEN es.IsComplete = 1 then 0   
     WHEN je.IsWorking = 1 then 0   
     WHEN je.InWorkQueue = 1 then 0 --already queued,  cannot queue again.     
    ELSE 1   
    end),
IsWorking,
InSequence = CASE 
	WHEN je.JobExecutionID is null then CAST(null as bit)
	WHEN es.IsComplete = 1 then CAST(1 as bit) 
	WHEN je.isWorking =1  then 1
	WHEN je.ForceSequence = 1 then 1
	else (SELECT InSequence FROM SEIDR.vw_JobExecution WHERE JobExecutionID = je.JobExecutionID)
	end,
jp.DeliveryScheduleID, ds.Description [ExpectedDeliverySchedule],
jp.ScheduleID, s.Description [Schedule], ScheduleFromDate, ScheduleThroughDate, ScheduleNoHistory, 
jp.RegistrationFolder, jp.RegistrationDestinationFolder, FileFilter, jp.FileDateMask,
jp.Creator, jp.[Track], c.[SpawningParentProfileCount],je.METRIX_LoadBatchID
FROM SEIDR.JobProfile jp WITH (NOLOCK)
OUTER APPLY(SELECT COUNT(SpawnJobID) [SpawningParentProfileCount]
			FROM SEIDR.SpawnJob
			WHERE JobProfileID = jp.JobProfileID 
			AND active = 1)c
LEFT JOIN REFERENCE.Organization o
	ON jp.OrganizationID = o.OrganizationID
LEFT JOIN REFERENCE.Project p
	ON jp.ProjectID = p.ProjectID
LEFT JOIN SEIDR.Schedule s
	ON jp.ScheduleID = s.ScheduleID
	AND s.ACtive = 1
LEFT JOIN SEIDR.Schedule ds
	ON jp.DeliveryScheduleID = ds.ScheduleID
CROSS APPLY SEIDR.ufn_GetDays(COALESCE(jp.ScheduleFromDate, (SELECT MIN(ProcessingDate) FROM SEIDR.JobExecution WITH (NOLOCK) WHERE JobProfileID = jp.JobProfileID AND active = 1), CONVERT(date, jp.DC)), jp.ScheduleThroughDate) d
LEFT JOIN SEIDR.JobExecution je WITH (NOLOCK)
	ON je.JobProfileID = jp.JobProfileID
	AND je.ProcessingDate = d.Date
	AND je.Active = 1
LEFT JOIN SEIDR.ExecutionStatus es WITH (NOLOCK)
	ON je.ExecutionStatusCode = es.ExecutionStatusCode
	AND je.ExecutionStatusNameSpace = es.[NameSpace]
LEFT JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	ON je.JobProfile_JobID = jpj.JobProfile_JobID
LEFT JOIN SEIDR.JOb j WITH (NOLOCK)
	ON jpj.JobID = j.JobID
WHERE jp.Active = 1 
AND (
	ScheduleValid = 1 
	or RegistrationValid = 1 
	OR je.JobExecutionID is not null --older records/profiles. 
				--But also SpawnJob - don't need to track until the JobExecution is actually created
	)
AND (
	jp.ScheduleID is null
	OR je.JobExecutionID is not null
	OR EXISTS(SELECT null -- There is an earlier JobExecution, whose ProcessingDate should have allowed the schedule to create a JobExecution
				FROM SEIDR.JobExecution WITH (NOLOCK)
				CROSS APPLY (SELECT SEIDR.ufn_CheckSchedule(jp.ScheduleID, CONVERT(datetime, d.Date) + CASE WHEN d.Date < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, ProcessingDate)) sch(Qualifying)
				WHERE JobProfileID = jp.JobProfileID AND ProcessingDate < d.Date
				AND sch.Qualifying is not null
				)
	)