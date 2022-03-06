CREATE TABLE [SEIDR].[FileSizeCheckJob]
(
	FileSizeCheckJobID int not null primary key identity(1,1),
	JobProfile_JobID int not null foreign key references SEIDR.JobProfile_Job(JobProfile_JobID) UNIQUE,	
	DaysBack tinyint not null default(30) CHECK(DaysBack >= 1), --Let the configuration procedure control max days back.
	EmptyFileSize int not null default(0) CHECK(EmptyFileSize >= 0),
	StandardDeviationMultiplier float not null default(1) CHECK (StandardDeviationMultiplier > 0 AND StandardDeviationMultiplier < 100),
	DoPercentCheck as CASE WHEN StandardDeviationMultiplier > 4 then 1 else 0 end,
	PercentThreshold as CASE WHEN StandardDeviationMultiplier > 4 then StandardDeviationMultiplier end,
	CheckLargeFiles bit not null default(0),
	IgnoreSunday bit not null default(0),
	IgnoreEmptyFileSunday bit not null default(0),
	IgnoreMonday bit not null default(0),
	IgnoreEmptyFileMonday bit not null default(0),
	IgnoreTuesday bit not null default(0),
	IgnoreEmptyFileTuesday bit not null default(0),
	IgnoreWednesday bit not null default(0),
	IgnoreEmptyFileWednesday bit not null default(0),
	IgnoreThursday bit not null default(0),
	IgnoreEmptyFileThursday bit not null default(0),
	IgnoreFriday bit not null default(0),
	IgnoreEmptyFileFriday bit not null default(0),
	IgnoreSaturday bit not null default(0),
	IgnoreEmptyFileSaturday bit not null default(0),
	DC datetime not null default(GETDATE()),
	LU datetime not null default(GETDATE()),
	Creator varchar(260) not null default SUSER_NAME(),
	Editor varchar(260) not null default SUSER_NAME()
)
