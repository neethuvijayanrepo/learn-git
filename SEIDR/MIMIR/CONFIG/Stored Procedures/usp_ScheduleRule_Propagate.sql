CREATE PROCEDURE [CONFIG].[usp_ScheduleRule_Propagate]
	@ScheduleRuleID int,
	@HourOffset int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF @HourOffset is null
		SELECT @HourOffset = [SECURITY].ufn_GetTimeOffset_UserName(SUSER_NAME())

	INSERT INTO SEIDR.ScheduleRuleCluster(Description)
	SELECT Description FROM SEIDR.ScheduleRule r
	WHERE r.Active = 1 
	AND ScheduleRuleID = @ScheduleRuleID
	AND NOT EXISTS(SELECT null FROM SEIDR.ScheduleRuleCluster_ScheduleRule WHERE ScheduleRuleID = r.ScheduleRuleID)
		
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('ScheduleRule is already used in ScheduleRuleCluster. Do Nothing.', 0, 0) WITH NOWAIT		
		RETURN
	END
	DECLARE @SRC int = SCOPE_IDENTITY()


	INSERT INTO SEIDR.ScheduleRuleCluster_ScheduleRule(ScheduleRuleID, ScheduleRuleClusterID)
	SELECT r.ScheduleRuleID, src.ScheduleRuleClusterID
	FROM SEIDR.ScheduleRuleCluster src
	JOIN SEIDR.ScheduleRule r
		ON src.Description = r.Description
	WHERE r.Active = 1
	AND r.ScheduleRuleID = @ScheduleRuleID
	AND NOT EXISTS(SELECT null FROM SEIDR.ScheduleRuleCluster_ScheduleRule WHERE ScheduleRuleID = r.ScheduleRuleID)
	

	INSERT INTO SEIDR.Schedule(Description)
	SELECT Description
	FROM SEIDR.ScheduleRuleCluster src
	WHERE src.Active = 1
	AND src.ScheduleRuleClusterID = @SRC
	AND NOT EXISTS(SELECT null FROM SEIDR.Schedule_SCheduleRuleCluster WHERE ScheduleRuleClusterID = src.ScheduleRuleClusterID)

	INSERT INTO SEIDR.Schedule_ScheduleRuleCluster(ScheduleID, ScheduleRuleClusterID)
	SELECT s.ScheduleID, src.ScheduleRuleClusterID
	FROM SEIDR.Schedule s
	JOIN SEIDR.ScheduleRuleCluster src
		ON s.Description = src.Description
	WHERE src.Active = 1
	AND src.SCheduleRuleClusterID = @SRC
	AND NOT EXISTS(SELECT null FROM SEIDR.Schedule_ScheduleRuleCluster WHERE ScheduleRuleClusterID = src.ScheduleRuleClusterID)

	SELECT *, SEIDR.ufn_Schedule_GetDescription(ScheduleID, @HourOffset)
	FROM SEIDR.vw_Schedule
	WHERE ScheduleRuleID = @ScheduleRuleID
END