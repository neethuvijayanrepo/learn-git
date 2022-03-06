
CREATE PROCEDURE REFERENCE.usp_Organization_u
	@OrganizationID int,
	@Description varchar(100) = null,
	@ParentOrganizationID int = null,
	@DefaultPriorityCode varchar(10) = null,
	@FTP_RootFolder varchar(500) = null,
	@Source_RootFolder varchar(500) = null,
	@Metrix_RootFolderName varchar(100) = null,
	@NewOrganizationID int = null
	
AS
BEGIN
	SELECT @Description = UTIL.ufn_CleanField(@Description),
			@DefaultPriorityCode = UTIL.ufn_CleanField(@DefaultPriorityCode),
			@FTP_RootFolder = UTIL.ufn_CleanField(@FTP_RootFolder),
			@Source_RootFolder = UTIL.ufn_CleanField(@Source_RootFolder),
			@Metrix_RootFolderName = UTIL.ufn_CleanField(@Metrix_rootFolderName)

	UPDATE REFERENCE.Organization
	SET 		
		Description = COALESCE(@Description, Description),
		ParentOrganizationID = COALESCE(@ParentOrganizationID, ParentOrganizationID),
		DefaultPriorityCode = COALESCE(@DefaultPriorityCode, DefaultPriorityCode),
		FTP_RootFolder = COALESCE(@FTP_RootFolder, FTP_RootFolder),
		Source_RootFolder = COALESCE(@Source_RootFolder, Source_RootFolder),
		Metrix_RootFolderName = COALESCE(@Metrix_RootFolderName, Metrix_RootFolderName)
	WHERE OrganizationID = @OrganizationID

	IF @NewOrganizationID IS NOT NULL
	BEGIN
		UPDATE REFERENCE.Organization
		SET OrganizationID = @NewOrganizationID
		WHERE OrganizationID = @OrganizationID
		SET @OrganizationID = @NewOrganizationID
	END		
	
	SELECT * 
	FROM REFERENCE.Organization 
	WHERE OrganizationID = @OrganizationID
END