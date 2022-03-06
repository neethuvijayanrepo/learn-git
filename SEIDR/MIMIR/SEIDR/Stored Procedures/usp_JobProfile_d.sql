CREATE PROCEDURE SEIDR.usp_JobProfile_d
	@JobProfileID int,
	@Delete bit = 1
AS
BEGIN
	IF @Delete = 1
	BEGIN
		UPDATE SEIDR.JobProfile
		SET DD = GETDATE()
		WHERE JobProfileID = @JobProfileID 
		AND Active = 1
	END
	ELSE
	BEGIN
		UPDATE SEIDR.JobProfile
		SET DD = NULL
		WHERE JobProfileID = @JobProfileID
	END
END