CREATE TABLE [SEIDR].[FileSizeHistory]
(
	[FileSizeHistoryID] INT NOT NULL PRIMARY KEY identity(1,1),
	FileSizeCheckJobID int not null foreign key REFERENCES SEIDR.FileSizeCheckJob(FileSizeCheckJobID),
	FilePath varchar(260) not null,
	FileSize BIGINT not null,
	DayOfWeek tinyint not null check(DayOfWeek BETWEEN 1 AND 7),
	FileDate date not null,
	PassValidation AS CONVERT(bit, CASE WHEN DeviationAtEvaluation is null then 1 
										WHEN DeviationAtEvaluation < ABS(FileSize - AverageAtEvaluation) THEN 0
										ELSE 1
										END) PERSISTED NOT NULL,
	JobExecutionID bigint not null foreign key REFERENCES SEIDR.JobExecution(JobExecutionID),
	AllowContinue bit not null default(0),
	DD smalldatetime null,
	Active as CONVERT(bit, IIF(DD IS NULL, 1, 0)) PERSISTED NOT NULL,
	DeviationAtEvaluation float,
	AverageAtEvaluation bigint,
	DC datetime not null default(GETDATE()),
	UNIQUE(JobExecutionID, DD)
)
GO

CREATE INDEX fidx_FileSizeHistory_FileSizeCheckJobID_AllowContinue_FileDate ON SEIDR.FileSizeHistory(FileSizeCheckJobID, AllowContinue, FileDate, [DayOfWeek])
INCLUDE(FileSize, Active)
WHERE (DD IS NULL)