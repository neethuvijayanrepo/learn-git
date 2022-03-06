CREATE PROCEDURE [REFERENCE].[usp_Organization_Contact_d]
	@Organization_ContactID int = null,
	@OrganizationID int = null,
	@ProjectID smallint = null,
	@ContactID int = null,
	@UserName varchar(260) = null
AS
	DECLARE @ThroughDate date
	DECLARE @DefaultThroughDate date = GETDATE()
	IF @Organization_ContactID is not null
	BEGIN
		SELECT @ThroughDate = ThroughDate, 
			@OrganizationID = OrganizationID, 
			@ProjectID = ProjectID, 
			@ContactID = ContactID
		FROM REFERENCE.Organization_Contact
		WHERE Organization_ContactID = @Organization_ContactID

		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('Could not find Organization_ContactID %d', 16, 1, @Organization_ContactID)
			RETURN
		END
		IF @ThroughDate is null OR @ThroughDate > @DefaultThroughDate
			SET @ThroughDate = @DefaultThroughDate

		UPDATE REFERENCE.Organization_Contact
		SET ThroughDate = @ThroughDate
		WHERE Organization_ContactID = @Organization_ContactID
	END
	ELSE IF @ContactID IS NOT NULL
	BEGIN
		IF @OrganizationID is null
		BEGIN
			RAISERROR('Must provide @organizationID', 16, 1)
			RETURN
		END

		SELECT @ThroughDate = MIN(ThroughDate)
		FROM REFERENCE.Organization_Contact
		WHERE OrganizationID = @OrganizationID
		AND (@ProjectID is null or ProjectID = @ProjectID)
		AND ContactID = @ContactID
		
		IF @ThroughDate is null OR @ThroughDate > @DefaultThroughDate
			SET @ThroughDate = @DefaultThroughDate

		UPDATE REFERENCE.Organization_Contact
		SET ThroughDate = @ThroughDate
		WHERE OrganizationID = @OrganizationID
		AND (@ProjectID is null or ProjectID = @ProjectID)
		AND ContactID = @ContactID

	END
	ELSE
	BEGIN
		RAISERROR('Must provide either @ContactID or @Organization_Contact to filter records to set through dates', 16, 1)
		RETURN
	END

	DECLARE @AutoMessage varchar(2000) 
	
	IF @ProjectID IS NOT NULL
	BEGIN
		SELECT @autoMessage = 'Set Contact ThroughDate for Organization "' + o.Organization + '"'
			+ ', Project "' + o.Project + '"'		
		FROM REFERENCE.vw_Project_Organization o
		WHERE OrganizationID = @OrganizationID
		AND ProjectID = @ProjectID
	END
	ELSE
		SELECT @autoMessage = 'Set Contact ThroughDate for Organization "' + o.Description + '"'
		FROM REFERENCE.Organization o
		WHERE OrganizationID = @OrganizationID
	
	exec REFERENCE.usp_ContactNote_i @ContactID, @AutoMessage, 1, @userName
RETURN 0
