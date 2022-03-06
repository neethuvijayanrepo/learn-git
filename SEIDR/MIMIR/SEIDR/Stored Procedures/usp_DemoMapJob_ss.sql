CREATE PROCEDURE [SEIDR].[usp_DemoMapJob_ss]
	@JobProfile_JobID int
AS
Select
	DM.DemoMapID,
    DM.JobProfile_JobID,	 
	DM.SkipLines,
	DM.DoAPB,
	DM.OutputFolder, 
	DM.FilePageSize, 
	DM.FileMapID,
	DM.FileMapDatabaseID,
	DM.PayerLookupDatabaseID,
	DM.Enable_OOO,	
	CASE WHEN DM.Delimiter = 'TAB' then CHAR(9) ELSE CONVERT(char(1), DM.Delimiter) end [Delimiter],
	CASE WHEN DM.OutputDelimiter = 'TAB' THEN CHAR(9) ELSE CONVERT(char(1), DM.OutputDelimiter) end [OutputDelimiter],
	DM._InsuranceBalanceUnavailable,
	DM._InsuranceDetailUnavailable,
	DM._PartialDemographicLoad,
	DM._PatientBalanceUnavailable,
	DM.OOO_InsuranceBalanceValidation,
	DM.HasHeaderRow
	FROM SEIDR.DemoMapJob DM
	WHERE DM.JobProfile_JobID = @JobProfile_JobID
RETURN 0

