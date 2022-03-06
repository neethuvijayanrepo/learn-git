CREATE VIEW [SEIDR].[vw_JobExecution_Detail]
AS
SELECT je.JobExecutionID, UTIL.ufn_PathItem_GetName(je.FilePath) [FileName],
je.OrganizationID, je.ProjectID, je.LoadProfileID, je.userKey1, je.UserKey2, 
je.ProcessingDate [ProcessingDate], DATENAME(dw,je.ProcessingDate) [ProcessingDayOfWeek], 
CONVERT(varchar, je.ProcessingDate, 0) ProcessingDateExpanded, 
je.FilePath, je.FileSize, je.FileHash,
je.ExecutionStatusCode, je.ExecutionStatus, es.Description [ExecutionStatusDescription], 
es.IsError [IsError], es.IsComplete [IsComplete], 
[HasIssue] = CASE WHEN RetryCount > 0 and es.IsComplete = 0 then CAST(1 as bit) else cast(0 as bit) end,
je.ExecutionTimeSeconds, je.TotalExecutionTimeSeconds,
je.StepNumber, jpj.Description [CurrentStep], j.JobName [CurrentJobName],
 je.RetryCount, 
je.JobPriority + '-' + jp.JobPriority [JobPriority],
[DaysBack] =  DATEDIFF(dd, je.ProcessingDate, GETDATE()),
[Today] = IIF(je.ProcessingDate = CONVERT(date, GETDATE()), CONVERT(bit, 1), 0),
--Expected = 0 + je.JobExecutionID is not null? Surprise!
[Expected] = CASE WHEN RegistrationValid = 1 AND jp.DeliveryScheduleID IS NOT NULL
		then CASE WHEN (
						SELECT SEIDR.ufn_CheckSchedule(jp.DeliveryScheduleID, 
						CONVERT(datetime, je.ProcessingDate) + CASE WHEN je.ProcessingDate < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, 
						COALESCE(jp.ScheduleFromDate, jp.DC))
						) 
						is not null 
				then CAST(1 as bit) else 0 end
		WHEN je.ScheduleRuleClusterID IS NOT NULL then 1 
		WHEN je.SpawningJobExecutionID IS NOT NULL THEN 1
		else CAST(RegistrationValid as bit) end, 
jp.DeliveryScheduleID, ds.Description [ExpectedDeliverySchedule],
je.InWorkQueue,
 [CanQueueIfInSequence] = CONVERT(bit,   
    CASE   
     WHEN es.IsComplete = 1 then 0   
     WHEN je.IsWorking = 1 then 0   
     WHEN je.InWorkQueue = 1 then 0 --already queued,  cannot queue again.     
    ELSE 1   
    end),
je.IsWorking,
je.ForceSequence,
InSequence = CASE 
	WHEN es.IsComplete = 1 then CAST(1 as bit) 
	WHEN je.isWorking =1  then CAST(1 as bit)
	WHEN je.ForceSequence = 1 then CAST(1 as bit)
	else CAST( COALESCE((SELECT InSequence FROM SEIDR.vw_JobExecution WHERE JobExecutionID = je.JobExecutionID), 0) as bit)
	end,
p.[ParentStepCount],
jp.JobProfileID, jp.Description [JobProfile], 
src.ScheduleRuleClusterID [QualifyingScheduleRuleClusterID],
src.Description [QualifyingScheduleRuleCluster],
jp.ScheduleValid, jp.ScheduleID, s.Description [Schedule], ScheduleFromDate, ScheduleThroughDate, ScheduleNoHistory, 
jp.RegistrationValid, jp.RegistrationFolder, jp.RegistrationDestinationFolder, FileFilter, jp.FileDateMask,
jp.Creator, jp.[Track], c.[SpawningParentProfileCount], je.SpawningJobExecutionID, n.*, cn.*
FROM SEIDR.JobProfile jp
OUTER APPLY(SELECT COUNT(SpawnJobID) [SpawningParentProfileCount]
			FROM SEIDR.SpawnJob
			WHERE JobProfileID = jp.JobProfileID 
			AND active = 1)c
JOIN SEIDR.JobExecution je
	ON je.JobProfileID = jp.JobProfileID
	AND je.Active = 1
CROSS APPLY(SELECT COUNT(*) [ParentStepCount]
			FROM SEIDR.JobProfile_Job_Parent
			WHERE JobProfile_JobID = je.JobProfile_JobID) p
CROSS APPLY(SELECT COUNT(*) [NoteCount], 
					COUNT(CASE WHEN [Auto] = 1 then 1 end) [AutoNotes],
					COUNT(CASE WHEN [Auto] = 0 then 1 end) [ManualNotes],
					COUNT(CASE WHEN [Technical] = 1 AND [Auto] = 0 then 1 end) [TechnicalNotes],
					COUNT(CASE WHEN [Technical] = 0 AND [Auto] = 0 then 1 end) [NonTechnicalNotes]
			FROM SEIDR.JobExecution_Note WITH (NOLOCK)
			WHERE JobExecutionID = je.JobExecutionID
	)n
CROSS APPLY(SELECT
					COUNT(*) [CurrentStep_NoteCount],
					COUNT(CASE WHEN [Auto] = 1 then 1 end) [CurrentStep_AutoNotes],
					COUNT(CASE WHEN [Auto] = 0 then 1 end) [CurrentStep_ManualNotes],
					COUNT(CASE WHEN [Technical] = 1 AND [Auto] = 0 then 1 end) [CurrentStep_TechnicalNotes],
					COUNT(CASE WHEN [Technical] = 0 AND [Auto] = 0 then 1 end) [CurrentStep_NonTechnicalNotes]
			FROM SEIDR.JobExecution_Note WITH (NOLOCK)
			WHERE JobExecutionID = je.JobExecutionID
			AND JobProfile_JobID = je.JobProfile_JobID 
	)cn
LEFT JOIN SEIDR.Schedule s
	ON jp.ScheduleID = s.ScheduleID
	AND s.Active = 1
LEFT JOIN SEIDR.Schedule ds
	ON jp.DeliveryScheduleID = ds.ScheduleID
LEFT JOIN SEIDR.ScheduleRuleCluster src
	ON je.ScheduleRuleClusterID = src.ScheduleRuleClusterID
JOIN SEIDR.ExecutionStatus es
	ON je.ExecutionStatusCode = es.ExecutionStatusCode
	AND je.ExecutionStatusNameSpace = es.[NameSpace]
LEFT JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	ON je.JobProfile_JobID = jpj.JobProfile_JobID
LEFT JOIN SEIDR.JOb j WITH (NOLOCK)
	ON jpj.JobID = j.JobID
WHERE jp.Active = 1