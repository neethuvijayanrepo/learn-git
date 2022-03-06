CREATE PROCEDURE [SEIDR].[usp_JobProfile_SetSchedule]
	@JobProfileID int,
	@ScheduleID int = null,  --If need to turn off, better to set from date to null (or future) or through date in the past.
	@RemoveScheduleID bit = 0,
	@Schedule varchar(250) = null,
	@ScheduleFromDate date = null,
	@RemoveScheduleFromDate bit = 0,
	@ScheduleThroughDate date =null,
	@RemoveScheduleThroughDate bit = 0,
	@CreateHistoricalExecutions bit = null
AS
BEGIN
	IF @ScheduleID IS NULL AND @Schedule IS NOT NULL
	BEGIN
		SELECT @ScheduleID = ScheduleID
		FROM SEIDR.Schedule
		WHERE Description = @Schedule
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Unable to find schedule with description "%s"', 16, 1, @Schedule)
			RETURN
		END
	END
	UPDATE SEIDR.JobProfile
	SET ScheduleID = CASE WHEN @RemoveScheduleID = 0 then COALESCE(@ScheduleID, ScheduleID) end, 
		ScheduleFromDate = CASE WHEN @RemoveScheduleFromDate = 0 then COALESCE(@ScheduleFromDate, ScheduleFromDate) end,
		ScheduleThroughDate = CASE WHEN @RemoveScheduleThroughDate = 0 then COALESCE( @ScheduleThroughDate, ScheduleThroughDate) end,
		ScheduleNoHistory = COALESCE(1- @CreateHistoricalExecutions, ScheduleNoHistory, 0)
	--OUTPUT INSERTED.*
	WHERE JobProfileID = @JobProfileID
	
	SELECT * 
	FROM SEIDR.vw_JobProfile
	WHERE JobProfileID = @JObProfileID
END