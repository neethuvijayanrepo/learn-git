CREATE VIEW SEIDR.vw_JobExecution_LoadProfileID
AS
	
	SELECT 'TO_METRIX' as [INFO_TYPE], JobExecutionID, ProcessingDate, jp.LoadProfileID, 
		jp.JobProfileID, JobProfile, 
		jp.UserKey1, jp.UserKey2, 
		jp.OrganizationID, jp.ProjectID, 
		jp.StepNumber, jp.Step, je.ExecutionStatus, Complete = je.Success, Error = CONVERT(bit, 1 - je.Success),
		CASE WHEN jp.Source is not null then SEIDR.ufn_ApplyDateMask(jp.Source, ProcessingDate) else je.FilePath end [FilePath],
		je.DC [LU] --DC Of history table is essentially LU of the Step logged
	FROM SEIDR.vw_LoadProfileID jp
	JOIN SEIDR.JobExecution_ExecutionStatus je
		ON jp.JobProfile_JobID = je.JobProfile_JobID
		AND je.isLatestForExecutionStep = 1
	WHERE INFO_TYPE = 'FILESYSTEM' 
	UNION ALL
	SELECT 'PROFILE', JobExecutionID, ProcessingDate, jp.LoadProfileID, 
		jp.JobProfileID, JobProfile, 
		jp.UserKey1, jp.UserKey2,
		je.OrganizationID, je.ProjectID, 
		null, null, je.ExecutionStatus, es.isComplete, es.IsError,
		je.FilePath,
		je.LU
	FROM SEIDR.JobExecution je
	JOIN SEIDR.ExecutionStatus es	
		ON je.ExecutionStatusCode = es.ExecutionStatusCode
		AND je.ExecutionStatusNameSpace = es.[NameSpace]
	JOIN SEIDR.vw_LoadProfileID jp
		ON je.JobProfileID =jp.JobProfileID
	WHERE INFO_TYPE = 'PROFILE'
	UNION ALL 
	SELECT 'SOURCE',  je.JobExecutionID, je.ProcessingDate, jp.LoadProfileID, 
		jp.JobProfileID, jp.JobProfile, 
		jp.UserKey1, jp.UserKey2, 
		jp.OrganizationID, jp.ProjectID, 
		ISNULL(jp.StepNumber, fp.StepNumber), ISNULL(jp.Step, fp.Source), je.ExecutionStatus, Complete = je.Success, 
		Error = CONVERT(bit, 1 - je.Success),
		CASE WHEN jp.Source is not null then SEIDR.ufn_ApplyDateMask(jp.Source, je.ProcessingDate) 
			WHEN fp.FilePath is not null then SEIDR.ufn_ApplyDateMask(fp.FilePath, je.ProcessingDate)	
				else je.FilePath end [FilePath], 
		--fp.JobProfile_JobID,
		je.DC [LU] --DC Of history table is essentially LU of the Step logged
	FROM SEIDR.vw_LoadProfileID jp
	JOIN SEIDR.JobExecution je0 
		ON jp.JobProfileID = je0.JobProfileID
	JOIN SEIDR.JobExecution_ExecutionStatus je
		ON je0.JobExecutionID = je.JobExecutionID
		AND je.isLatestForExecutionStep = 1
		AND je.StepNumber = 1
	LEFT JOIN SEIDR.vw_FilePath fp
		ON jp.JobProfileID = fp.JobProfileID
		AND fp.SetExecutionFilePath = 1		
	WHERE INFO_TYPE = 'PROFILE_ALL'	
	AND (Fp.JobProfile_JobID is null or fp.JobProfile_JobID = jp.JobProfile_JobID OR fp.StepNumber = 1 or fp.StepNumber is null)
	AND (jp.JobProfile_JobID is null or je.JobProfile_JobID = jp.JobProfile_JobID)