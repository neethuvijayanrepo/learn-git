CREATE VIEW [REFERENCE].[vw_Schedule]
AS
SELECT s.ScheduleID, s.Description [Schedule], 
SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) [TimeOffset],
SEIDR.ufn_Schedule_GetDescription(s.ScheduleID, SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME())) [ComputedDescription],
s.ForSequenceControl,
src.ScheduleRuleClusterID, src.Description [ScheduleRuleCluster], 
sr.ScheduleRuleID, sr.Description [ScheduleRule], 
sr.PartOfDateType, sr.PartOfDate, 
CASE 
WHEN sr.PartOfDateType = 'dw' then 
	CASE WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.Hour < 0  then DATENAME(dw, ((7 + sr.PartOfDate - 1) % 7) - 2)
		WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.Hour > 23  then DATENAME(dw, ((7 + sr.PartOfDate + 1) % 7) - 2)
		ELSE DATENAME(dw, sr.PartOfDate - 2)
	end
END [ShiftedDayOfWeek],
CASE 
WHEN sr.PartOfDateType = 'dw' then 
	CASE WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.Hour < 0  then ISNULL(NULLIF(((7 + sr.PartOfDate - 1) % 7), 0), 7) 
		WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.Hour > 23  then ISNULL(NULLIF(((7 + sr.PartOfDate + 1) % 7), 0), 7) 
		ELSE sr.PartOfDate
	end
END [ShiftedPartOfDate],
sr.IntervalType, 
sr.IntervalValue, 
sr.[Hour],
CASE WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.[Hour] < 0 then 24 + sr.Hour + SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME())
WHEN SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.[Hour] > 23 then (SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.[Hour]) % 24
ELSE SECURITY.ufn_GetTimeOffset_UserName(SUSER_NAME()) + sr.[Hour]
END [ShiftedHour],
sr.[Minute], 
p.Description [PartOfDateTypeExplanation]
FROM SEIDR.Schedule s
JOIN SEIDR.Schedule_ScheduleRuleCluster ssrc
	On s.ScheduleID = ssrc.ScheduleID	
JOIN SEIDR.ScheduleRuleCluster src
	ON ssrc.ScheduleRuleClusterID = src.ScheduleRuleClusterID
JOIN SEIDR.ScheduleRuleCluster_ScheduleRule srcsr
	ON src.ScheduleRuleClusterID = srcsr.ScheduleRuleClusterID
JOIN SEIDR.ScheduleRule sr
	ON srcsr.ScheduleRuleID = sr.ScheduleRuleID
LEFT JOIN SEIDR.ScheduleDatePart p
	ON sr.PartOfDateType = p.PartOfDateType
WHERE sr.Active = 1
AND s.Active = 1
AND src.Active = 1