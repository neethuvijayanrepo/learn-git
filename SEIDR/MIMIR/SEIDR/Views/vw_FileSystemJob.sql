



CREATE VIEW [SEIDR].[vw_FileSystemJob] AS
	SELECT FileSystemJobID, jpj.JobProfile_JobID, 
	jpj.Description, 
	ComputedDescription = 
		CASE WHEN fs.Operation LIKE '%METRIX' then fs.Operation + ' -> LoadProfileID ' + CONVERT(varchar(30), ISNULL(fs.LoadProfileID, jp.LoadProfileID) )
			 else 'FileSystem: ' + fs.Operation
				+ CASE WHEN OutputPath is not null  then ' -> '  
					+ CASE WHEN OutputPath LIKE '%<%' then ISNULL('"' + NULLIF(REPLACE(UTIL.ufn_PathItem_GetName(OutputPath), '*', '') + '"		', ''), '')
							WHEN OutputPath LIKE '%*' then UTIL.ufn_PathItem_GetName(OutputPath)
							else REPLACE(UTIL.ufn_PathItem_GetName(OutputPath), '*', '') 
							end
					+ CASE WHEN OutputPath LIKE '%AndromedaFilesSandbox%' then ' [SANDBOX FOLDER]'
							WHEN OutputPath LIKE '%PREPROCESS%' then ' [PREPROCESS FOLDER]'
							WHEN OutputPath LIKE '%AndromedaFiles%' then ' [METRIX FOLDER]'
							WHEN OutputPath LIKE '%_SourceFiles%' then ' [SOURCE FOLDER]'
							WHEN OutputPath LIKE '%\FTP%' AND OutputPath NOT LIKE '%FTP' then ' [FTP]'
							WHEN Outputpath LIKE '%\Tabuler\Input\%' OR OutputPath LIKE '%VortexML%' or OutputPath LIKE '%PROCLAIM%' then ' [PROCLAIM FOLDER]'
							ELSE '' 
							end
					else '' 
					end
				+ CASE WHEN outputPath NOT LIKE '%\INPUT\<%'
						AND 
						(
							REPLACE(OutputPath, '_', '') LIKE '%\<%*%' 
							OR REPLACE(outputPath, '_', '') LIKE '%\<[YMD]%><[YMD]%><[YMD]%>' 
						)
						 then ' (Parent Folder: ' + UTIL.ufn_PathItem_GetName(REPLACE(OutputPath, '\' + UTIL.ufn_PathItem_GetName(OutputPath), '')) + ')'						
					else '' end
			end + CASE WHEN Operation IN ('CHECK', 'EXIST', 'CREATE_DUMMY') AND UpdateExecutionPath = 1 then '. Set as JobExecution.FilePath' else '' end,
	jpj.StepNumber,
	Operation [FileOperation], 
	Source, CONFIG.ufn_GetShortHandPath(Source, jp.OrganizationID, jp.ProjectID, UserKey1, COALESCE(fs.LoadProfileID, jp.LoadProfileID)) [ShortHandSource], 
	OutputPath, CONFIG.ufn_GetShortHandPath(OutputPath, jp.OrganizationID, jp.ProjectID, UserKey1, COALESCE(fs.LoadProfileID, jp.LoadProfileID)) ShortHandOutputPath,
	Filter, 	
	UpdateExecutionPath, Overwrite, fs.LoadProfileID, db.DatabaseLookupID,	db.Description [DatabaseLookup],
	jp.JobProfileID, jp.Description [JobProfile], jp.OrganizationID,  o.Description [Organization], 
	jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
	jp.UserKey1, jp.UserKey2,
	jpj.CanRetry, jpj.RetryDelay, jpj.RetryLimit, jpj.RetryCountBeforeFailureNotification,
	jpj.TriggerExecutionStatusCode [TriggerExecutionStatus], jpj.TriggerExecutionNameSpace, jpj.Branch, jpj.TriggerBranch,
	jpj.RequiredThreadID [ThreadID], jpj.FailureNotificationMail, s.Description [SequenceSchedule]
	FROM SEIDR.FileSystemJob fs
	JOIN SEIDR.JobProfile_Job jpj
		ON fs.JobPRofile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile jp 
		ON jpj.JobProfileID = jp.JobProfileID
	LEFT JOIN SEIDR.DatabaseLookup db
		ON fs.DatabaseLookupID = db.DatabaseLookupID
	LEFT JOIN SEIDR.Schedule s
		ON jpj.SequenceScheduleID = s.scheduleID 
		AND s.Active = 1
	LEFT JOIN REFERENCE.Organization o
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON jp.ProjectID = p.ProjectID
	WHERE fs.Active = 1
	AND jpj.Active = 1
	AND jp.Active = 1