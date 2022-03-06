CREATE FUNCTION UTIL.ufn_CleanField(@FieldValue varchar(4000))
RETURNS varchar(4000)
AS
BEGIN
	WHILE @FieldValue LIKE '%  %'
	BEGIN
		SET @FieldValue = REPLACE(@FieldValue, '  ', ' ')
	END
	SET @FieldValue = NULLIF(LTRIM(RTRIM(@FieldValue)), '')	
	RETURN @FieldValue
END