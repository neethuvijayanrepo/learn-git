CREATE PROCEDURE [SEIDR].[usp_EdiConversion_ss]
	@JobProfile_JobID int
AS
	SELECT [JobProfile_JobID], [CodePage], [OutputFolder], [KeepOriginal] 
	FROM SEIDR.EdiConversion
	WHERE JobProfile_JobID = @JobProfile_JobID
	
	RETURN 0
