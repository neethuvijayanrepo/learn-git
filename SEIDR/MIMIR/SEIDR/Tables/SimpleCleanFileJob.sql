CREATE TABLE [SEIDR].[SimpleCleanFileJob]
(
	[JobProfile_JobID] INT NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),
	Extension varchar(30) not null default('CLN'),
	LineEnd_CR bit not null default(1),
	LineEnd_LF bit not null default(1),
	CHECK(LineEnd_CR = 1 or LineEnd_LF = 1),
	Line_MinLength int null, 
	Line_MaxLength int null, 
	CHECK(Line_MinLength is null or Line_MaxLength is null or Line_MinLength <= Line_MaxLength),
    [BlockSize] INT NULL CHECK(BlockSize > 5000 or BlockSize is null), 
    [CodePage] INT NULL, 
    [AddTrailer] BIT NOT NULL DEFAULT 0, 
    [KeepOriginal] BIT NOT NULL DEFAULT 1
)
