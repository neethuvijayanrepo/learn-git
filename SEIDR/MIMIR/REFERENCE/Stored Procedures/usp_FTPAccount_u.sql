CREATE PROCEDURE [REFERENCE].[usp_FTPAccount_u]
	@FTPAccountID int,
	@PpkFileName nvarchar(100) = null,
	@RemovePpkFileName bit = 0,
	@OrganizationID int = null,
	@ProjectID smallint,
	@Description varchar(100) = null,	
	@Fingerprint nvarchar(100) = null,	
	@FTPProtocolID int = null,
	@EncryptedPassword varchar(255) = null,
	@TransferResumeSupport bit = null
AS
BEGIN	
	UPDATE a
	SET [Description] = COALESCE(@Description, [Description]),
		OrganizationID = ISNULL(@OrganizationID, a.OrganizationID),
		ProjectID = @ProjectID,
		FTPProtocolID = p.FTPProtocolID,		
		Fingerprint = COALESCE(@Fingerprint, a.FingerPrint),
		PpkFileName = CASE WHEN @RemovePpkFileName = 0 then COALESCE(@PpkFileName, a.PpkFileName) end,
		[Password] = COALESCE(@EncryptedPassword, a.[Password]),
		TransferResumeSupport = COALESCE(@TransferResumeSupport, a.TransferResumeSupport)
	FROM SEIDR.FTPAccount a
	JOIN SEIDR.FTPProtocol p
		ON p.FTPProtocolID = COALESCE(@FTPProtocolID, a.FTPProtocolID)
	WHERE a.FTPAccountID = @FTPAccountID
	
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED 
	SELECT FTPAccountID, ftp.FTPProtocolID, UPPER(pr.Protocol)[Protocol], ftp.TransferResumeSupport, ftp.Description, UserName, Port, Passive, FingerPrint, PPKFilename,
		ftp.OrganizationID, o.Description [Organization],
		ftp.ProjectID, p.Description [Project], p.CRCM, p.Modular, p.Active [ProjectActive], p.FromDate, p.ThroughDate
	FROM SEIDR.FTPAccount ftp
	JOIN SEIDR.FTPProtocol pr
		ON ftp.FTPProtocolID = pr.FTPProtocolID
	LEFT JOIN REFERENCE.Organization o
		ON ftp.organizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON ftp.ProjectID = p.ProjectID
	WHERE ftp.FTPAccountID = @FTPAccountID
END