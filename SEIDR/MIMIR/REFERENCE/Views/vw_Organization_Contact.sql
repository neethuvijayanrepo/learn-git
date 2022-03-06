CREATE VIEW [REFERENCE].[vw_Organization_Contact]
AS     
	SELECT b.Organization_ContactID,
	po.OrganizationID, po.[Organization], 
	po.ProjectID, po.[Project], 
	uk.UserKey, uk.Description [UserKeyDescription],
	uk.Inbound, uk.Outbound, uk.VendorSpecific, 
	c.ContactID,
	c.FirstName as ContactFirstName, 
	c.DisplayName as ContactDisplayName, 
	c.LastName as ContactLastName,
	c.FullNameLastFirst as ContactFullNameLastFirst,
	c.Email as ContactEmail, 
	c.Phone as ContactPhone,
	b.[FromDate] as [ContactFromDate], --Date that the team can begin using this contact. Should NOT link to ProcessingDate.
	COALESCE(b.ThroughDate, po.ThroughDate) [ContactThroughDate],--Date that we should stop contact for org/user key/project combo
	[Active] = CONVERT(bit, CASE WHEN b.Active = 0 then 0 
					WHEN po.ThroughDate is null then 1 
					WHEN po.ThroughDate > GETDATE() then 1
					ELSE 0 
					END),
	b.DC 
	FROM REFERENCE.Organization_Contact b
	JOIN REFERENCE.Contact c
		ON b.ContactID = c.ContactID
	JOIN REFERENCE.vw_Project_Organization po
		ON b.OrganizationID = po.OrganizationID
		AND (b.ProjectID is null OR b.ProjectID = po.ProjectID)
	JOIN REFERENCE.UserKey uk
		ON b.UserKey = uk.UserKey
	WHERE c.Active = 1
	--AND (po.ThroughDate is null or po.ThroughDate > GETDATE())
	UNION ALL
	SELECT b.Organization_ContactID,
	o.OrganizationID, o.Description, 
	null, null,
	uk.UserKey, uk.Description [UserKeyDescription],
	uk.Inbound, uk.Outbound, uk.VendorSpecific, 
	c.ContactID,
	c.FirstName as ContactFirstName, 
	c.DisplayName as ContactDisplayName, 
	c.LastName as ContactLastName,
	c.FullNameLastFirst as ContactFullNameLastFirst,
	c.Email as ContactEmail, 
	c.Phone as ContactPhone,
	b.[FromDate], --Date that the team can begin using this contact. Should NOT link to ProcessingDate.
	COALESCE(b.ThroughDate, o.OrganizationThroughDate), --Date that we should stop contact for org/user key/project combo
	
	[Active] = CONVERT(bit, CASE WHEN b.Active = 0 then 0 
					WHEN o.OrganizationThroughDate is null then 1 
					WHEN o.OrganizationThroughDate > GETDATE() then 1
					ELSE 0 
					END),
	b.DC 
	FROM REFERENCE.Organization_Contact b
	JOIN REFERENCE.Contact c
		ON b.ContactID = c.ContactID
	JOIN REFERENCE.Organization o
		ON b.OrganizationID = o.OrganizationID		
	JOIN REFERENCE.UserKey uk
		ON b.UserKey = uk.UserKey
	WHERE c.Active = 1
	AND o.OrganizationID >= 0
	--AND (o.OrganizationThroughDate is null or o.OrganizationThroughDate > GETDATE())
	AND NOT EXISTS(SELECT null FROM REFERENCE.vw_Project_Organization WHERE OrganizationID = o.OrganizationID)
  --Could potentially do a catch-all with null user key in bridge table, but that 
  --might lead to some misinformation if someone just initially adds a default and not the exceptions
  /*  
    OR (b.UserKey is null --if no user key, then can apply to everything, except where a contact is specified directly
        AND NOT EXISTS(SELECT null 
                        FROM REFERENCE.PlaceHolderBridge b2
                        WHERE UserKey = uk.UserKey
                        AND b2.OrganizationID = po.OrganizationID
                        AND (b2.ProjectID is null OR b2.ProjectID = po.ProjectID)
                        )
        )
    */
    
    
  
  


