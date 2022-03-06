CREATE PROCEDURE [REFERENCE].[usp_Organization_i]
	@OrganizationID int,
	@Description varchar(100),
	@ParentOrganizationID int = 0,
	@DefaultPriorityCode varchar(10) = 'NORMAL',
	@FTP_RootFolder varchar(500),
	@Source_RootFolder varchar(500),
	@Metrix_RootFolderName varchar(100)
AS
BEGIN
	IF @ParentOrganizationID is null
	BEGIN
		RAISERROR('Invalid ParentOrganizationID for new Organization.', 16, 1)
		RETURN
	END
	SET @Description = UTIL.ufn_CleanField(@Description)
	SET @Metrix_RootFolderName = UTIL.ufn_CleanField(@Metrix_RootFolderName)

	IF @parentOrganizationID > 0 AND (@Source_RootFolder is null or @FTP_RootFolder is null)
	BEGIN
		SELECT 
			@Source_RootFolder = COALESCE(@Source_RootFolder, Source_RootFolder),
			@FTP_RootFolder = COALESCE(@FTP_RootFolder, FTP_RootFolder)
		FROM REFERENCE.Organization WITH (NOLOCK)
		WHERE OrganizationID = @ParentOrganizationID
	END
	IF RIGHT(@FTP_RootFolder, 1) = '\'
		SET @FTP_RootFolder = SUBSTRING(@FTP_RootFolder, 1, LEN(@FTP_RootFolder) - 1)
	IF RIGHT(@Source_RootFolder, 1) = '\'
		SET @Source_RootFolder = SUBSTRING(@Source_RootFolder, 1, LEN(@Source_RootFolder) - 1)
	

	INSERT INTO REFERENCE.Organization(OrganizationID, Description, ParentOrganizationID, DefaultPriorityCode, 
		FTP_RootFolder, Source_RootFolder, Metrix_RootFolderName)
	VALUES(@OrganizationID, @Description, @ParentOrganizationID, @DefaultPriorityCode, 
		@FTP_RootFolder, @Source_RootFolder, @Metrix_RootFolderName)
END