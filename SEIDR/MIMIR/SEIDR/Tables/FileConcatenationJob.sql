CREATE TABLE [SEIDR].[FileConcatenationJob]
(
	JobProfile_JobID INT NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),
	HasHeader bit not null,
	SecondaryFile varchar(500) not null,
	SecondaryFileHasHeader bit not null,
	OutputPath varchar(500) not null
)
