CREATE VIEW [SEIDR].[vw_JobExecution]  
AS  
SELECT JobExecutionID, je.JobProfileID, jpj.JobProfile_JobID, je.StepNumber,  jpj.Description [Step],
  je.ExecutionStatusCode,  
  je.ExecutionStatusNameSpace, je.ExecutionStatus, 
  je.Branch, je.PreviousBranch,
  je.RetryCount, jpj.CanRetry, jpj.RetryDelay,  
  s.IsComplete,  
  je.IsWorking,  
  s.IsError,  
  [CanQueue] = CONVERT(bit,   
    CASE   
     --WHEN jpj.JobProfile_JobID is null then 0  
     WHEN s.IsComplete = 1 then 0   
     WHEN je.IsWorking = 1 then 0   
     WHEN je.InWorkQueue = 1 then 0 --already queued, skip.    
     --WHEN s.IsError = 1 then 0 --even if it's an error, if it matches up with a job, can queue.  
    ELSE 1   
    end),  
  je.FilePath, je.FileSize, je.FileHash,  
  je.LastExecutionStatusCode, je.LastExecutionStatusNameSpace,  
  je.UserKey, je.UserKey1, je.UserKey2,  
  je.ForceSequence, ProcessingDate, ProcessingTime, ProcessingDateTime,  
   CONVERT(bit, CASE 
				WHEN je.ForceSequence = 1 OR jpj.SequenceScheduleID IS NULL 
					 AND NOT EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent WHERE JobProfile_JobID = jpj.JobProfile_JobID) THEN 1 
				WHEN s.IsComplete = 1 then 1 --lazy. If complete, don't worry about sequence
				WHEN je.IsWorking = 1 --OR je.InWorkQueue =1
					then 1 --If it has already been picked up for work on this step, then be lazy.
				WHEN    EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent WHERE JobProfile_JobID = jpj.JobProfile_JobID) 
				AND NOT EXISTS(
					  SELECT Null
						FROM SEIDR.JobExecution o
						JOIN SEIDR.JobProfile_Job_Parent p
							ON o.JobProfile_JobID = p.JobProfile_JobID
						LEFT JOIN SEIDR.vw_JobExecutionHistory h
							ON p.Parent_JobProfile_JobID = h.JobProfile_JobID
							AND DATEDIFF(day, o.ProcessingDate, h.ProcessingDate) = p.SequenceDayMatchDifference
							AND h.Success = 1
						WHERE o.JobExecutionID = je.JobExecutionID
						GROUP BY o.JobExecutionID
						HAVING COUNT(p.Parent_JobProfile_JobID) = COUNT(h.JobExecutionID) --Number of parents should match the number of (left joined) parent executions
						)
					then 0	--if missing any parent executions, then fail.
				WHEN SequenceScheduleID IS NULL THEN 1			
				ELSE (SELECT CASE WHEN SEIDR.ufn_CheckSchedule(jpj.SequenceScheduleID, CONVERT(datetime, je.ProcessingDate) + CASE WHEN je.ProcessingDate < CONVERT(date, GETDATE()) then CONVERT(time, '23:59') else CONVERT(datetime, CONVERT(time, GETDATE())) end, MAX(jes.ProcessingDate)) is null then 0 else 1 end
						FROM SEIDR.JobExecution_ExecutionStatus jes
						JOIN SEIDR.JobExecution je2
							ON jes.JobExecutionID = je2.JobExecutionID
							AND je2.Active = 1
						WHERE jes.JobProfile_JobID = jpj.JobProfile_JobID
						AND jes.Success = 1 AND jes.IsLatestForExecutionStep = 1
					   ) 
				END) InSequence, --need to check for CanQueue and InSequence  
   -- Schedule function checking existence of a JobExecution_ExecutionStatus record lining up with the processing date   
   -- and Success = 1  
  jpj.SequenceScheduleID,   
  j.SingleThreaded [JobSingleThreaded],  
  pje.PriorityValue as ExecutionPriority,  
  pjp.PriorityValue as ProfilePriority,  
  CASE WHEN s.IsComplete = 1 then null else DATEDIFF(hour, je.LU, GETDATE()) end WorkQueueAge,  
  CASE WHEN s.IsComplete = 1 then null else (DATEDIFF(hour, je.LU, GETDATE()) * 3) + pje.PriorityValue + pjp.PriorityValue + (DATEDIFF(day, je.ProcessingDate, GETDATE())/ 3) end [WorkPriority],  
  j.JobID, j.JobName, j.JobNameSpace, j.ThreadName [JobThreadName], j.Loaded,  
  COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID) RequiredThreadID,  
  jp.SuccessNotificationMail, jpj.[FailureNotificationMail],
  je.OrganizationID, o.Description [Organization],
  je.ProjectID, p.Description [Project],
  je.LoadProfileID,
  jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
  je.TotalExecutionTimeSeconds,
  je.SpawningJobExecutionID,
  je.METRIX_ExportBatchID,
  je.METRIX_LoadBatchID
FROM SEIDR.JobExecution je  
JOIN REFERENCE.Organization o WITH (NOLOCK)
	ON je.OrganizationID = o.OrganizationID
JOIN SEIDR.[Priority] pje  
	ON je.[JobPriority] = pje.PriorityCode  
JOIN SEIDR.ExecutionStatus s  
	ON je.ExecutionStatusNameSpace = s.[NameSpace]  
	AND je.ExecutionStatusCode = s.ExecutionStatusCode   
	AND s.IsComplete = 0 --Ensure that complete jobs do not get picked up for working
JOIN SEIDR.JobProfile_Job jpj  
	ON je.JobProfile_JobID = jpj.JobProfile_JobID
JOIN SEIDR.JobProfile jp  WITH (NOLOCK) -- JobProfile doesn't have too much impact in the configuration here, mainly want to make sure that it's active.
	ON je.JobProfileID = jp.JobProfileID  
JOIN SEIDR.[Priority] pjp  WITH (NOLOCK)
	ON jp.JobPriority = pjp.PriorityCode  
JOIN SEIDR.Job j WITH (NOLOCK)
	ON jpj.JobID = j.JobID
	AND j.Loaded = 1   --Shouldn't really be a concern, but to be safe
LEFT JOIN REFERENCE.Project p
	ON je.ProjectID = p.ProjectID
WHERE je.Active = 1 AND jp.Active = 1 
AND (je.StopAFterStepNumber is null or je.StepNumber <= je.StopAfterStepNumber)

/*
--Potential Safeties for processing? Would want to discuss with Neng first... If a file is picked up, it would show up in JobExecution, but just not get picked up...

AND (jp.ScheduleFromDate is null or je.ProcessingDate >= jp.ScheduleFromDate)
AND (jp.ScheduleThroughDate is null or je.ProcessingDate <= jp.ScheduleThroughDate) 
*/