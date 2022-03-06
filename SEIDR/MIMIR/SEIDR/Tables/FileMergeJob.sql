CREATE TABLE [SEIDR].[FileMergeJob]
(
	[JobProfile_JobID] INT NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),
	MergeFile varchar(500) not null,
	OutputFilePath varchar(500) not null,
	Overwrite bit not null default(1),
	InnerJoin bit not null default(1),
	LeftJoin as CONVERT(bit, 1- InnerJoin),
	KeepDelimiter bit not null,
	HasTextQualifier bit not null,
	RemoveDuplicateColumns bit not null,
	RemoveExtraMergeColumns bit not null,
	CaseSensitive bit not null default(0),
	PreSorted bit not null default(0), --Indicates that both input files are already sorted.
	LeftInputHasHeader bit not null,
	RightInputHasHeader bit not null,
	IncludeHeader bit not null,
	LeftKey1 varchar(128) not null,
	RightKey1 varchar(128) null, --If null, assume column name shared.
	LeftKey2 varchar(128) null,
	RightKey2 varchar(128) null,
	LeftKey3 varchar(128) null,
	RightKey3 varchar(128) null
)
