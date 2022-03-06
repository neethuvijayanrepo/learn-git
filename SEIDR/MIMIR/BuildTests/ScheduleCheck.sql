RAISERROR('Start ScheduleCheck', 0, 0)

DECLARE @ScheduleID int, @TestResult int
SELECT TOP 1 @ScheduleID = ScheduleID
FROM SEIDR.Schedule
WHERE Description = 'Daily @ 10:00AM'  --Comment out to see it fail. 
--Note that this is included in one of the publish scripts, so it should exist.

IF @ScheduleID IS NULL
BEGIN
	RAISERROR('Missing ScheduleID', 16, 1)
	RETURN
END


SELECT @TestResult = SEIDR.ufn_CheckSchedule(@ScheduleID, '2019-04-09 4:00', '2019-04-08')
IF @TestResult is not null
BEGIN
	RAISERROR('Unexpected schedule Match.', 16, 1)
	RETURN
END

SELECT @TestResult = SEIDR.ufn_CheckSchedule(@ScheduleID, '2019-04-09 10:00', '2019-04-08')
IF @TestResult is null
BEGIN
	RAISERROR('Unexpected schedule - no match', 16, 1)
	RETURN
END

SELECT @TestResult = SEIDR.ufn_CheckSchedule(@ScheduleID, '2019-04-09 10:00', '2019-04-07')
IF @TestResult IS NOT NULL
BEGIN
	RAISERROR('UnExpected Schedule Match - DateDiff > 1', 16, 1)
	RETURN
END
RAISERROR('ScheduleCheck - Pass', 0, 0)
