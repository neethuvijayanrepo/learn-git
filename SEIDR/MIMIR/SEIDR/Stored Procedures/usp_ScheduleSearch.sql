
	CREATE PROCEDURE [SEIDR].[usp_ScheduleSearch]
		@Description varchar(200),
		@Hour int = null,
		@Minute tinyint = null,
		@ScheduleID int  =null out,
		@UserID smallint = null --Default user for SUSER_NAME() if null
	AS
	BEGIN
		DECLARE @TimeOffset int = SECURITY.ufn_GetTimeOffset(@UserID)		
		IF @Hour is not null		
		BEGIN
			SET @Hour -= @TimeOffset					
			IF @Hour < 0
				SET @hour = 24 + @Hour
			--SET @TimeOffset = 0
		END
		
		SELECT ScheduleID, Description [Schedule], 
			SEIDR.ufn_Schedule_GetDescription(ScheduleID, @TimeOffset) [ComputedDefinition], 
			ForSequenceControl, Creator
		INTO #SchedResults
		FROM SEIDR.Schedule s WITH (NOLOCK)
		WHERE Description LIKE '%' + @Description + '%'
		AND Active = 1
		AND (@Hour is null
			OR EXISTS(SELECT null FROM SEIDR.vw_Schedule WITH (NOLOCK) WHERE ScheduleID = s.ScheduleID AND [Hour] = @Hour AND [Minute] = ISNULL(@Minute, [Minute]))
			)
		IF @@ROWCOUNT > 0
		BEGIN
			SELECT TOP 1 @ScheduleID = ScheduleID
			FROM #SchedResults
			ORDER BY [Schedule]

			SELECT  
				CASE WHEN ScheduleID = @ScheduleID then CAST(1 as bit) else CAST(0 as bit) end [@ScheduleID_OUT],
				*
			FROM #SchedResults
		END
		ELSE
		BEGIN
			RAISERROR('NO MATCH', 16, 1)
		END
		DROP TABLE #SchedResults
	END
	IF @ScheduleID IS NOT NULL AND @TimeOffset <> 0
		SELECT * 
		FROM REFERENCE.vw_Schedule
		WHERE ScheduleID = @ScheduleID