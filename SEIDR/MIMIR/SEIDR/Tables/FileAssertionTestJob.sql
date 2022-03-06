CREATE TABLE [SEIDR].[FileAssertionTestJob]
(
	[AssertionTestJobID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	JobProfile_JobID int not null foreign key REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID) UNIQUE,
	ExpectedOutputFile varchar(500) not null,
	CheckColumnNameMatch bit not null default(0),
	CheckColumnOrderMatch bit not null default(0) CHECK (CheckColumnOrderMatch = 0 OR CheckColumnNameMatch = 1), 
    [SkipColumns] VARCHAR(4000) NULL
)
