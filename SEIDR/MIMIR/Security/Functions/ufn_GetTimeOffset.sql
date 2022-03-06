CREATE FUNCTION [SECURITY].[ufn_GetTimeOffset](@UserID smallint)
RETURNS smallint
AS
BEGIN
	IF @UserID is null
		SELECT @UserID = UserID
		FROM SECURITY.[User]
		WHERE UserName = SUSER_NAME()

	DECLARE @offset smallint = 0
	SELECT @offset = TimeZoneOffset
	FROM SECURITY.[User] 
	WHERE UserID = @UserID
	
	RETURN @offset
END