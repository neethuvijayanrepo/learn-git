

CREATE VIEW [SEIDR].[vw_FilePath]
AS
	SELECT JobProfileID, Description [JobProfile], OrganizationID, ProjectID, UserKey1, UserKey2, null as JobProfile_JobID, null as StepNumber, null as StepDescription, 
	'JobProfile (Registration)' [Source], 
		FilePath = 
			CASE WHEN FileDateMask NOT LIKE REPLACE(FileFilter, '*', '%') 
			THEN CASE WHEN RegistrationFolder LIKE '%\' then RegistrationFolder + FileFilter else RegistrationFolder + '\' + FileFilter end		
			ELSE CASE WHEN RegistrationFolder LIKE '%\' then RegistrationFolder + FileDateMask else RegistrationFolder + '\' + FileDateMask end		
			END,
	SetExecutionFilePath = CONVERT(bit, 1),
	null as Branch
	FROM SEIDR.JobProfile
	WHERE Active = 1 AND RegistrationValid = 1
	
	UNION ALL	
	SELECT JobProfileID, Description [JobProfile], OrganizationID, ProjectID, UserKey1, UserKey2, 
							null as JobProfile_JobID, null as StepNumber, null as StepDescription, 
	'JobProfile (Registration Destination)' [Source], 
		FilePath = 
			CASE WHEN FileDateMask NOT LIKE REPLACE(FileFilter, '*', '%') 
			THEN CASE 
						WHEN RegistrationDestinationFolder is null AND RegistrationFolder LIKE '%\' then RegistrationFolder + '_Registered\' + FileFilter 
						WHEN RegistrationDestinationFolder is null then RegistrationFolder + '\_Registered\' + FileFilter
						WHEN RegistrationDestinationFolder LIKE '%\' then RegistrationDestinationFolder + FileFilter 
						else RegistrationDestinationFolder + '\' + FileFilter 
						end		
			ELSE CASE 
					WHEN RegistrationDestinationFolder is null AND RegistrationFolder LIKE '%\' then RegistrationFolder + '_Registered\' + FileDateMask 
					WHEN RegistrationDestinationFolder is null then RegistrationFolder + '\_Registered\' + FileDateMask
					WHEN RegistrationDestinationFolder LIKE '%\' then RegistrationDestinationFolder + FileDateMask 
					else RegistrationDestinationFolder + '\' + FileDateMask 
					end		
			END,
	SetExecutionFilePath = CONVERT(bit, 1),
	null as Branch 
	FROM SEIDR.JobProfile
	WHERE Active = 1 AND RegistrationValid = 1
	
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, fs.Description, 'FileSysytemJob (Source)', Source, CASE WHEN FileOperation IN ('CHECK', 'EXIST') then UpdateExecutionPath else CONVERT(bit, 0) end,
	fs.Branch
	FROM SEIDR.vw_FileSystemJob fs
	WHERE Source is not null
	
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FileSysytemJob (OutputPath)', OutputPath, UpdateExecutionPath,
	Branch
	FROM SEIDR.vw_FileSystemJob
	WHERE OutputPath is not null

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FTP (RemotePath)', RemotePath, CONVERT(bit, 0), Branch
	FROM SEIDR.vw_FTPJob
	WHERE RemotePath is not null
	
	UNION ALL 
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FTP (RemoteTargetPath)', RemoteTargetPath, CONVERT(bit, 0), Branch
	FROM SEIDR.vw_FTPJob
	WHERE RemoteTargetPath IS NOT NULL
	
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FTP (LocalPath)', LocalPath, CASE WHEN FTPOperation = 'RECEIVE' then CONVERT(bit, 1) else 0 end, Branch
	FROM SEIDR.vw_FTPJob
	WHERE LocalPath is not null	

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'LoaderJob (OutputFolder/OutputFileName)', 
		FilePath = CASE WHEN OutputFolder LIKE '%\' then OutputFolder + ISNULL(OutputFileName, '')
					ELSE OutputFolder + '\' + ISNULL(OutputFileName, '')
					end,
		1, Branch
	FROM SEIDR.vw_LoaderJob
	WHERE OutputFolder is not null 
	
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'SpawnJob (SourceFile)', 
		FilePath = SourceFile,
		0, Branch
	FROM SEIDR.vw_SpawnJob
	WHERE SourceFile is not null 

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID,
		StepNumber, Description, 'PGPJob (SourcePath)',
		FilePath = SourcePath,
		0, Branch
	FROM SEIDR.vw_PGPJob
	WHERE SourcePath is not null

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID,
		StepNumber, Description, 'PGPJob (OutputPath)',
		FilePath = OutputPath,
		0, Branch
	FROM SEIDR.vw_PGPJob
	WHERE OutputPath is not null

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FileConcatenation (SecondaryFile)', 
		FilePath = SecondaryFile,
		0, Branch
	FROM SEIDR.vw_FileConcatenationJob
	WHERE SecondaryFile is not null 
	
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FileConcatenation (OutputPath)', 
		FilePath = OutputPath,
		1, Branch
	FROM SEIDR.vw_FileConcatenationJob
	WHERE OutputPath is not null

	UNION ALL 
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, userKey1, Userkey2, JobProfile_JobID, StepNumber, Description, 'EDI Conversion (OutputFolder)',
		FilePath = OutputFolder,
		1, Branch
	FROM SEIDR.vw_EDIConversionJob
	WHERE OutputFolder is not null

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, JobName + ' (SettingsFilePath)',
		FilePath = SettingsFilePath,
		0, Branch
	FROM [SEIDR].[vw_SettingsFile]

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, JobName + ' (ExpectedOutputFile)',
		FilePath = ExpectedOutputFile,
		0, Branch
	FROM [SEIDR].[vw_FileAssertionTestJob]

	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, JobName + ' (OutputFolder)',
		FilePath = OutputFolder,
		0, Branch
	FROM SEIDR.vw_DemoMapJob
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FileMergeJob (MergeFile)',
		FilePath = MergeFile,
		0, Branch
	FROM SEIDR.vw_FileMergeJob
	UNION ALL
	SELECT JobProfileID, JobProfile, OrganizationID, ProjectID, UserKey1, UserKey2, JobProfile_JobID, StepNumber, Description, 'FileMergeJob (OutputFilePath)',
		FilePath = OutputFilePath,
		1, Branch
	FROM SEIDR.vw_FileMergeJob