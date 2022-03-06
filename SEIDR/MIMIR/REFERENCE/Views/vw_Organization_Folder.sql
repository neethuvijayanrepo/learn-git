
CREATE VIEW [REFERENCE].[vw_Organization_Folder]
AS
	SELECT  o.[OrganizationID], o.Description [Organization], ProjectID, p.Description [Project], CRCM, Modular, FromDate, ThroughDate, p.Active,
		CASE WHEN p.FTP_RootFolderOverride LIKE '%\' then p.FTP_RootFolderOverride
			WHEN p.FTP_RootFolderOverride IS NOT NULL then p.FTP_RootFolderOverride + '\'
			WHEN o.FTP_RootFolder LIKE '%\' then o.FTP_RootFolder
			ELSE COALESCE(o.FTP_RootFolder + '\', '')
			end [#FTP], 
		CASE WHEN p.Source_RootFolderOverride LIKE '%\' then p.Source_RootFolderOverride
			WHEN p.Source_RootFolderOverride IS NOT NULL THEN p.Source_RootFolderOverride + '\'
			WHEN o.Source_RootFolder LIKE '%\' then o.Source_RootFolder
			ELSE COALESCE(o.Source_RootFolder + '\', '') -- For replacing something with null will result in null even if it doesn't match.
			end [#Source],		
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			+ '\andromeda_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\' 		
			+ COALESCE(p.Metrix_RootFolderName_Override, o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#METRIX],		
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			+ '\exports_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\'
			+ COALESCE(p.Metrix_RootFolderName_Override, o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#EXPORT],
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			+ '\proclaim_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\'			 
			+ COALESCE(p.Metrix_RootFolderName_Override, o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#PROCLAIM]		
	FROM REFERENCE.Project p
	JOIN REFERENCE.Organization o
		ON p.OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)
	WHERE o.OrganizationID NOT IN (0, -1) AND p.Active = 1
	UNION ALL
	SELECT o.[OrganizationID], o.Description [Organization], null, null, null, null, null, null, 1,
		CASE 
			WHEN o.FTP_RootFolder LIKE '%\' then o.FTP_RootFolder
			ELSE COALESCE(o.FTP_RootFolder + '\', '')
			end [#FTP], 
		CASE 
			WHEN o.Source_RootFolder LIKE '%\' then o.Source_RootFolder
			ELSE COALESCE(o.Source_RootFolder + '\', '')
			end,
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			+ '\andromeda_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\' 
			+ COALESCE(o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#METRIX], 
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			 + '\exports_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\'		
			+ COALESCE(o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#EXPORT],
		CASE WHEN @@SERVERNAME LIKE '%PRD%' then '\\ncimtxfls02.nci.local' else '\\ncimtxfls01' end
			+ '\proclaim_' + SUBSTRING(@@SERVERNAME, 7, 3) + '\'
			+ COALESCE(o.Metrix_RootFolderName, REPLACE(o.Description, ' ', '')) + '\' [#PROCLAIM]		
	FROM REFERENCE.Organization o
	WHERE o.OrganizationID  NOT IN (0, -1)

GO


