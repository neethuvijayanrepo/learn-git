CREATE PROCEDURE [REFERENCE].[usp_Organization_Contact_u]
	@Organization_ContactID int,
	@UserKey varchar(50) = null,
	@ContactID int = null,
	@ProjectID smallint = null,
	@RemoveProjectID bit = 0,
	@FromDate date = null,
	@ThroughDate date = null,
	@RemoveThroughDate bit = 0
	
AS
	UPDATE REFERENCE.Organization_Contact
	SET UserKey = COALESCE(@UserKey, UserKey),
		ContactID = COALESCE(@ContactID, ContactID),		
		ProjectID = CASE WHEN @RemoveProjectID = 0 then COALESCE(@ProjectID, ProjectID) end,
		FromDate = COALESCE(@FromDate, FromDate),
		ThroughDate = CASE WHEN @RemoveThroughDate = 0 then COALESCE(@ThroughDate, ThroughDate) end
	WHERE Organization_ContactID = @Organization_ContactID
RETURN 0
