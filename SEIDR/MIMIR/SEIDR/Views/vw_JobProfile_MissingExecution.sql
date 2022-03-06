CREATE VIEW SEIDR.vw_JobProfile_MissingExecution
AS
SELECT jp.JobProfileID, jp.Description, d.[Date] [ProcessingDate], DATENAME(dw, d.Date) [ProcessingDayOfWeek], CONVERT(varchar, d.[Date], 0) ProcessingDateExpanded, 
jp.ScheduleID, s.Description [Schedule], ScheduleFromDate, ScheduleThroughDate, ScheduleNoHistory, 
jp.RegistrationDestinationFolder, FileFilter, jp.Creator, JobPriority
FROM SEIDR.JobProfile jp
LEFT JOIN SEIDR.Schedule s
	ON jp.ScheduleID = s.ScheduleID
CROSS APPLY SEIDR.ufn_GetDays(COALESCE(jp.ScheduleFromDate, CONVERT(date, jp.DC)), jp.ScheduleThroughDate) d
WHERE NOT EXISTS(SELECT null FROM SEIDR.JobExecution WITH (NOLOCK) WHERE JobProfileID = jp.JobProfileID AND ProcessingDate = d.[Date])
AND jp.Active = 1 AND (ScheduleValid = 1 or RegistrationValid = 1)
AND (
	jp.ScheduleID is null
	OR EXISTS(SELECT null -- There is an earlier JobExecution, whose ProcessingDate should have allowed the schedule to create a JobExecution
				FROM SEIDR.JobExecution WITH (NOLOCK)
				CROSS APPLY (SELECT SEIDR.ufn_CheckSchedule(jp.ScheduleID, d.Date, ProcessingDate)) sch(Qualifying)
				WHERE JobProfileID = jp.JobProfileID AND ProcessingDate < d.Date
				AND sch.Qualifying is not null
				)
	)