CREATE PROCEDURE [CONFIG].usp_Schedule_ScheduleRuleCluster_i
	@ScheduleID int,
	@ScheduleRuleClusterID int,
	@HourOffset int,
	@SafetyMode bit = 1
AS
BEGIN
	IF @SafetyMode = 1
	BEGIN
		IF EXISTS(SELECT null FROM [SEIDR].[JobProfile] WITH (NOLOCK) WHERE [ScheduleID] = @ScheduleID AND Active = 1)
		BEGIN
			SELECT * FROM [SEIDR].[vw_JobProfile] WITH (NOLOCK) WHERE [ScheduleID] = @ScheduleID
			RAISERROR('Schedule is in use.', 16, 1)
			RETURN
		END
		IF EXISTS(SELECT null FROM [SEIDR].[vw_JobProfile_Job] WITH (NOLOCK) WHERE [SequenceScheduleID] = @ScheduleID)
		BEGIN
			SELECT * 
			FROM SEIDR.vw_JobProfile_Job
			WHERE SequenceScheduleID = @ScheduleID
			RAISERROR('Schedule is in use.' , 16, 1)
			RETURN
		END
	END

	INSERT INTO SEIDR.Schedule_ScheduleRuleCluster(ScheduleID, ScheduleRuleClusterID)
	VALUES(@ScheduleID, @ScheduleRuleClusterID)

	SELECT *, SEIDR.ufn_Schedule_GetDescription(ScheduleID, @HourOffset) 
	FROM SEIDR.vw_Schedule 
	WHERE ScheduleID = @ScheduleID
END