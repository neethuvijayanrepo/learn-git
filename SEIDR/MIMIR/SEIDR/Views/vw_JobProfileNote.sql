CREATE VIEW [SEIDR].[vw_JobProfileNote]
AS 
	SELECT n.JobProfileNoteID,
		n.JobProfileID, 
		jp.UserKey1 [UserKey],
		jp.Description [JobProfile],
		n.NoteText,	
		n.[Auto],	
		DATEADD(hour, SECURITY.ufn_GetTimeOffset_UserName(null), n.DC) [NoteCreationTime],
		COALESCE(u.DisplayName, n.Author) [NoteAuthor],
		u.EmailAddress
	FROM SEIDR.JobProfileNote n
	JOIN SEIDR.JobProfile jp
		ON n.JobProfileID = jp.JobProfileID
	LEFT JOIN SECURITY.[User] u
		ON n.Author = u.UserName
	WHERE n.Active = 1
		

