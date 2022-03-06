CREATE FUNCTION [SECURITY].[ufn_GetTimeOffset_UserName](@UserName varchar(260))
RETURNS smallint
AS
BEGIN
	DECLARE @offset smallint = 0
	SELECT @offset = TimeZoneOffset
	FROM SECURITY.[User] 
	WHERE UserName = COALESCE(@UserName, SUSER_NAME())
	
	RETURN @offset
END