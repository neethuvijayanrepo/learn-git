CREATE VIEW SEIDR.vw_LoadProfileID
AS
	SELECT 'FILESYSTEM' [INFO_TYPE], fs.LoadProfileID, jpj.JobProfileID, jp.Description [JobProfile], 
		jp.UserKey1, jp.UserKey2,
		jp.OrganizationID, jp.ProjectID, jpj.StepNumber, 
		jpj.Description [Step], jpj.JobProfile_JobID,
		fs.Source
	FROM SEIDR.JobProfile jp
	JOIN SEIDR.JobProfile_Job jpj
		ON jp.JobProfileID = jpj.JobProfileID
	JOIN SEIDR.FileSystemJob fs
		ON jpj.JobProfile_JobID = fs.JobProfile_JobID
	WHERE fs.LoadProfileID is not null AND Jpj.Active = 1
	AND jp.Active = 1
	UNION ALL
	SELECT  'PROFILE', LoadProfileID, jp.JobProfileID, jp.Description,  
		jp.UserKey1, jp.UserKey2,
		OrganizationID, ProjectID, 
		null, null, null, null
	FROM SEIDR.JobProfile jp
	WHERE NULLIF(LoadProfileID, 0) IS NOT NULL AND Active = 1
	AND NOT EXISTS( SELECT null 
					FROM SEIDR.JobProfile_job jpj
					JOIN SEIDR.FileSystemJob fs
						ON jpj.JobProfile_JobID = fs.JobProfile_JobID
					WHERE jpj.Active = 1 
					AND jp.JobProfileID = jp.JobProfileID
					AND fs.LoadProfileID = jp.LoadProfileID --Shows in previous union in this case
					AND fs.Operation LIKE '%METRIX')
	UNION ALL
	SELECT  'PROFILE_ALL', ISNULL(fs.LoadProfileID, jp.LoadProfileID), jp.JobProfileID, jp.Description,  
		jp.UserKey1, jp.UserKey2,
		jp.OrganizationID, jp.ProjectID, 
		fs.StepNumber, fs.Description, fs.JobProfile_JobID, fs.Source
	FROM SEIDR.JobProfile jp
	LEFT JOIN SEIDR.vw_FileSystemJob fs
		ON jp.JobProfileiD = fs.JobProfileID 
		AND fs.StepNumber = 1
	WHERE (NULLIF(jp.LoadProfileID, 0) IS NOT NULL or fs.LoadProfileID IS NOT NULL) AND jp.Active = 1