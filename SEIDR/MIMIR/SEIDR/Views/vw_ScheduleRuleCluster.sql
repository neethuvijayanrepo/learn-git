


CREATE VIEW [SEIDR].[vw_ScheduleRuleCluster]
AS
SELECT 
src.ScheduleRuleClusterID, src.Description [ScheduleRuleCluster], 
sr.ScheduleRuleID, sr.Description [ScheduleRule], 
sr.PartOfDateType, sr.PartOfDate, 
sr.IntervalType, sr.IntervalValue, 
sr.[Hour], sr.[Minute], p.Description [PartOfDateTypeExplanation]
FROM SEIDR.ScheduleRuleCluster src	
JOIN SEIDR.ScheduleRuleCluster_ScheduleRule srcsr
	ON src.ScheduleRuleClusterID = srcsr.ScheduleRuleClusterID
JOIN SEIDR.ScheduleRule sr
	ON srcsr.ScheduleRuleID = sr.ScheduleRuleID
LEFT JOIN SEIDR.ScheduleDatePart p
	ON sr.PartOfDateType = p.PartOfDateType
WHERE sr.Active = 1
AND src.Active = 1