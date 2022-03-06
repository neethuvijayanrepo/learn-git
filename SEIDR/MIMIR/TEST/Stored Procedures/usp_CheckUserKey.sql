CREATE PROCEDURE [TEST].[usp_CheckUserKey]
	@UserKey varchar(50),
	@Description varchar(130) = null,
	@Priority varchar(10) = null
AS
	IF NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = @UserKey)
	BEGIN
		INSERT INTO REFERENCE.UserKey(UserKey, Description, [OverrideOrganizationDefaultPriorityCode])
		VALUES(@UserKey, COALESCE(@Description, @UserKey), @Priority)
	END
	ELSE
		UPDATE REFERENCE.UserKey
		SET Description = COALESCE(@Description, Description),
			OverrideOrganizationDefaultPriorityCode = COALESCE(@Priority,  OverrideOrganizationDefaultPriorityCode)
		WHERE UserKey = @UserKey
RETURN 0
