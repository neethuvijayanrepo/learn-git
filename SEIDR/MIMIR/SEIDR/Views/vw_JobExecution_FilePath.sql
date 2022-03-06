
CREATE VIEW [SEIDR].[vw_JobExecution_FilePath]
AS
	SELECT je.JobExecutionID, je.OrganizationID, je.ProjectID, je.UserKey1, je.UserKey2, je.ProcessingDate,
	fv.Source as [ConfigurationSource], fv.JobProfile_JobID, fv.StepNumber, fv.StepDescription, 
	SEIDR.ufn_ApplyDateMask(fv.FilePath, je.ProcessingDate) [UnmaskedFilePath], SetExecutionFilePath, fv.JobProfileID, fv.JobProfile
	FROM SEIDR.JobExecution je
	JOIN SEIDR.vw_FilePath fv
		ON je.JobProfileID = fv.JobProfileID
	WHERE je.Active = 1