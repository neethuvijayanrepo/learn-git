CREATE  PROCEDURE [Security].[usp_Security_Login]
	@UserName varchar(50) ,
	@UserPassword varchar(50) ,
	@IPAddress char(16) 
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	DECLARE @XACT_SAVE_POINT char(11)
	DECLARE @UserID int
	DECLARE @SG UNIQUEIDENTIFIER
	DECLARE @LoginStatus INT = 0 ---LOGIN FAILED
	DECLARE @IsLockedOut INT 
	SET @UserName = RTRIM(LTRIM(@UserName))
	DECLARE @LastLogin DATETIME2

	BEGIN TRANSACTION

	BEGIN TRY
		--Setting the value of UserID based on the given username and password
		SELECT @IsLockedOut=u.AccountLockedOut, @UserID=u.UserID	FROM [Security].[User] u	WHERE u.UserName = @UserName
		AND pwdcompare(CAST(@UserPassword AS varbinary(max)), u.UserPassword) = 1 

		IF @UserID IS NULL
		BEGIN
			SET @UserID = 0;
		END;

		IF @UserID > 0
		BEGIN
			UPDATE [Security].[UserAccountSession] SET 
				LogoutDateTime = GETDATE()
			WHERE 
					UserID = @UserID
				AND LogoutDateTime IS NULL;

			IF @IsLockedOut = 0
			BEGIN
				SET @SG = NEWID();

				INSERT INTO [Security].[UserAccountSession]
				(
					SG,
					UserID,
					LoginDateTime,
					IPAddress,
					LastActionDateTime,
					RV,
					LU,
					DC
				)
				SELECT @SG,
					@UserID,
					GETDATE(),
					@IPAddress,
					GETDATE(),
					0,
					GETDATE(),
					GETDATE()
	     		FROM [Security].[User]
				WHERE UserID=@UserID

				SET @LoginStatus = 1;---VALID LOGIN DETAILS
			END
			ELSE
			BEGIN
				SET @LoginStatus = 2 ---ACCOUNT LOCKEDOUT
			END;
		END
		ELSE
		BEGIN
			INSERT INTO [Security].[LoginAttemptFailure]
				(UserName,
				UserPassword,
				LoginAttemptDateTime,
				IPAddress,
				DC)
			VALUES (
				@UserName,
				@UserPassword,
				GETDATE(),
				@IPAddress,
				GETDATE()
				)

			IF EXISTS(SELECT * FROM [Security].[User]	WHERE UserName = @UserName AND AccountLockedOut = 0)
			BEGIN
				--Setting the Last LoginDateTime from UserAccountSession based on the Username
			 SET @LastLogin= (SELECT MAX(LoginDateTime) FROM [Security].[UserAccountSession] us
							  JOIN [Security].[User] u ON us.UserID = u.UserID WHERE u.UserName=@UserName)

				IF (SELECT COUNT(1) FROM [Security].[LoginAttemptFailure] WHERE UserName = @UserName AND
				     LoginAttemptDateTime > @LastLogin AND LoginAttemptDateTime > DATEADD(minute, -30, GETDATE())) >= 5
				BEGIN
					UPDATE [Security].[User]
					SET AccountLockedOut = 1
					WHERE UserName = @UserName;

					SET @LoginStatus = 2 ---ACCOUNT LOCKEDOUT
				END
			END
		END


		IF @LoginStatus = 1
		BEGIN
			SELECT
				@LoginStatus AS LoginStatus,
				U.[UserID]  AS [UserId],
				@SG AS SG,
				U.[UserName],
				U.[DisplayName],
				U.[Email],
				UR.[RoleID],
				R.RoleName
			FROM
				[Security].[User] U
				LEFT JOIN [Security].[UserRole] UR ON UR.[UserID] = U.[UserID]
				LEFT JOIN [Security].[Role] R ON R.[RoleID] = UR.[RoleID]
			WHERE
				U.[UserID] = @UserID;
		END
		ELSE
		BEGIN
			SELECT
				@LoginStatus AS LoginStatus,
				CAST(0 as smallint) AS [UserId],
				@SG AS SG,
				'' AS [UserName],
				'' AS [DisplayName],
				'' AS [Email],
				CAST(0 as smallint) AS [RoleID],
				'' AS RoleName
		END;

		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION
	END CATCH
END;
GO


