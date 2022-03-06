CREATE PROCEDURE [REFERENCE].[usp_ContactNote_d]
	@ContactNoteID int,
	@Delete bit = 1
AS
	UPDATE REFERENCE.ContactNote
	SET DD = CASE WHEN @Delete = 1 then COALESCE(DD, GETDATE()) END
	WHERE ContactNoteID = @ContactNoteID
RETURN 0
