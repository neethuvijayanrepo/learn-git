
CREATE PROCEDURE [CONFIG].[usp_ScheduleRule_i]
	@Description varchar(250) = null, 
	@PartOfDateType varchar(5) = null,
	@PartOfDate int = null,
	@IntervalType varchar(4) = null,
	@IntervalValue int = null,
	@Hour tinyint = null,
	@Minute tinyint = null,
	@HourOffset int = 0
AS
BEGIN		
	IF @Hour > 24
		SET @hour = @Hour % 24
	IF ABS(@HourOffset) > 24
	BEGIN
		IF @HourOffset < 0
		BEGIN
			SET @HourOffset = - (ABS(@HourOffset)% 24)
		END
		ELSE
			SET @HourOffset = @HourOffset % 24
	END

	SET @Hour -= @HourOffset
	IF @Hour < 0
	BEGIN
		SET @Hour = 24 + @HourOffset
	END
	DECLARE @ScheduleRuleID int
	IF @Hour is null
		SET @Hour = 0
	IF @Minute is null
		SET @Minute = 0

	SET @Description = UTIL.ufn_CleanField(@Description)

	IF @PartOfDatetype is not null and not exists(SELECT Null FROM SEIDR.ScheduleDatePart WHERE PartOfDateType = @PartOfDateType)
	BEGIN
		SELECT * FROM SEIDR.ScheduleDatePart WITH (NOLOCK)
		RAISERROR('Invalid Date Part Type: %s', 16, 1, @PartOfDateType)
		RETURN
	END
	
	IF EXISTS(SELECT null
				FROM SEIDR.ScheduleRule 
				WHERE ISNULL(PartOfDateType, 'NULL') = ISNULL(@PartOfDateType, 'NULL')
				AND ISNULL(PartOfDate, 0) = ISNULL(@PartOfDate, 0)
				AND ISNULL(IntervalType, 'NULL') = ISNULL(@IntervalType, 'NULL')
				AND ISNULL(IntervalValue, 0) = ISNULL(@IntervalValue, 0)
				AND ISNULL(Hour, 0) = @Hour
				AND ISNULL(Minute, 0) = @Minute
				AND Active = 1)
	BEGIN
			SELECT * 
			FROM SEIDR.ScheduleRule 
			WHERE ISNULL(PartOfDateType, 'NULL') = ISNULL(@PartOfDateType, 'NULL')
			AND ISNULL(PartOfDate, 0) = ISNULL(@PartOfDate, 0)
			AND ISNULL(IntervalType, 'NULL') = ISNULL(@IntervalType, 'NULL')
			AND ISNULL(IntervalValue, 0) = ISNULL(@IntervalValue, 0)
			AND ISNULL(Hour, 0) = @Hour
			AND ISNULL(Minute, 0) = @Minute
			AND Active = 1

			RAISERROR('ScheduleRule overlaps with existing rule.', 16, 1)
			RETURN
	END

	INSERT INTO SEIDR.ScheduleRule(
		Description, 
		PartOfDateType, PartOfDate, IntervalType, IntervalValue, [Hour], [Minute])
	VALUES(
		ISNULL(@Description, 
				CONVERT(varchar(30), GETDATE()) --If description is null, just use date as a placeholder. Get Computed description as default afterward.
				), 
		@PartOfDateType, @PartOfDate, @IntervalType, @IntervalValue, @Hour, @Minute)
	
	SELECT @ScheduleRuleID = SCOPE_IDENTITY()
	IF @Description is null
	BEGIN
		UPDATE sr
		SET Description = SEIDR.ufn_ScheduleRule_GetDescription(ScheduleRuleID, 0)
		FROM SEIDR.ScheduleRule sr
		WHERE ScheduleRuleID = @ScheduleRuleID
	END

	SELECT * FROM SEIDR.ScheduleRule WHERE ScheduleRuleID = @ScheduleRuleID
	execute [CONFIG].[usp_ScheduleRule_Propagate] @ScheduleRuleID = @ScheduleRuleID, @HourOffset = @HourOffset	
END