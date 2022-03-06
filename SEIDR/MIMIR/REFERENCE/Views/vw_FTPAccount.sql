

CREATE VIEW [REFERENCE].[vw_FTPAccount]
	AS
	SELECT FTPAccountID, ftp.FTPProtocolID, UPPER(pr.Protocol)[Protocol], ftp.TransferResumeSupport, ftp.Description, 
		UserName, Port, Passive, FingerPrint, PPKFilename,
		ftp.OrganizationID, o.Description [Organization],
		ftp.ProjectID, p.Description [Project], p.CRCM, p.Modular, p.Active [ProjectActive], p.FromDate, p.ThroughDate
	FROM SEIDR.FTPAccount ftp
	JOIN SEIDR.FTPProtocol pr
		ON ftp.FTPProtocolID = pr.FTPProtocolID
	LEFT JOIN REFERENCE.Organization o
		ON ftp.organizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON ftp.ProjectID = p.ProjectID
	WHERE Ftp.DD IS NULL