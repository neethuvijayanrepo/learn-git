CREATE PROCEDURE [CONFIG].[usp_ScheduleRuleCluster_i]
	@Description varchar(250),
	@RuleList [UTIL].[udt_IntID] readonly
AS
BEGIN
	DECLARE @ScheduleRuleClusterID int
	SET @Description = UTIL.ufn_CleanField(@Description)
	IF @Description is null
	BEGIN
		RAISERROR('Must provide @Description', 16, 1)
		RETURN
	END

	INSERT INTO SEIDR.ScheduleRuleCluster(Description)
	VALUES(@Description)
	SELECT @ScheduleRuleClusterID = SCOPE_IDENTITY()

	INSERT INTO SEIDR.ScheduleRuleCluster_ScheduleRule(ScheduleRuleClusterID, ScheduleRuleID)
	SELECT @ScheduleRuleClusterID, ID
	FROM @RuleList

	DECLARE @ScheduleID int
	INSERT INTO SEIDR.Schedule(Description)
	VALUES(@Description)
	SELECT @ScheduleID = SCOPE_IDENTITY()

	INSERT INTO SEIDR.Schedule_ScheduleRuleCluster(ScheduleID, ScheduleRuleClusterID)
	VALUES(@ScheduleID, @ScheduleRuleClusterID)

	SELECT * FROM SEIDR.vw_Schedule
	WHERE ScheduleID = @ScheduleID
END