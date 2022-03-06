CREATE VIEW [REFERENCE].[vw_JobProfile_Contacts]
	AS 
	SELECT JobProfileID, jp.Description [JobProfile], LoadProfileID, 
		[c].[UserKey], 
		[c].[UserKeyDescription], 
		[c].[Inbound], 
		[c].[Outbound], 
		[c].[VendorSpecific], 
		[c].[ContactFirstName], 
		[c].[ContactLastName], 
		[c].ContactDisplayName, 
		[c].[ContactEmail], 
		[c].[ContactPhone], 
		[c].[ContactFromDate], 
		[c].[ContactThroughDate], 
		ScheduleFromDate as JobProfileScheduleFromDate, 
		ScheduleThroughDate as JobProfileScheduleThroughDate,
		[c].[OrganizationID], 
		[c].[Organization], 
		[c].[ProjectID], 
		[c].[Project]
	FROM SEIDR.JobProfile jp
	JOIN REFERENCE.vw_Organization_Contact c 
		ON jp.OrganizationID = c.OrganizationID
		AND (jp.ProjectID is null or jp.ProjectID = c.ProjectID)
		AND c.UserKey IN (jp.UserKey1, jp.UserKey2)
	WHERE jp.Active = 1	
	AND c.Active = 1