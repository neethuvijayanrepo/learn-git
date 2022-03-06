CREATE TABLE [SEIDR].[JobProfileNote]
(
	[JobProfileNoteID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	JobProfileID int not null foreign key references SEIDR.JobProfile(JobProfileID),
	NoteText varchar(2000) not null,
	Author varchar(260) not null,
	[Auto] bit not null default(0),
	DC smalldatetime not null default(GETDATE()),
	DD smalldatetime,	
	Active as (CONVERT(bit, IIF(DD IS NULL, 1, 0))) PERSISTED NOT NULL
)
