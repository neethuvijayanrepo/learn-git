CREATE TABLE [REFERENCE].[Contact]
(
	[ContactID] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	FirstName varchar(130) not null,
	LastName varchar(130),
	DisplayName as FirstName + COALESCE(' ' + LastName, ''),
	FullNameLastFirst as COALESCE(LastName + ', ', '') + FirstName,
	Email varchar(130),
	Phone varchar(15),
	DC smalldatetime not null default(GETDATE()),
	LU smalldatetime not null default(GETDATE()),
	Creator varchar(260) NOT NULL DEFAULT(SUSER_NAME()),
	Editor varchar(260) NOT NULL DEFAULT(SUSER_NAME()),
	DD smalldatetime null,
	Active as CONVERT(bit, IIF(DD is null, 1, 0)) PERSISTED NOT NULL	
)

GO

CREATE TRIGGER [REFERENCE].[trg_Contact_U]
    ON [REFERENCE].[Contact]
    FOR  UPDATE
    AS
    BEGIN
        SET NoCount ON
		INSERT INTO REFERENCE.Contact_History(ContactID, FirstName, LastName, Email, Phone, Editor, Active)
		SELECT d.ContactID, d.FirstName, d.LastName, d.Email, d.Phone, d.Editor, d.Active
		FROM deleted d		
    END