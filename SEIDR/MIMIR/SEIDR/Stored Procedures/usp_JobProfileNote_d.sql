CREATE PROCEDURE [SEIDR].[usp_JobProfileNote_d]
	@JobProfileNoteID int,
	@Delete bit = 1
AS
	UPDATE SEIDR.JobProfileNote
	SET DD = CASE WHEN @Delete = 1 THEN COALESCE(DD, GETDATE()) END
	WHERE JobProfileNoteID = @JobProfileNoteID
RETURN 0
