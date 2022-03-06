CREATE PROCEDURE [UTIL].[usp_GetMissingFiles] @OrganizationID int, @ProcessingDate datetime
as
BEGIN
	SELECT * , UTIL.ufn_PathItem_GetName(FilePath), SEIDR.ufn_ApplyDateMask(FilePath, @ProcessingDate), 
	SEIDR.ufn_ApplyDateMask(UTIL.ufn_PathItem_GetName(FilePath), @ProcessingDate) [ExpectedFile]
	FROM[SEIDR].[vw_FilePath_ShortHand] f
	WHERE 1=1
	AND OrganizationID = @OrganizationID
	AND (
		Source = 'JobProfile (Registration)' AND Shorthand LIKE '#FTP%' 
		or StepNumber = 1 AND Source = 'FTP (RemotePath)'
		)
	AND NOT EXISTS(SELECT null 
					FROM SEIDR.JobExecution 
					WHERE JobProfileID = f.JobProfileID 
					AND ProcessingDate = @ProcessingDate)
END
GO