
CREATE FUNCTION [SEIDR].[ufn_CheckScheduleRule](@ScheduleRuleID int, @DateToCheck datetime, @FromDate datetime)
RETURNS BIT
AS
BEGIN
	DECLARE @RET bit = 0
	IF @DateToCheck > @FromDate 
	AND EXISTS(SELECT null
				FROM SEIDR.ScheduleRule sr  WITH (READPAST)
				WHERE ScheduleRuleID = @ScheduleRuleID
				AND 
				(
					PartOfDateType is null 
					OR PartOfDateType LIKE 'EOM%'
						AND CONVERT(date, @DateToCheck) = 
						CASE PartOfDateType
							WHEN 'eom-d' then DATEADD(day, -PartOfDate, EOMONTH(@DateToCheck)) --E.g., 5 days before month end, 0 days before month end, etc.
							WHEN 'eom+d' then DATEADD(day, PartOfDate , EOMONTH(@DateToCheck))
							--WHEN 'eom+m' then EOMONTH(@DateToCheck, PartOfDate) --Doesn't make sense to check
							--WHEN 'eom-m' then EOMONTH(@DateToCheck, -PartOfDate)
							WHEN 'EOM' then EOMONTH(@DateToCheck)							
							end
					OR PartOfDateType NOT LIKE 'EOM%'
						AND PartOfDate = 
						CASE PartOfDateType --ToDo: Foreign key to enforce type...
							WHEN 'yy'   then DATEPART(yy, @DateToCheck)
							WHEN 'yyyy' then DATEPART(yy, @DateToCheck)
							WHEN 'year' then DATEPART(yy, @DateToCheck)
							WHEN 'wk' then  DATEPART(wk, @DateToCheck)
							--WHEN 'ww' then  DATEPART(wk, @DateToCheck)
							WHEN 'week' then  DATEPART(wk, @DateToCheck)
							WHEN 'dy' then  DATEPART(dy, @DateToCheck)
							--WHEN 'y' then  DATEPART(dy, @DateToCheck)
							WHEN 'day' then  DATEPART(dd, @DateToCheck)
							WHEN 'dd' then  DATEPART(dd, @DateToCheck)
							WHEN 'dm' then DATEPART(dd, @DateToCheck)
							--WHEN 'd' then  DATEPART(dd, @DateToCheck)
							--WHEN 'm' then DATEPART(mm, @DateToCheck)
							WHEN 'mm' then DATEPART(mm, @DateToCheck)
							--WHEN 'q' then DATEPART(qq, @DateToCheck)
							WHEN 'qq' then  DATEPART(qq, @DateToCheck)
							WHEN 'dw' then DATEPART(dw, @DateToCheck)
						END					
				)
				AND  
				(
					IntervalType is null -- Use case is for Sequencing, will not be as useful for profile schedules
					OR IntervalValue =
						CASE IntervalType --todo: Foreign key to enforce valid types
							WHEN 'year' then DATEDIFF(year, @FromDate, @DateToCheck)
							WHEN 'yyyy' then DATEDIFF(year, @FromDate, @DateToCheck)
							WHEN 'yy' then DATEDIFF(year, @FromDate, @DateToCheck)
							WHEN 'qq' then DATEDIFF(quarter, @FromDate, @DateToCheck)
							WHEN 'q' then DATEDIFF(quarter, @FromDate, @DateToCheck)
							WHEN 'mm' then DATEDIFF(month, @FromDate, @DateToCheck)
							WHEN 'm' then DATEDIFF(month, @FromDate, @DateTOCheck)
							WHEN 'wk' then DATEDIFF(week, @FromDate, @DateToCheck)
							WHEN 'dd' then DATEDIFF(day, @FromDate, @DateToCheck)
							WHEN 'd' then DATEDIFF(day, @FromDate, @DateToCheck) 
							--If job should poll multiple times per day, should have the job ReQueue instead of completing. 
						end 
				)
				AND ([Hour] IS NULL OR DATEPART(hh, @DateToCheck) >= [Hour])
				AND (
						[Minute] is null 
						OR DATEPART(mi, @DateToCheck) >= [Minute] 
						OR DATEPART(hh, @DateToCheck) > ISNULL([Hour], 0) -- Only need minute check when hour matches.
					)
			)
	BEGIN
		SET @Ret = 1
	END
	ELSE IF @DateToCheck = @FromDate 	
	AND EXISTS(SELECT null
				FROM SEIDR.ScheduleRule sr  WITH (READPAST)
				WHERE ScheduleRuleID = @ScheduleRuleID
				AND ISNULL(IntervalValue, 0) = 0 
				/*
				Note: Regardless of interval type, if IntervalValue is 0 
				and the Dates are equal, then the interval section would match.
				*/
				AND (PartOfDateType is null 				
					OR PartOfDateType LIKE 'EOM%'
						AND CONVERT(date, @DateToCheck) = 
						CASE PartOfDateType
							WHEN 'eom-d' then DATEADD(day, -PartOfDate, EOMONTH(@DateToCheck)) --E.g., 5 days before month end, 0 days before month end, etc.
							WHEN 'eom+d' then DATEADD(day,  PartOfDate, EOMONTH(@DateToCheck))
							WHEN 'eom+m' then EOMONTH(@DateToCheck, PartOfDate)
							WHEN 'eom-m' then EOMONTH(@DateToCheck, -PartOfDate)
							WHEN 'EOM' then EOMONTH(@DateToCheck)							
							end
					OR PartOfDateType NOT LIKE 'EOM%'
						AND PartOfDate = 
						CASE PartOfDateType --ToDo: Foreign key...
							WHEN 'yy'   then DATEPART(yy, @DateToCheck)
							WHEN 'yyyy' then DATEPART(yy, @DateToCheck)
							WHEN 'year' then DATEPART(yy, @DateToCheck)
							WHEN 'wk' then  DATEPART(wk, @DateToCheck)
							WHEN 'ww' then  DATEPART(wk, @DateToCheck)
							WHEN 'week' then  DATEPART(wk, @DateToCheck)
							WHEN 'dy' then  DATEPART(dy, @DateToCheck)
							--WHEN 'y' then  DATEPART(dy, @DateToCheck)
							WHEN 'day' then  DATEPART(dd, @DateToCheck)
							WHEN 'dd' then  DATEPART(dd, @DateToCheck)
							--WHEN 'd' then  DATEPART(dd, @DateToCheck)
							--WHEN 'm' then DATEPART(mm, @DateToCheck)
							WHEN 'mm' then DATEPART(mm, @DateToCheck)
							--WHEN 'q' then DATEPART(qq, @DateToCheck)
							WHEN 'qq' then  DATEPART(qq, @DateToCheck)
							WHEN 'dw' then DATEPART(dw, @DateToCheck)
						END					
					)
				AND ([Hour] IS NULL OR DATEPART(hh, @DateToCheck) >= [Hour])
				AND (
						[Minute] is null 
						OR DATEPART(mi, @DateToCheck) >= [Minute] 
						OR DATEPART(hh, @DateToCheck) > ISNULL([Hour], 0) -- Only need minute check when hour matches.
					)
			)
	BEGIN
		SET @Ret = 1
	END
	RETURN @RET
END
