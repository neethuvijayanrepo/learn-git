
CREATE FUNCTION UTIL.ufn_GetDatabaseProperty
(
	@PropertyName nvarchar(128)
)
RETURNS SQL_VARIANT
AS
BEGIN
	DECLARE @val sql_variant
	SELECT @val = value	
	FROM sys.extended_properties
	WHERE class = 0 and name = @PropertyName

	RETURN @val
END
-- ufn_GetForeignKey_JoinCondition