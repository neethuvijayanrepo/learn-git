CREATE PROCEDURE [CONFIG].usp_Schedule_d
	@ScheduleID int,
	@Delete bit = 1
AS
BEGIN
	IF @Delete = 1
	BEGIN
		UPDATE SEIDR.Schedule
		SET DD = COALESCE(DD, GETDATE())
		WHERE ScheduleID = @ScheduleID
	END
	ELSE
	BEGIN
		UPDATE SEIDR.Schedule
		SET DD = null
		WHERE ScheduleID = @ScheduleID 
	END
END