CREATE PROCEDURE [REFERENCE].[usp_Contact_iu]
	@ContactID int = null output,
	@FirstName varchar(130),
	@LastName varchar(130) = null,
	@RemoveLastName bit = 0,
	@Email varchar(130) = null,
	@RemoveEmail bit =0 ,
	@Phone varchar(15) = null,
	@RemovePhone bit = 0,
	@UserName varchar(260) = null
AS
	IF @UserName is null
		SET @UserName = SUSER_NAME()
	ELSE IF SUSER_NAME() NOT LIKE @UserName
		SET @UserName += '(' + SUSER_NAME() + ')'


	IF @ContactID is not null
	BEGIN
		RAISERROR('Editing existing Contact ID %d', 0, 0, @ContactID)

		UPDATE REFERENCE.Contact
		SET FirstName = COALESCE(@FirstName, FirstName),
			LastName = CASE WHEN @RemoveLastName = 0 then COALESCE(@LastName, LastName) END,
			Email = CASE WHEN @RemoveEmail = 0 then COALESCE(@Email, email) end,
			Phone = CASE WHEN @RemovePhone = 0 then COALESCE(@Phone, Phone) end,
			Editor = @UserName,
			LU = GETDATE()
		WHERE ContactID = @ContactID		
		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Contact not found!', 16, 1)
			RETURN
		END
	END
	ELSE
	BEGIN
		INSERT INTO REFERENCE.Contact(FirstName, LastName, Phone, Email, Creator, Editor)
		VALUES(@Firstname, @LastName, @Phone, @EMail, @UserName, @UserName)
		SELECT @ContactID = SCOPE_IDENTITY()
	END
RETURN 0
