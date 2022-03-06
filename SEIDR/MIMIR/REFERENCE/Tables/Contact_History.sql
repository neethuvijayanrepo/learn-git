CREATE TABLE [REFERENCE].[Contact_History]
(
	[Contact_HistoryID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	ContactID int not null,
	FirstName varchar(130),
	LastName varchar(130),
	Email varchar(130),
	Phone varchar(15),
	DC smalldatetime not null default(GETDATE()),
	Editor varchar(260) not null, 
    [Active] BIT NOT NULL
)
