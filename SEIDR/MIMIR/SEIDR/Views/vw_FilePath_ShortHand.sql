CREATE VIEW SEIDR.vw_FilePath_ShortHand
AS
SELECT fp.JobProfileID,
		fp.JobProfile,
		fp.OrganizationID, 
		f.Organization,
		fp.ProjectID, f.Project,
		fp.UserKey1, fp.UserKey2, fp.JobProfile_JobID, fp.StepNumber, fp.StepDescription, 
		fp.SetExecutionFilePath,
		fp.Source, fp.FilePath,
		[Shorthand] = CONFIG.ufn_GetShortHandPath(fp.FilePath, fp.OrganizationID, fp.ProjectID, fp.UserKey1, null) --Don't use the LoadProfileID
			/*REPLACE(REPLACE(REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(fp.FilePath, 
						#Source, '#SOURCE\'),
						#FTP, '#FTP\'),
						#PRODUCTION, '#PRODUCTION\'),
						#SANDBOX, '#SANDBOX\'),
						#EXPORT, '#EXPORT\'),
						#UAT, '#UAT\'),
						'DAILY_LOADS\Preprocessing\', '#PREPROCESS\'),
						'DAILY_LOADS\', '#DAILY\'),
						'MASTER_LOADS\', '#MASTER\'),
						'_' + UserKey1+ '\', '_#KEY\'),
						'\' + UserKey1+ '\', '\#KEY\'),
						'#PRODUCTION\#PREPROCESS', '#PREPROCESS'), '#PRODUCTION\#DAILY', '#DAILY'), '#PRODUCTION\#MASTER', '#MASTER')						
				*/		 
FROM SEIDR.vw_FilePath fp
JOIN REFERENCE.vw_Organization_Folder f
	ON fp.OrganizationID = f.OrganizationID
	AND (f.ProjectID is null and fp.ProjectID is null or f.ProjectID = fp.ProjectID)