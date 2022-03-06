CREATE TABLE [SEIDR].[BatchScriptJob]
(
	[BatchScriptJobID] INT identity(1,1) NOT NULL PRIMARY KEY,
	JobProfile_JobID int not null foreign key references SEIDR.JobProfile_Job(JobProfile_JobID),
	BatchScriptPath varchar(500) not null,
	[Parameter3] varchar(300) null, --Parameter 1 = FilePath of JobExecution, Parameter 2 = JobExecutionID
	[Parameter4] varchar(300) null,
	DD datetime null,
	[Active] AS CONVERT(bit, CASE WHEN DD IS NULL THEN 1 else 0 end) PERSISTED NOT NULL,
	Valid AS CONVERT(bit, CASE WHEN BatchScriptPath LIKE '%.BAT' then 1 else 0 end) PERSISTED NOT NULL
)
GO

CREATE UNIQUE INDEX uq_BatchScriptJob_JobProfile_Job ON SEIDR.BatchScriptJob(JobProfile_JobID)
WHERE DD IS NULL