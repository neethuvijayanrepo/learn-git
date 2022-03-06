

CREATE VIEW [SEIDR].[vw_Schedule]
AS
SELECT s.ScheduleID, s.Description [Schedule], s.ForSequenceControl,
src.ScheduleRuleClusterID, src.Description [ScheduleRuleCluster], 
sr.ScheduleRuleID, sr.Description [ScheduleRule], 
sr.PartOfDateType, sr.PartOfDate, 
sr.IntervalType, sr.IntervalValue, 
sr.[Hour], sr.[Minute], p.Description [PartOfDateTypeExplanation]
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