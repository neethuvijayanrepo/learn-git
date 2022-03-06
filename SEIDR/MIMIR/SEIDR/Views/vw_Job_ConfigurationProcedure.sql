


CREATE VIEW [SEIDR].[vw_Job_ConfigurationProcedure]
AS
SELECT j.JobID, j.JobName, j.Description, 
j.ConfigurationTable, REPLACE(j.ConfigurationTable, 'SEIDR.', 'SEIDR.vw_') [ConfigurationView], 
j.NeedsFilePath, 
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = REPLACE(j.ConfigurationTable, 'SEIDR.', 'SEIDR.usp_JobProfile_') + '_iu'
WHERE ConfigurationTable LIKE '%Job'
UNION ALL
SELECT j.JobID, j.JobName, j.Description, 
j.ConfigurationTable, REPLACE(j.ConfigurationTable, 'SEIDR.', 'SEIDR.vw_') + 'Job' [ConfigurationView], 
j.NeedsFilePath, 
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = REPLACE(j.ConfigurationTable, 'SEIDR.', 'SEIDR.usp_JobProfile_') + 'Job_iu'
WHERE ConfigurationTable NOT LIKE '%Job'
UNION ALL
SELECT j.JobID, j.JobName, j.Description, 
j.ConfigurationTable, 'SEIDR.vw_JobProfile_Job' [ConfigurationView], 
j.NeedsFilePath, 
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = 'SEIDR.usp_JobProfile_' + JobName + '_iu'
WHERE ConfigurationTable IS NULL
UNION ALL
SELECT j.JobID, j.JobName, j.Description, 
j.ConfigurationTable, '[SEIDR].[vw_SettingsFile]' [ConfigurationView], 
j.NeedsFilePath, 
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = 'SEIDR.usp_JobProfile_' + JobName + '_iu'
WHERE ConfigurationTable = 'SEIDR.JobProfile_Job_SettingsFile'--Generic
UNION ALL
SELECT j.JobID, j.JobName, j.Description, 
j.ConfigurationTable, REPLACE(j.ConfigurationTable, 'SEIDR.', 'SEIDR.vw_') [ConfigurationView], 
j.NeedsFilePath, 
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = 'SEIDR.usp_JobProfile_' + JobName + '_iu'
WHERE ConfigurationTable = 'SEIDR.FileAssertionTestJob' --Semi generic - shared.
UNION ALL
SELECT j.JobID, j.JobName, j.Description,
j.ConfigurationTable, 'METRIX.vw_ExportSettings', 
j.NeedsFilePath,
p.OBJECT_ID, p.QuotedProcedureName,
p.ProcedureName, p.Name [ParameterName], p.Parameter_ID, p.ParameterType, p.Max_Length, p.Precision, p.Scale, p.Is_Output, p.Is_ReadOnly
FROM SEIDR.Job j
JOIN UTIL.vw_ProcedureInfo p
	ON p.ProcedureName = 'SEIDR.usp_JobProfile_ExportSettings_iu'
WHERE JobNameSpace = 'METRIX_EXPORT' OR j.ConfigurationTable = 'METRIX.ExportSettings'