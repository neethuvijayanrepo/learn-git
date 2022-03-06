

CREATE VIEW [REFERENCE].[vw_Project_Organization]
AS
	SELECT ProjectID, p.Description [Project], o.[OrganizationID], o.Description [Organization], CRCM, Modular, 
		FromDate, 
		COALESCE(ThroughDate, OrganizationThroughDate) ThroughDate, 
		p.Active,
		COALESCE(p.FTP_RootFolderOverride, o.FTP_RootFolder) [FTP_RootFolder], 
		COALESCE(p.Source_RootFolderOverride, o.Source_RootFolder) [Source_RootFolder],
		COALESCE(p.Metrix_RootFolderName_Override, o.Metrix_RootFolderName) [Metrix_RootFolderName]
	FROM REFERENCE.Project p
	JOIN REFERENCE.Organization o
		ON p.OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)