CREATE TABLE [SEIDR].[JobProfile_Job_SettingsFile]
(
	[JobProfile_JobID] INT NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID), 
    [SettingsFilePath] VARCHAR(360) NOT NULL
)
