CREATE PROCEDURE [REFERENCE].[usp_Organization_Contact_i]
	@OrganizationID int,
	@UserKey varchar(50),
	@ContactID int,
	@ProjectID smallint = null,
	@FromDate date = null,
	@ThroughDate date = null,
	@UserName varchar(260) = null
	
AS
	IF @fromDate is null
		SET @FromDate = CONVERT(date, GETDATE())

	INSERT INTO REFERENCE.Organization_Contact(OrganizationID, ProjectID, UserKey, 
			ContactID, DC, FromDate, ThroughDate)
	VALUES(@OrganizationID, @ProjectID, @UserKey, 
			@ContactID, GETDATE(), @FromDate, @ThroughDate)

	
	
	DECLARE @autoMessage varchar(2000)
	IF @ProjectID IS NOT NULL
	BEGIN
		SELECT @autoMessage = 'Added Contact to Organization "' + o.Organization + '"'
			+ CASE WHEN @ProjectID IS NOT NULL THEN ', Project "' + o.Project + '"' else '' end		
		FROM REFERENCE.vw_Project_Organization o
		WHERE OrganizationID = @OrganizationID
		AND ProjectID = @ProjectID
	END
	ELSE
		SELECT @autoMessage = 'Added Contact to Organization "' + o.Description + '"'
		FROM REFERENCE.Organization o
		WHERE OrganizationID = @OrganizationID



	SET @autoMessage += ', UserKey = "' + @UserKey + '"
FromDate: ' + CONVERT(varchar(20), @FromDate, 0)
	IF @ThroughDate is not null
		SET @autoMessage += '
ThroughDate: ' + CONVERT(varchar(20), @throughDate, 0)
	
	exec REFERENCE.usp_ContactNote_i @ContactID, 
		@AutoMessage, 
		@Auto = 1,
		@userName = @UserName

RETURN 0
