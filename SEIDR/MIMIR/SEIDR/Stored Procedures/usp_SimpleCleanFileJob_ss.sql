CREATE PROCEDURE [SEIDR].[usp_SimpleCleanFileJob_ss]
	@JobProfile_JobID int
AS
BEGIN
	SELECT [Extension], [LineEnd_CR], [LineEnd_LF], [Line_MinLength], [Line_MaxLength], [BlockSize], [CodePage], [AddTrailer], [KeepOriginal]
	FROM SEIDR.SimpleCleanFileJob 
	WHERE JobProfile_JobID = @JobProfile_JobID
	
	IF @@ROWCOUNT = 0
		RETURN -1

	RETURN 0
END