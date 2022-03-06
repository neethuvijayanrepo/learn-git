CREATE VIEW [REFERENCE].[vw_JobExecution_Contacts]
	AS 
	SELECT JobExecutionID, METRIX_LoadBatchID, METRIX_ExportBatchID,
		JobProfileID, 
		LoadProfileID, 
		je.ProcessingDate,
		UTIL.ufn_PathItem_GetName(je.FilePath) [FileName], 
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
		[c].[OrganizationID], 
		[c].[Organization], 
		[c].[ProjectID], 
		[c].[Project]
	FROM SEIDR.JobExecution je
	JOIN REFERENCE.vw_Organization_Contact c 
		ON je.OrganizationID = c.OrganizationID
		AND (je.ProjectID is null or je.ProjectID = c.ProjectID)
		AND c.UserKey IN (je.UserKey1, je.UserKey2)
	WHERE je.Active = 1	
	AND c.Active = 1