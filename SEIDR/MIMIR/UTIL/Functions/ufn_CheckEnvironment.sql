CREATE FUNCTION UTIL.ufn_CheckEnvironment()
RETURNS VARCHAR(50)
AS
BEGIN
	DECLARE @Val varchar(50) = 'DEVELOPMENT'
	SELECT @Val = CONVERT(varchar(50), value)
	from sys.extended_Properties
	WHERE class = 0 and name = N'ENVIRONMENT_TYPE'
	RETURN @Val
END
-- ufn_GetDatabaseProperty