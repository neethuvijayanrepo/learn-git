--Initial values for schedule
INSERT INTO SEIDR.ScheduleDatePart(partOFDateType, Description)
SELECT [Type], [Desc]
FROM(
SELECT 'eom-d' [Type], 'Match if DateToCheck is X days before the EndOfMonth for the month of DateToCheck (Need >= 0 to match)' [Desc]
UNION ALL SELECT 'eom+d', 'Match if DateToCheck is X days after the EndOfMonth for the month of DateToCheck (Need <= 0 to match)'
--UNION ALL SELECT 'eom-m', 'End of month for X months ago'
--UNION ALL SELECT 'eom+m', 'End of Month for X months in the future'
UNION ALL SELECT 'EOM', 'Match if DateToCheck is the End of Month.'
UNION ALL SELECT 'yyyy', 'Match if the year of DateToCheck matches PartOfDate'
UNION ALL SELECT 'yy', 'Match if the year of DateToCheck matches PartOfDate'
UNION ALL SELECT 'year', 'Match if the year of DateToCheck matches PartOfDate'
UNION ALL SELECT 'wk', 'Match if the week of the year of DateToCheck matches PartOfDate'
UNION ALL SELECT 'week', 'Match if the week of the year of DateToCheck matches PartOfDate'
UNION ALL SELECT 'dy', 'Match if the day of the year for DateToCheck matches PartOfDate'
UNION ALL SELECT 'day', 'Match if the day of the month for DateToCheck matches PartOfDate'
UNION ALL SELECT 'dd', 'Match if the day of the month for DateToCheck matches PartOfDate'
UNION ALL SELECT 'dm', 'Match if the day of the month for DateToCheck matches PartOfDate'
UNION ALL SELECT 'dw', 'Match if the day of the week for DateToCheck matches PartOfDate'
UNION ALL SELECT 'qq', 'Match if the quarter of the year for DateToCheck matches PartOfDate'
UNION ALL SELECT 'mm', 'Match if the month of the year for DateToCheck matches PartOfDate - DATEPART(mm, @DateToCheck)'
)q
WHERE NOT EXISTS(SELECT null FROM SEIDR.ScheduleDatePart WHERE PartOfDateType = q.[Type])



INSERT INTO SEIDR.ScheduleRule(Description, PartOFDateType, PartOfDate, Intervaltype, IntervalValue, hour, Minute)
SELECT distinct CASE WHEN d is not null then  DATENAME(dw, d + 5) else 'Daily' end + ' @ ' + CONVERT(varchar, Hour % 12) + ':' + RIGHT('0' + CONVERT(varchar, Minute), 2) + CASE WHEN hour < 12 then 'AM' else 'PM' end,
	CASE WHEN d is null then null else 'dw' end, CASE WHEN d is null then null else d end,
	CASE WHEN d is null then 'dd' end, case when d is null then 1 end,
	td.[Hour], tdm.[Minute]
--	SELECT * --Note: d + 5 because DATENAME(dw, _) is treating the second parameter as a date serial.
FROM 
(VALUES (7), (1), (2), (3), (4), (5), (6), (null)) as DayWeek(d) 
CROSS JOIN
(VALUES(1), (2), (3), (4), (5), (6), (7) ,(8), (9), (10), (11), (12), (13), (14), (15),(16), (17), (18), (19), (20), (21), (22), (23), (0)) as Td(hour)
CROSS JOIN
(VALUES(0), (30), (45), (15)) as Tdm(minute)
WHERE NOT EXISTS(SELECT null
				FROM SEIDR.ScheduleRule
				WHERE Hour = td.[Hour]
				AND Minute = tdm.[Minute]
				AND (d is null and PartOfDate is null or d = PartOFDate AND PartOfDateType = 'dw')
				)

INSERT INTO SEIDR.ScheduleRuleCluster(Description)
SELECT Description FROM SEIDR.SCheduleRule r
WHERE r.Active = 1 
AND NOT EXISTS(SELECT null FROM SEIDR.ScheduleRuleCluster_ScheduleRule WHERE ScheduleRuleID = r.ScheduleRuleID)

INSERT INTO SEIDR.ScheduleRuleCluster_ScheduleRule(ScheduleRuleID, ScheduleRuleClusterID)
SELECT r.ScheduleRuleID, src.ScheduleRuleClusterID
FROM SEIDR.ScheduleRuleCluster src
JOIN SEIDR.ScheduleRule r
	ON src.Description = r.Description
WHERE r.Active = 1
AND NOT EXISTS(SELECT null FROM SEIDR.ScheduleRuleCluster_ScheduleRule WHERE ScheduleRuleID = r.ScheduleRuleID)


INSERT INTO SEIDR.Schedule(Description)
SELECT Description
FROM SEIDR.ScheduleRuleCluster src
WHERE src.Active = 1
AND NOT EXISTS(SELECT null FROM SEIDR.Schedule_SCheduleRuleCluster WHERE ScheduleRuleClusterID = src.ScheduleRuleClusterID)

INSERT INTO SEIDR.Schedule_ScheduleRuleCluster(ScheduleID, ScheduleRuleClusterID)
SELECT s.ScheduleID, src.ScheduleRuleClusterID
FROM SEIDR.Schedule s
JOIN SEIDR.SCheduleRuleCluster src
	ON s.Description = src.Description
WHERE src.Active = 1
AND NOT EXISTS(SELECT null FROM SEIDR.Schedule_ScheduleRuleCluster WHERE ScheduleRuleClusterID = src.ScheduleRuleClusterID)

GO