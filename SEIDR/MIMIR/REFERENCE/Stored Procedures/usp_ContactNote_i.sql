CREATE PROCEDURE [REFERENCE].[usp_ContactNote_i]
	@ContactID int,
	@NoteText varchar(2000),
	@Auto bit = 0,
	@UserName varchar(260) = null
AS
	SET @NoteText = UTIL.ufn_CleanField(@NoteText)
	IF @NoteText is null
	BEGIN
		RAISERROR('Must provide a non empty @NoteText', 16, 1)
		RETURN
	END
	
	IF @UserName is null
		SET @UserName = SUSER_NAME()
	ELSE IF SUSER_NAME() NOT LIKE @UserName
		SET @UserName += '(' + SUSER_NAME() + ')'

	INSERT INTO REFERENCE.ContactNote(ContactID, NoteText, Author, DC, [Auto])
	VALUES(@ContactID, @NoteText, @UserName, GETDATE(), @Auto)
RETURN 0
