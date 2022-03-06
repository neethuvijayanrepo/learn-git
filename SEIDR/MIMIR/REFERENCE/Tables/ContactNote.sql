CREATE TABLE [REFERENCE].[ContactNote]
(
	[ContactNoteID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	ContactID int not null foreign key REFERENCES REFERENCE.Contact(ContactID),
	NoteText varchar(2000) not null,
	DC smalldatetime not null default(GETDATE()),
	Author varchar(260) not null,
	[Auto] bit not null,
	DD smalldatetime,
	Active as (CONVERT(bit, IIF(DD IS NULL, 1, 0))) PERSISTED NOT NULL
)
