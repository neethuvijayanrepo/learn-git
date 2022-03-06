
CREATE FUNCTION [REFERENCE].[usp_Organization_VerifyFolder](@OrganizationID int, @ProjectID smallint, @Folder varchar(500), @FolderType varchar(20))
RETURNS bit
AS
BEGIN
	IF @Folder is null
		RETURN 1
	IF @OrganizationID IN (0, -1)
		RETURN 0
	DECLARE @RC bit = 0
	IF @FolderType = 'FTP'
	BEGIN
		IF @ProjectID IS NULL
		BEGIN
			IF EXISTS(SELECT null 
						FROM REFERENCE.Organization 
						WHERE OrganizationID = @OrganizationID
						AND @Folder LIKE FTP_RootFolder + '%')
				RETURN 1
			RETURN 0
		END
		ELSE IF @ProjectID IS NOT NULL 
			AND EXISTS(SELECT null 
						FROM REFERENCE.vw_Project_Organization 
						WHERE ProjectID = @ProjectID 
						AND @OrganizationID = @OrganizationID
						AND @Folder LIKE FTP_RootFolder + '%')
		BEGIN
			RETURN 1
		END

		RETURN 0
	END
	IF @FolderType = 'SOURCE'
	BEGIN
		IF @ProjectID IS NULL
		BEGIN
			IF EXISTS(SELECT null 
						FROM REFERENCE.Organization 
						WHERE OrganizationID = @OrganizationID
						AND @Folder LIKE Source_RootFolder + '%')
				RETURN 1
			RETURN 0
		END
		ELSE IF @ProjectID IS NOT NULL 
			AND EXISTS(SELECT null 
						FROM REFERENCE.vw_Project_Organization 
						WHERE ProjectID = @ProjectID 
						AND @OrganizationID = @OrganizationID
						AND @Folder LIKE Source_RootFolder + '%' )
		BEGIN
			RETURN 1
		END
		RETURN 0
	END
	RETURN 0
END