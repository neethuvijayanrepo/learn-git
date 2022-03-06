CREATE PROCEDURE [SEIDR].[usp_JobProfile_CheckSchedule]
AS
BEGIN
	/*
	--Note the difference here: Time component can cause the day of the schedule to drop off due to the comparison used in the function for the date range. 
	DECLARE @CHK date = GETDATE() - 10
	SELECT @CHK
	SELECT * from SEIDR.ufn_GetDays(@CHK, null) 

	
	DECLARE @CHK2 datetime = GETDATE() - 10
	SELECT @CHK2
	SELECT * from SEIDR.ufn_GetDays(@CHK2, null) 


	*/ 
	DECLARE @RC int = 0 --seconds to delay next call to this procedure
	
	--Test for missing days of schedules, between the last JobExecution processed and every day until today
	DECLARE @Now datetime = GETDATE()
	DECLARE @TODAY date = CONVERT(date, @NOW)

	CREATE TABLE #JobSchedule(JobProfileID int not null, ScheduleID int  not null, ScheduleDate datetime not null, 
			ComparisonDate datetime not null, 
			[MatchingRuleClusterID] int null, 
			[Match] as (CONVERT(bit, CASE WHEN MatchingRuleClusterID is null then 0 else 1 end)),
			--Today as CASE WHEN CONVERT(date, ScheduleDate) = CONVERT(date, GETDATE()) then 1 else 0 end,
			PRIMARY KEY(JobProfileID, ScheduleID, ScheduleDate))
	INSERT INTO #JobSchedule(JobProfileID, ScheduleID, ScheduleDate, ComparisonDate)
	SELECT jp.JobProfileID, jp.ScheduleID, d.[Date], LastProcessDate
	FROM SEIDR.JobProfile jp
	JOIN (SELECT JobProfileID, MAX(ProcessingDate) LastProcessDate
			FROM SEIDR.JobExecution 
			--WHERE Active = 1
			GROUP BY JobProfileID)je
		ON jp.JobProfileID = je.JobProfileID
	CROSS APPLY SEIDR.ufn_GetDays(jp.ScheduleFromDate, jp.ScheduleThroughDate) d
	WHERE jp.ScheduleValid = 1 AND jp.Active = 1
	AND d.[Date] < @TODAY --Don't include today here, checked without temp table below.
	AND d.[Date] > je.LastProcessDate

	UPDATE #JobSchedule
	SET ScheduleDate = DATEADD(minute, 23 * 60 + 59, ScheduleDate) --Historical, set to max datepart
	--WHERE Today = 0


	UPDATE js
	SET [MatchingRuleClusterID] = SEIDR.ufn_CheckSchedule(js.ScheduleID, ScheduleDate, ComparisonDate)
	FROM #JobSchedule js

	--If not using an interval, may insert multiple days for a single profile here!
	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
			StepNumber, ExecutionStatusCode, 
			ProcessingDate, 
			ScheduleRuleClusterID,OrganizationID,ProjectID,LoadProfileID)
	SELECT distinct js.JobProfileID, jp.UserKey, jp.UserKey1, jp.UserKey2, 
			1, 'S',
			CASE WHEN jp.ScheduleNoHistory =1 then @Now --When profile says no history, use today's date for the ProcessingDate (Date not null).
				else ScheduleDate end,					-- Else use the matched date (so we get all the days in between)
			[MatchingRuleClusterID],jp.OrganizationID,jp.ProjectID,jp.LoadProfileID
	FROM #JobSchedule js
	JOIN SEIDR.JobProfile jp
		ON js.JobProfileID = jp.JobProfileID
	WHERE [Match] = 1
	AND (jp.ScheduleNoHistory = 0 OR ScheduleThroughDate is null OR @Now <= ScheduleThroughDate)
	AND NOT EXISTS(SELECT null
					FROM SEIDR.JobExecution
					WHERE JobProfileID = js.JobProfileID
                    AND ProcessingDate = (CASE WHEN jp.ScheduleNoHistory = 1 then @TODAY else CONVERT(date, js.ScheduleDate) end)
					)
	
	IF @@ROWCOUNT = 0
		SET @RC += 20



	--For today's executions (not historical/using the temp table)
	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
		StepNumber, ExecutionStatusCode, ScheduleRuleClusterID,
		ProcessingDate, ProcessingTime,OrganizationID,ProjectID,LoadProfileID)
	SELECT js.JobProfileID, UserKey, UserKey1, UserKey2, 
			1, 'S', [MatchingRuleClusterID],
			@Now, @Now,js.OrganizationID,js.ProjectID,js.LoadProfileID
	FROM SEIDR.JobProfile js
	JOIN(SELECT JobProfileID, MAX(ProcessingDateTime) ProcessingDateTime
			FROM SEIDR.JobExecution
			GROUP BY JobProfileID)je
		ON js.JobProfileID = je.JobProfileID
	CROSS APPLY(SELECT SEIDR.ufn_CheckSchedule(js.ScheduleID, @Now, ProcessingDateTime))s(MatchingRuleClusterID)
	WHERE js.ScheduleValid = 1 AND js.Active = 1	
	AND  s.MatchingRuleClusterID is not null
	AND @Now >= js.ScheduleFromDate  --Part of ScheduleValid, but avoid timing issues at the very end of the day
	and (js.ScheduleThroughDate is null or @Now <= js.ScheduleThroughDate)
	AND NOT EXISTS(SELECT null
					FROM SEIDR.JobExecution
					WHERE JobProfileID = js.JobProfileID
                    AND ProcessingDate = @TODAY
					--AND ScheduleRuleClusterID IS NOT NULL
					)
	
	IF @@ROWCOUNT = 0
		SET @RC += 30

	--Clear temp table, check missing days for jobs that have never run before
	DELETE #JobSchedule
	INSERT INTO #JobSchedule(JobProfileID, ScheduleID, ScheduleDate, ComparisonDate)
	SELECT jp.JobProfileID, jp.ScheduleID, d.[Date], jp.ScheduleFromDate
	FROM SEIDR.JobProfile jp
	CROSS APPLY SEIDR.ufn_GetDays(jp.ScheduleFromDate, jp.ScheduleThroughDate) d --Note that this keeps the days going into the table within the profile's From/Through date
	WHERE jp.ScheduleValid = 1 AND jp.Active = 1
	AND NOT EXISTS(SELECT null FROM SEIDR.JobExecution WHERE JobProfileID = jp.JobProfileID)
	AND d.Date < @TODAY
	
	UPDATE #JobSchedule
	SET ScheduleDate = DATEADD(minute, 24 * 60 -1, ScheduleDate) --Historical (ScheduleDate  is in at least a day in the past), set time to max datepart


	
	UPDATE js
	SET [MatchingRuleClusterID] = SEIDR.ufn_CheckSchedule(js.ScheduleID, ScheduleDate, ComparisonDate)
	FROM #JobSchedule js

	--SELECT * FROM #JobSchedule

	--If not using an interval, may insert multiple days for a single profile here!
	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
			StepNumber, ExecutionStatusCode, 
			ProcessingDate, 
			ScheduleRuleClusterID,OrganizationID,ProjectID,LoadProfileID)
	SELECT distinct js.JobProfileID, jp.UserKey, jp.UserKey1, jp.UserKey2, 
			1, 'S',
			CASE WHEN jp.ScheduleNoHistory =1 then @Now --When profile says no history, use today's date for the ProcessingDate (Date not null).
				else ScheduleDate end,					-- Else use the matched date (so we get all the days in between)
			[MatchingRuleClusterID],jp.OrganizationID,jp.ProjectID,jp.LoadProfileID
	FROM #JobSchedule js --already considers from/through date
	JOIN SEIDR.JobProfile jp
		ON js.JobProfileID = jp.JobProfileID
	WHERE [Match] = 1
	AND (jp.ScheduleNoHistory = 0 OR ScheduleThroughDate is null OR @Now <= ScheduleThroughDate)
	AND NOT EXISTS(SELECT null
					FROM SEIDR.JobExecution
					WHERE JobProfileID = js.JobProfileID
                    AND ProcessingDate = CASE WHEN jp.ScheduleNoHistory = 1 then @TODAY else CONVERT(date, js.ScheduleDate) end
					)
	
	

	/****************************************************/
	/*********	INITIAL INSERTS FROM SCHEDULE	*********/
	/****************************************************/



	--The very first/initial execution for new schedules, matching against today since temp table does not include today's date.
	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
		StepNumber, ExecutionSTatusCode, ScheduleRuleClusterID,
		ProcessingDate, ProcessingTime,OrganizationID,ProjectID,LoadProfileID) --	DECLARE @NOW Datetime = GETDATE()
	SELECT distinct JobProfileID, UserKey, UserKey1, UserKey2, 
			1, 'S', [MatchingRuleClusterID],
			CASE WHEN js.ScheduleNoHistory = 1 then @now else js.ScheduleFromDate end, --Should technically be okay to just do @Now, since the only reason we should need this is due to not including today's date in the temp table (because temp table is for missing days, not for today's executions)
			@Now,js.OrganizationID,js.ProjectID,js.LoadProfileID
	FROM SEIDR.JobProfile js		
	CROSS APPLY(SELECT SEIDR.ufn_CheckSchedule(js.ScheduleID, @Now, js.ScheduleFromDate))s(MatchingRuleClusterID)
	WHERE js.ScheduleValid = 1 AND js.Active = 1	
	AND @Now >= js.ScheduleFromDate  --Part of ScheduleValid, but we're not using the temp table here, so avoid timing issues.
	and (js.ScheduleThroughDate is null or @Now <= js.ScheduleThroughDate)
	AND NOT EXISTS(SELECT null 
					FROM SEIDR.JobExecution WITH (NOLOCK)
					WHERE JobProfileID = js.JobProfileID
					AND ProcessingDate >= js.ScheduleFromDate)
	AND  s.MatchingRuleClusterID is not null	

	IF @@ROWCOUNT = 0
		SET @RC += 5

	--Very first/Initial insert for when the schedule is set up using interval instead of datepart 
	--(otherwise, it will start X days after the ScheduleFromDate)
	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
	StepNumber, ExecutionSTatusCode, ScheduleRuleClusterID,
	ProcessingDate, ProcessingTime,OrganizationID,ProjectID,LoadProfileID) --	DECLARE @NOW Datetime = GETDATE()
	SELECT distinct JobProfileID, UserKey, UserKey1, UserKey2, 
			1, 'S', [MatchingRuleClusterID],
			CASE WHEN js.ScheduleNoHistory = 1 then @now else js.ScheduleFromDate end,
			@Now, js.OrganizationID,js.ProjectID,js.LoadProfileID
	FROM SEIDR.JobProfile js	
	CROSS APPLY(SELECT TOP 1 s.ScheduleRuleClusterID 
				FROM SEIDR.vw_Schedule s
				LEFT JOIN SEIDR.vw_Schedule s2
					ON s.ScheduleRuleClusterID = s2.ScheduleRuleClusterID
					AND 
					(	
						-- Ensure we're matching non-interval rules for the Cluster (e.g., Monday at 10)
						s.IntervalType is NULL AND SEIDR.ufn_CheckScheduleRule(s2.ScheduleRuleID, @Now, js.ScheduleFromDate) = 0
						--OR 
						--IntervalType IS NOT NULL AND s2.Hour > s.Hour 
						-- s would need to be the latest hour for interval types.
						-- Comment out for now - shouldn't really need to mix interval types within a cluster...
					)
				WHERE s.ScheduleID = js.ScheduleID 
				AND s.IntervalType IS NOT NULL
				AND DATEPART(hour, @now) >= s.[Hour]
				AND DATEPART(minute, @now) >= s.[Minute]
				AND s2.ScheduleRuleID is null
				)s(MatchingRuleClusterID)	
	WHERE js.ScheduleValid = 1 AND js.Active = 1	
	AND @Now >= js.ScheduleFromDate  --Part of ScheduleValid, but make sure we don't have timing issues since we're not using the temp table here.
	and (js.ScheduleThroughDate is null or @Now <= js.ScheduleThroughDate)
	AND NOT EXISTS(SELECT null 
					FROM SEIDR.JobExecution WITH (NOLOCK)
					WHERE JobProfileID = js.JobProfileID
					AND ProcessingDate >= js.ScheduleFromDate)	

	IF @@ROWCOUNT = 0
		SET @RC += 5


	RETURN @RC
END
