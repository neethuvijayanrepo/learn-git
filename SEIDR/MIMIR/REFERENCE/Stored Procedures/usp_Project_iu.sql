
CREATE PROCEDURE [REFERENCE].[usp_Project_iu]
	@ProjectID smallint, 
	@Description varchar(150),
	@CRCM bit = null,
	@FromDate date = null,
	@ThroughDate date = null,
	@RemoveThroughDate bit = 0,
	@OrganizationID int = null,
	@FTP_RootFolderOverride varchar(500) = null,
	@Source_RootFolderOverride varchar(500) = null,
	@Metrix_RootFolderName_Override varchar(100) = null
AS
BEGIN
	
	IF RIGHT(@FTP_RootFolderOverride, 1) = '\'
		SET @FTP_RootFolderOverride = SUBSTRING(@FTP_RootFolderOverride, 1, LEN(@FTP_RootFolderOverride) - 1)
	IF RIGHT(@Source_RootFolderOverride, 1) = '\'
		SET @Source_RootFolderOverride = SUBSTRING(@Source_RootFolderOverride, 1, LEN(@Source_RootFolderOverride) - 1)
	SELECT @Description = UTIL.ufn_CleanField(@Description),
		@Metrix_RootFolderName_Override = UTIL.ufn_CleanField(@Metrix_RootFolderName_Override)

	IF @Description is null
	BEGIN
		RAISERROR('Invalid @Description.', 16, 1)
		RETURN
	END
	IF @OrganizationID <= 0
	BEGIN
		RAISERROR('Invalid @OrganizationID: %d', 16, 1, @OrganizationID)
		RETURN
	END
	IF EXISTS(SELECT null FROM REFERENCE.Project WHERE ProjectID = @ProjectID)
	BEGIN
		UPDATE REFERENCE.Project
		SET Description = @Description,
			FromDate = COALESCE(@FromDate, FromDate),
			ThroughDate = CASE WHEN @RemoveThroughDate = 0 then COALESCE(@ThroughDate, ThroughDate) end,
			OrganizationID = COALESCE(@OrganizationID, OrganizationID),
			CRCM = COALESCE(@CRCM, CRCM),
			FTP_RootFolderOverride = @FTP_RootFolderOverride,
			Source_RootFolderOverride = @Source_RootFolderOverride,
			Metrix_RootFolderName_Override = @Metrix_RootFolderName_Override
		WHERE ProjectID = @ProjectID

		RETURN
	END
	IF @OrganizationID is null
	BEGIN
		RAISERROR('Please provide @OrganizationID for new Project record', 16, 1)
		RETURN
	END
	IF @FromDate is null 
		SET @FromDate = CONVERT(date, GETDATE())
	IF @ThroughDate is not null AND @ThroughDate < @FromDate
	BEGIN
		RAISERROR('Invalid ThroughDate - must be later than FromDate', 16, 1)
		RETURN
	END
	
	IF @CRCM is null
	BEGIN
		RAISERROR('Please provide value for @CRCM for new Project record', 16,1 )
		RETURN
	END
	INSERT INTO REFERENCE.Project(ProjectID, Description, CRCM, FromDate, ThroughDate, OrganizationID, 
		FTP_RootFolderOverride, Source_RootFolderOverride, Metrix_RootFolderName_Override)
	VALUES(@ProjectID, @Description, @CRCM, @FromDate, @ThroughDate, @OrganizationID, 
		@FTP_RootFolderOverride, @Source_RootFolderOverride, @Metrix_RootFolderName_Override)
END