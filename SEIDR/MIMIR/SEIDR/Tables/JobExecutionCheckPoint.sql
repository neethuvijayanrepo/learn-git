CREATE TABLE [SEIDR].[JobExecutionCheckPoint]
(
	[CheckPointID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	JobExecutionID bigint not null foreign key references SEIDR.JobExecution(JobExecutionID) ON DELETE CASCADE,
	JobProfile_JobID int not null foreign key references SEIDR.JobProfile_Job(JobProfile_JobID) ON DELETE CASCADE,
	JobID int not null foreign key references SEIDR.Job(JobID) ON DELETE CASCADE,
	CheckPointNumber int not null,
	CheckPointKey varchar(10) null,
	CheckPointMessage varchar(300) not null,
	ThreadID int,
	CheckPointTime int not null,
	DC smalldatetime not null default(GETDATE())
)
