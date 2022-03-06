CREATE FUNCTION [UTIL].[ufn_PathItem_GetRootName](@Path varchar(500))
RETURNS varchar(500) 
AS
BEGIN		
	IF @Path is null OR LEN(@Path) < 2
		RETURN NULL
	SET @Path = REPLACE(@Path, '/', '\')
	DECLARE @end varchar(500)
	IF LEN(@Path) > 3 AND LEFT(@Path, 2) = '\\' 
	BEGIN 
		IF CHARINDEX('\', @Path, 3) > 0
			SET @End =SUBSTRING(@Path, 3, CHARINDEX('\', @Path, 3) - 3)
		ELSE
			SET @end = SUBSTRING(@Path, 3, LEN(@Path))
	END
	ELSE IF LEN(@Path) > 4 AND LEFT(@Path, 3) LIKE '_:\'
	BEGIN 
		IF CHARINDEX('\', @Path, 4) > 0
			SET @End = SUBSTRING(@Path, 4, CHARINDEX('\', @Path, 4) - 4)
		ELSE
			SET @End = SUBSTRING(@Path, 4, LEN(@Path))
	END
	ELSE IF CHARINDEX('\', @Path, 1) > 0
		SET @End = SUBSTRING(@Path, 1, CHARINDEX('\', @Path, 1) - 1)
	ELSE
		SET @End = @Path

	IF RIGHT(@end, 1) = '\'
		SELECT @end = LEFT(@End, LEN(@End) - 1)
	RETURN NULLIF(LTRIM(RTRIM(@End)), '')
END