

CREATE FUNCTION [UTIL].[ufn_GetRange] 
( 
 -- Add the parameters for the function here
 @StartValue bigint,
 @EndValue bigint
)
RETURNS @ValueList TABLE (Number bigint primary key)
AS
BEGIN

	IF @EndValue is null
	BEGIN
		--RAISERROR('Invalid @EndValue: Null.', 16, 1)
		--RETURN
		RETURN
	END 
	SET @StartValue = COALESCE(@StartValue, 0)
	IF @StartValue > @EndValue
	BEGIN		
		RETURN
	END 
 --Get a date range by subtracting up to 999 days
 --SET @EndDate = COALESCE(@EndDate, GETDATE())
 INSERT INTO @ValueList   
 select a.Num
 from (
 select @EndValue -  (a.a + (10 * b.a) + (100 * c.a) + (1000 * d.a) + (10000 * e.a) /*+ (100000 * f.a)*/) as Num
 from (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as a
 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as b
 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as c
 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as d
 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as e
 --cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as f
 ) a
 WHERE a.Num >= @StartValue 

 WHILE @StartValue < @EndValue - 99999
 BEGIN
	SET @EndValue -= 100000
	 INSERT INTO @ValueList   
	 SELECT a.Num
	 FROM (
	 select @EndValue -  (a.a + (10 * b.a) + (100 * c.a) + (1000 * d.a) + (10000 * e.a) /* + (100000 * f.a)*/) as Num
	 from (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as a
	 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as b
	 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as c
	 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as d
	 cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as e
	 --cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as f
	 )a
	 WHERE a.Num >= @StartValue 
 END

 RETURN
END