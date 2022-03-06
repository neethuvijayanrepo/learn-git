CREATE FUNCTION [SEIDR].[ufn_GetUserTime]
(
	@UserName varchar(128) = NULL,
	@ChangeDate datetime
)
RETURNS DATETIME
AS
BEGIN
	IF @UserName is null
		SET @UserName = SUSER_NAME()	
	IF EXISTS(SELECT null FROM SEIDR.User_TimeOffset WITH (NOLOCK) WHERE UserName  = @UserName)
		SELECT @ChangeDate += CONVERT(datetime, TimeOffset)
		FROM SEIDR.User_TimeOffset WITH (NOLOCK)
		WHERE UserName = @UserName
	
	RETURN @ChangeDate
END

