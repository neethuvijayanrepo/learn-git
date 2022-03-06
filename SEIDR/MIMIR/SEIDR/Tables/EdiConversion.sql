CREATE TABLE [SEIDR].[EdiConversion]
(
	[JobProfile_JobID] INT NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),
	[CodePage] int null,
	OutputFolder varchar(500) null, 
    [KeepOriginal] BIT NOT NULL DEFAULT 1
)
