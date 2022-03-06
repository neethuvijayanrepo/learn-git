CREATE PROCEDURE REFERENCE.usp_FinalizeOrganizationID
	@TempOrganizationID int,
	@FinalOrganizationID int,
	@ProjectID smallint = null
AS
BEGIN
	IF @TempOrganizationID >= -1
	BEGIN
		SELECT * 
		FROM REFERENCE.Organization
		WHERE OrganizationID < -1 
		AND Description LIKE 'TEMP%'
		RAISERROR('Invalid value for @TempOrganizationID - Must be < -1', 16, 1)
		RETURN
	END
	IF REFERENCE.ufn_Check_Project_Organization(@ProjectID, @FinalOrganizationID) = 0
	BEGIN
		SELECT * 
		FROM REFERENCE.vw_Project_Organization
		WHERE OrganizationID = @FinalOrganizationID		

		RAISERROR('Invalid Project/Organization mapping', 16, 1)
		RETURN
	END

	UPDATE REFERENCE.Organization
	SET FTP_RootFolder = null, 
		Source_RootFolder = null, 
		Metrix_RootFolderName = null
	WHERE OrganizationID = @TempOrganizationID

	UPDATE SEIDR.JobProfile
	SET OrganizationID = @FinalOrganizationID, 
		ProjectID = @ProjectID
	WHERE OrganizationID = @TempOrganizationID
	AND Active = 1

	UPDATE SEIDR.JobExecution
	SET OrganizationID = @FinalOrganizationID, 
		ProjectID = @ProjectID
	WHERE OrganizationID = @TempOrganizationID
	AND Active = 1

	UPDATE SEIDR.FTPAccount
	SET OrganizationID = @FinalOrganizationID,
		ProjectID = @ProjectID
	WHERE OrganizationID = @TempOrganizationID
END