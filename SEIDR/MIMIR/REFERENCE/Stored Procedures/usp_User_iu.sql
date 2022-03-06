

CREATE PROCEDURE REFERENCE.usp_User_iu
	@UserID smallint out,
	@Nickname varchar(130),
	@UserName varchar(260) = null,
	@FirstName varchar(130) =null,	
	@LastName varchar(130) = null,
	@EmailAddress varchar(260) = null,
	@TimeZoneOffset smallint = 0
AS
BEGIN
	IF @UserID is null AND @UserName is not null
		SELECT @UserID = UserID
		FROM SECURITY.[user] 
		WHERE UserName = @UserName

	DECLARE @CB_UserID smallint
	SELECT @CB_USERID = UserID
	FROM SECURITY.[User]
	WHERE userName = SUSER_NAME()
	
	IF @UserID is null
	BEGIN
		IF @UserName is null
		BEGIN
			RAISERROR('@UserName cannot be null', 16, 1)
			RETURN
		END
		IF @FirstName is null
		BEGIN
			RAISERROR('@FirstName cannot be null', 16, 1)
			RETURN
		END
		IF @LastName is null
		BEGIN
			RAISERROR('@LastName cannot be null', 16, 1)
			RETURN
		END
		
		INSERT INTO SECURITY.[User](UserName, LastName, FirstName, Nickname, EmailAddress, TimeZoneOffset,
			CB, CB_UserId, UID)
		VALUES(@UserName, @LastName, @FirstName, @NickName, @EmailAddress, @TimeZoneOffset, 
			SUSER_NAME(), @CB_UserID, @CB_UserID)
		SELECT @UserID = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		UPDATE SECURITY.[User]
		SET UserName = COALESCE(@UserName, UserName),
		LastName = COALESCE(@LastName, LastName),
		FirstName = COALESCE(@FirstName, FirstName),
		EmailAddress = COALESCE(@EmailAddress, EmailAddress),
		NickName = @NickName,
		TimeZoneOffset = @TimeZoneOffset,
		LU = GETDATE(),
		[UID] = @CB_UserID
		WHERE UserID = @UserID
	END

	SELECT * FROM SECURITY.[User] WHERE UserID = @UserID
END