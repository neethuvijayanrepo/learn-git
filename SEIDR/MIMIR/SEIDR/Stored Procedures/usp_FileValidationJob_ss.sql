CREATE PROCEDURE [SEIDR].[usp_FileValidationJob_ss]
	@JobProfile_JobID int
AS
BEGIN
	--Allow a default configuration if no record exists.
	IF NOT EXISTS(SELECT null FROM SEIDR.FileValidationJob WHERE JobProfile_JobID = @JobProfile_JobID)
	BEGIN
		INSERT INTO SEIDR.FileValidationJob(JobProfile_JobID, Delimiter)
		VALUES(@JobProfile_JobID, '|') --Default output delimiter.
	END

	SELECT * -- Filling properties on the configuration object and we only have one table involved, so just take everything
	FROM SEIDR.FileValidationJob 
	WHERE JobProfile_JobID = @JobProfile_JobID

	
END