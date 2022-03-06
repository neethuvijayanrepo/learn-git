CREATE VIEW [REFERENCE].[vw_ContactNote]
	AS 
	SELECT n.ContactNoteID,
		n.ContactID, 
		c.ContactDisplayName,
		c.ContactFirstName,
		n.NoteText,		
		n.[Auto],
		COALESCE(u.DisplayName, n.Author) [NoteAuthor],
		c.OrganizationID,
		c.Organization,
		c.ProjectID,
		c.Project,
		c.VendorSpecific,		
		DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), n.DC) [NoteCreationTime]				
	FROM REFERENCE.ContactNote n
	JOIN REFERENCE.vw_Organization_Contact c
		ON c.ContactID = n.ContactID
	LEFT JOIN SECURITY.[User] u
		ON n.Author = u.UserName
	WHERE n.Active = 1
	UNION ALL
		SELECT n.ContactNoteID,
		CASE WHEN c.Active = 1 then n.ContactID end, 
		CASE WHEN c.Active = 0 then '[DELETED CONTACT] ' else '' end + c.DisplayName,
		CASE WHEN c.Active = 0 then '[DELETED CONTACT] ' else '' end + c.FirstName,
		n.NoteText,		
		n.[Auto],
		COALESCE(u.DisplayName, n.Author) [NoteAuthor],
		null,
		null,
		null,
		null,
		null,		
		DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), n.DC) [NoteCreationTime]				
	FROM REFERENCE.ContactNote n
	JOIN REFERENCE.Contact c
		ON c.ContactID = n.ContactID
	LEFT JOIN SECURITY.[User] u
		ON n.Author = u.UserName
	WHERE n.Active = 1
	AND c.ContactID NOT IN (SELECT ContactID FROM REFERENCE.Organization_Contact WHERE Active = 1)