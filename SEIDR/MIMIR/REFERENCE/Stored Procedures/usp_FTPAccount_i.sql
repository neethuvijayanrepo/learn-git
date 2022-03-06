CREATE PROCEDURE [REFERENCE].[usp_FTPAccount_i]
	@Description varchar(100),
	@OrganizationID int,
	@ProjectID smallint,
	@FTPProtocol varchar(50),
	@Server nvarchar(255),
	@UserName nvarchar(50) = null,
	@Password varchar(255) = null,
	@Port int = 22,
	@Passive bit = 0,
	@Fingerprint nvarchar(100) = null,
	@PpkFileName nvarchar(100) = null,
	@TransferResumeSupport bit = 1
AS
BEGIN	
	DECLARE @FTPAccountID int
	DECLARE @FTPProtocolID int
	SELECT @FTPProtocolID = FTPProtocolID
	FROM SEIDR.FTPProtocol 
	WHERE Protocol = @FTPProtocol
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT Protocol 
		FROM SEIDR.FTPPRotocol
		RAISERROR('Protocol not found: %s', 16, 1, @FTPProtocol);
		RETURN
	END

	SET @Description = UTIL.ufn_CleanField(@Description)

	INSERT INTO SEIDR.FTPAccount(FTPProtocolID, Description, Server, UserName, Password, Port, Passive, Fingerprint, PpkFileName, ProjectID, OrganizationID, DC, TransferResumeSupport)
	VALUES(@FTPProtocolID, @Description, @Server, @UserName, @Password, @Port, @Passive, @Fingerprint, @PpkFileName, @ProjectID, @OrganizationID, GETDATE(), @TransferResumeSupport)
	
	SELECT @FTPAccountID = SCOPE_IDENTITY()
	
	SELECT FTPAccountID, FTPProtocolID, UPPER(@FTPProtocol)[Protocol], ftp.TransferResumeSupport, ftp.Description, UserName, Port, Passive, FingerPrint, PPKFilename,
		ftp.OrganizationID, o.Description [Organization],
		ftp.ProjectID, p.Description [Project], p.CRCM, p.Modular, p.Active [ProjectActive], p.FromDate, p.ThroughDate
	FROM SEIDR.FTPAccount ftp
	LEFT JOIN REFERENCE.Organization o
		ON ftp.organizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON ftp.ProjectID = p.ProjectID
	WHERE ftp.FTPAccountID = @FTPAccountID
END