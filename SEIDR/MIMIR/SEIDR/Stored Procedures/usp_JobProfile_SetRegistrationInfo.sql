
 CREATE PROCEDURE [SEIDR].[usp_JobProfile_SetRegistrationInfo]
	@JobProfileID int,
	@FileDateMask varchar(128),
	@RemoveFileDateMask bit = 0,
	@FileFilter varchar(600), -- Filter to pick files up with - Multiple filters can be put in this field, delimited by ';'
	@FileExclusionFilter varchar(600) = null,	 -- Filter to AVOID files with - multiple exclusion filters can be put in this field, delimited by ';'
	@RemoveFileExclusionFilter bit = 0,
	@RegistrationFolder varchar(250) = null,
	@RemoveRegistrationFolder bit = 0,
	@RegistrationDestinationFolder varchar(250) = null,
	@RemoveRegistrationDestinationFolder bit = 0,
	@DeliveryScheduleID int = null,
	@RemoveDeliverySCheduleID bit = 0,
	@SafetyMode bit = 1
AS
BEGIN		
	IF @FileDateMask LIKE '%<[+-][0-9]%Y>%' OR @FileDateMask LIKE '%<[+-][0-9]%M>%'
	BEGIN
		RAISERROR('Date Mask Offset can only be auto-Corrected at day level. Please use the format <#YYYY#MM#DD> to set the offset, where # is +/- numeric.', 16, 1)
		RETURN
	END
	IF @FileDateMask LIKE '%<[+-][0-9][0-9]D>%'
	BEGIN
		SELECT @FileDateMask = STUFF(@FileDateMask, PATINDEX('%<[+-][0-9][0-9]D>%', @FileDateMask), 1, '<0YYYY0MM')
		RAISERROR('Correcting Date Offset for Registration..."%s"', 0, 0, @FileDateMask) WITH NOWAIT
	END
	else if @FileDateMask LIKE '%<[+-][0-9]D>%'
	BEGIN
		SELECT @FileDateMask = STUFF(@FileDateMask, PATINDEX('%<[+-][0-9]D>%', @FileDateMask), 1, '<0YYYY0MM')
		RAISERROR('Correcting Date Offset for Registration..."%s"', 0, 0, @FileDateMask) WITH NOWAIT
	END
	IF @FileFilter LIKE '%<%' OR @FileFilter LIKE '%>%'
	BEGIN
		RAISERROR('File Filter cannot be date masked.', 16, 1)
		RETURN;
	END
	IF @FileExclusionFilter LIKE '%<%' OR @FileExclusionFilter LIKE '%>%'
	BEGIN
		RAISERROR('File Exlusion filter cannot be date masked.', 16, 1);
	END

	IF @RegistrationFolder LIKE '%<%' OR @RegistrationFolder LIKE '%>%'
	BEGIN
		RAISERROR('RegistrationFolder cannot be Date Masked.', 16, 1)
		RETURN;
	END

	IF @RegistrationDestinationFolder LIKE '%<[+-][0-9]%>%'
	OR @RegistrationDestinationFolder LIKE '%<[0-9]%>%'
	BEGIN
		RAISERROR('RegistrationDestinationFolder Date Mask cannot be offset.', 16, 1)
		RETURN
	END


	DECLARE @OrganizationID int, @ProjectID smallint, @UserKey varchar(50), @LoadProfileiD int
	SELECT @OrganizationID = OrganizationID, @ProjectID = ProjectID, @UserKey = UserKey1, @LoadProfileiD = LoadProfileID
	FROM SEIDR.JobProfile WITH (NOLOCK)
	WHERE JobProfileID = @JobProfileID

	IF @RegistrationFolder LIKE '%#%' OR @RegistrationDestinationFolder LIKE '%#%'
	BEGIN
		SELECT @RegistrationFolder = CONFIG.ufn_ShortHandPath(@RegistrationFolder, @OrganizationID, @ProjectID, @UserKey, @LoadProfileID),
				@RegistrationDestinationFolder = CONFIG.ufn_ShortHandPath(@RegistrationDestinationFolder, @OrganizationID, @ProjectID, @UserKey, @LoadProfileID)				

		RAISERROR('@RegistrationFolder set to "%s". @RegistrationDestinationFolder set to "%s"', 0, 0, @RegistrationFolder, @RegistrationDestinationFolder)
	END

	IF @SafetyMode = 1
	BEGIN
		DECLARE @SafetyFail bit = 0
		IF REFERENCE.usp_Organization_VerifyFolder(@OrganizationID, @ProjectID, @RegistrationDestinationFolder, 'SOURCE') = 0
		BEGIN
			IF @ProjectID is null
				SELECT Source_RootFolder FROM REFERENCE.Organization WHERE OrganizationID = @OrganizationID
			ELSE
				SELECT * FROM REFERENCE.vw_Project_Organization WHERE OrganizationID = @OrganizationID OR ProjectID = @ProjectID
			RAISERROR('Invalid Destination Folder....', 16, 1)
			SET @SafetyFail = 1
		END		
		IF REFERENCE.usp_Organization_VerifyFolder(@OrganizationID, @ProjectID, @RegistrationFolder, 'FTP') = 0
		BEGIN
			IF @ProjectID is null
				SELECT Source_RootFolder FROM REFERENCE.Organization WHERE OrganizationID = @OrganizationID
			ELSE
				SELECT * FROM REFERENCE.vw_Project_Organization WHERE OrganizationID = @OrganizationID OR ProjectID = @ProjectID
			RAISERROR('Invalid Registration Folder....', 16, 1)
			SET @SafetyFail = 1
		END
		IF @SafetyFail = 1
			RETURN
	END

	
	UPDATE SEIDR.JobProfile
	SET FileDateMask = CASE WHEN @RemoveFileDateMask = 0 then ISNULL(@FileDateMask, FileDateMask) end,
		FileFilter = @FileFilter,
		FileExclusionFilter = CASE WHEN @RemoveFileExclusionFilter = 0 then COALESCE(@FileExclusionFilter, FileExclusionFilter) end,
		RegistrationFolder = CASE WHEN @RemoveRegistrationFolder = 0 then COALESCE( @RegistrationFolder, RegistrationFolder) end,
		RegistrationDestinationFolder = CASE WHEN @RemoveRegistrationDestinationFolder = 0 then COALESCE(@RegistrationDestinationFolder, RegistrationDestinationFolder) end,
		DeliveryScheduleID = CASE WHEN @RemoveDeliverySCheduleID = 0 then COALESCE(@DeliveryScheduleID, DeliveryScheduleID) end
	WHERE JobProfileID = @JobProfileID
	

	SELECT * FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID
END