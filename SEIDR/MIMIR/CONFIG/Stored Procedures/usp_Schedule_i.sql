CREATE PROCEDURE [CONFIG].[usp_Schedule_i]
	@Description varchar(200),
	@ScheduleRuleClusterList UTIL.udt_intID readonly,	
	@ForSequenceControl bit = 0,
	@SafetyMode bit = 1
AS
BEGIN
	DECLARE @HourOffset int = SECURITY.ufn_GetTimeOffset(null)
	DECLARE @ScheduleID int

	SET @Description = UTIL.ufn_CleanField(@Description)

	INSERT INTO SEIDR.Schedule(Description, ForSequenceControl)
	VALUES(@Description, @ForSequenceControl)
	
	SELECT @ScheduleID = SCOPE_IDENTITY()

	INSERT INTO SEIDR.Schedule_ScheduleRuleCluster(ScheduleID, ScheduleRuleClusterID)
	SELECT @ScheduleID, r.ID
	FROM @ScheduleRuleClusterList r

	SELECT *, SEIDR.ufn_Schedule_GetDescription(ScheduleID, @HourOffset) 
	FROM SEIDR.vw_Schedule 
	WHERE ScheduleID = @ScheduleID
END