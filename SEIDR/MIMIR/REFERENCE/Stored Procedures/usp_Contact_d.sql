CREATE PROCEDURE [REFERENCE].[usp_Contact_d]
	@ContactID int,
	@Delete bit = 1,
	@UserName varchar(260) = null
AS
	UPDATE REFERENCE.Contact
	SET DD = CASE WHEN @Delete = 1 then GETDATE() END
	WHERE ContactID = @ContactID
	AND Active = @Delete

	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @AutoNote varchar(2000) = 'Deleted Contact'
		IF @Delete = 0
			SET @autoNote = 'Re-Activate Contact'
		EXEC REFERENCE.usp_ContactNote_i @ContactID, @AutoNote, 1, @UserName
	END
	
RETURN 0
