CREATE FUNCTION [SEIDR].[ufn_ScheduleRule_GetDescription](@ScheduleRuleID int, @TimeOffset int)
RETURNS varchar(300)
AS
BEGIN
	DECLARE @desc varchar(300) = 'Check that the '

	DECLARE @PartOfDateType varchar(5), @IntervalType varchar(4)
	DECLARE @PartOfDate int, @IntervalValue int, @Hour smallint, @Minute tinyint

	SELECT @PartOfDateType = PartOfDateType, @IntervalType = IntervalType,
			@PartOfDate = PartOfDate, @IntervalValue = IntervalValue,
			@Hour = ISNULL([hour],0) + @TimeOffset, @Minute = ISNULL([Minute], 0)
	FROM SEIDR.ScheduleRule
	WHERE ScheduleRuleID = @ScheduleRuleID

	IF @Hour NOT BETWEEN 0 AND 24 AND @PartOfDateType = 'EOM'
		SET @hour = 'eom-d'
	IF @PartOfDateType IN ('dw', 'dy', 'dd', 'dm', 'day', 'EOM-d')
	BEGIN
		IF @Hour < 0
		BEGIN
			IF @PartOfDateType = 'EOM-d'
				SET @PartOfDate += 1
			ELSE
				SET @PartOfDate -= 1
			SET @Hour = 24 + @Hour --  -2 => 24 + (-2) => 22 -> 10 PM
		END
		ELSE IF @Hour >= 24
		BEGIN	
			IF @PartOfDateType = 'EOM-d'
			BEGIN
				SET @PartOfDate -= 1
				IF @PartOfDate = 0
					SET @PartOfDate = 'EOM'
				ELSE IF @PartOfDate < 0
				BEGIN
					SET @PartOfDateType = 'dm'					
					SET @PartOfDate = ABS(@PartOfDate)
				END
			END
			SET @PartOfDate += 1
			SET @Hour = @Hour % 24 -- 26 -> 2 AM
		END
	END
	IF @PartOfDateType is not null
	BEGIN
		SELECT @Desc = @Desc + CASE @PartOfDateType
							WHEN 'eom-d' then 'Date is ' + CONVERT(varchar(5), @PartOfDate) + ' days before End Of Month'
							WHEN 'EOM' then 'Date is End of Month'
							WHEN 'yyyy' then 'year of the Date is ' + CONVERT(varchar(4), @PartOFDate)
							WHEN 'yy' then 'year of the Date is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'year' then 'year of the Date is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'wk' then 'week # of the year of the Date is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'week' then 'week # of the year of the Date is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'dy' then 'Date''s day of year is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'day' then 'Date''s day of Month is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'dd' then 'Date''s day of Month is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'dd'then 'Date''s day of Month is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'dm'then 'Date''s day of Month is ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'dw' then 'Date is on a ' + DATENAME(dw, @PartOfDate - 2)
							WHEN 'qq' then 'Date is in Querter # ' + CONVERT(varchar(4), @PartOfDate)
							WHEN 'mm' then 'Date is in ' + CASE @PartOfDate	WHEN 1 then 'January' WHEN 2 then 'February' WHEN 3 then 'March'
																			WHEN 4 then 'April' when 4 then 'May' WHEN 6 then 'June'
																			WHEN 7 then 'July' WHEN 8 then 'August' WHEN 9 then 'September'
																			WHEN 10 then 'October' WHEN 11 then 'November' WHEN 12 then 'December'
																			ELSE '??' End
							END
		IF @IntervalType is not null
			SET @Desc = @Desc + ' AND '
	END
	IF @IntervalType is not null
	BEGIN
		SELECT @Desc += 'Date matches once every ' + CONVERT(varchar(5), @IntervalValue) 
		IF @IntervalType IN ('year', 'yyyy', 'yy')
			SET @Desc += ' years'
		ELSE IF @IntervalType IN ('qq', 'q')
			SELECT @Desc += ' quarters'
		ELSE IF @IntervalType IN ('mm', 'm')
			SELECT @Desc +=  ' months'
		ELSE IF @IntervalType = 'wk'
			SELECT @Desc += ' weeks'
		ELSE
			SELECT @Desc += ' days'
	END	
	SELECT @Desc += ' @ ' + CONVERT(varchar, CASE WHEN @Hour > 12 then @Hour - 12 
													WHEN @Hour = 0 then 12 
													else @Hour end)
					+ ':' + RIGHT('00' + CONVERT(varchar, @Minute), 2)
					+ CASE WHEN @Hour > 12 then ' PM' else ' AM' end
	RETURN @Desc
END