CREATE FUNCTION UTIL.ufn_PathItem_GetName(@Path varchar(500))
RETURNS varchar(500) 
AS
BEGIN		
	IF @Path is null
		RETURN NULL
	DECLARE @end varchar(500)
--SELECT
	SELECT @End = 
			right(@Path,case when charindex('\',reverse(@Path), 1) > 1 then charindex('\',reverse(@Path), 1)- 1 
									WHEN charindex('\',reverse(@Path), 1) =  1 then charINDEX('\', REVERSE(@Path), 2) - 1
									else len(@Path) end)
	IF RIGHT(@end, 1) = '\'
	SELECT @end = LEFT(@End, LEN(@End) - 1)
	RETURN NULLIF(LTRIM(RTRIM(@End)), '')
END