
 CREATE PROCEDURE [CONFIG].[usp_Schedule_u]
	@ScheduleID int, 
	@Description varchar(250) = null, 
	@ForSequenceControl bit = null
AS
BEGIN
	SET @Description = UTIL.ufn_CleanField(@Description)
	UPDATE SEIDR.Schedule
	SET [Description] = COALESCE(@Description, Description),
		ForSequenceControl = COALESCE(@ForSequenceControl, ForSequenceControl)
	WHERE ScheduleID = @ScheduleID
END