CREATE VIEW [REFERENCE].[vw_Organization_NewContact_Helper]
AS 
	SELECT OrganizationID, Organization, ProjectID, Project, po.ThroughDate,
		uk.UserKey, uk.Description [UserKeyDescription], uk.Inbound, uk.Outbound, uk.VendorSpecific,
		c.DisplayName, c.Email, c.Phone,
		[InsertScript] = 'exec REFERENCE.usp_Organization_Contact_i @OrganizationID = '  + CONVERT(varchar(20), OrganizationID) 
			+ ', @ProjectID = ' + CONVERT(varchar(20), ProjectID)
			+ ', @UserKey = ''' + uk.UserKey + ''', @ContactID = ' + CONVERT(varchar(20), c.ContactID) 
			+  ', @FromDate = null, @ThroughDate = null' --Note: these are the defaults anyway, but ease of allowing modification by including them in the prepopulated parameter list.
	FROM REFERENCE.vw_Project_Organization po WITH (NOLOCK)
	CROSS JOIN REFERENCE.UserKey uk WITH (NOLOCK)
	JOIN REFERENCE.Contact c WITH (NOLOCK)
		ON c.Active = 1
	WHERE NOT EXISTS(SELECT null 
						FROM REFERENCE.Organization_Contact WITH (NOLOCK)
						WHERE ContactID = c.ContactID
						AND OrganizationID = po.OrganizationID
						AND ProjectID = po.ProjectID)
	AND (po.ThroughDate is null or po.ThroughDate > GETDATE())
	UNION ALL
	SELECT OrganizationID, o.Description, null, null, o.OrganizationThroughDate,
		uk.UserKey, uk.Description, uk.Inbound, uk.Outbound, uk.VendorSpecific,
		c.DisplayName, c.Email, c.Phone,
		'exec REFERENCE.usp_Organization_Contact_i @OrganizationID = '  + CONVERT(varchar(20), OrganizationID) 
			+ ', @ProjectID = null'
			+ ', @UserKey = ''' + uk.UserKey + ''', @ContactID = ' + CONVERT(varchar(20), c.ContactID)
			+  ', @FromDate = null, @ThroughDate = null'
	FROM REFERENCE.Organization o WITH (NOLOCK)
	CROSS JOIN REFERENCE.UserKey uk WITH (NOLOCK)
	JOIN REFERENCE.Contact c WITH (NOLOCK)
		ON c.Active = 1
	WHERE NOT EXISTS(SELECT null 
						FROM REFERENCE.Organization_Contact WITH (NOLOCK)
						WHERE ContactID = c.ContactID
						AND OrganizationID = o.OrganizationID
						AND ProjectID IS NULL)
	AND (o.OrganizationThroughDate is null or o.OrganizationThroughDate > GETDATE())
	AND o.OrganizationID >= 0	

	