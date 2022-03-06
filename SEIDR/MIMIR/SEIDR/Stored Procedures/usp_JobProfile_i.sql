
CREATE PROCEDURE [SEIDR].[usp_JobProfile_i]
	@Description varchar(256),
	@OrganizationID int,
	@ProjectID smallint,
	@LoadProfileID int,
	@UserKey varchar(50),
	@ScheduleID int = null,
	@UserKey2 varchar(50) = null,
	@RegistrationDestinationFolder varchar(500) = null,
	@FileDateMask varchar(128) = null,
	@SuccessNotificationMail varchar(500) = null,
	@JobPriority varchar(10) = null,
	@ScheduleNoHistory bit = 0,	
	@RequiredThreadID tinyint = null,
	@StopAfterStepNumber tinyint = null,
	@Track bit = 1,
	@SafetyMode bit = 1,
	@JobProfileID int = null	
	AS
BEGIN
	SET @Description = UTIL.ufn_CleanField(@Description)		
	IF @Description is null
	BEGIN
		RAISERROR('@Description required.', 16, 1)
		RETURN
	END
	IF @OrganizationID is null
	BEGIN
		SELECT * FROM REFERENCE.vw_Project_Organization
		RAISERROR('@OrganizationID required.', 16, 1)
		RETURN
	END
	IF @Description LIKE '%TO METRIX%' OR @Description LIKE '%To PROCLAIM%' OR @Description LIKE '%TO SOURCE%' OR @Description LIKE '%TO FTP%'
	BEGIN
		RAISERROR('Please use a description that is for the entire profile. Do not reuse the Black Magic job name if multiple jobs are being combined.
Example: "CLIENT - TX - TO SOURCE" should just be "CLIENT - TX" or "CLIENT - Transaction" or something similar.', 16, 1)
		RETURN
	END
	SET @UserKey = UTIL.ufn_CleanField(@UserKey)
	SET @UserKey2 = UTIL.ufn_CleanField(@UserKey2)
	IF @UserKey is null or NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = @UserKey)
	BEGIN
		IF EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey LIKE '%' + @UserKEy + '%' OR Description LIKE '%' + @UserKey + '%')
			SELECT UserKey, Description
			FROM REFERENCE.UserKey 
			WHERE UserKey LIKE '%' + @UserKey + '%' OR Description LIKE '%' + @UserKey + '%'
		ELSE
			SELECT UserKey, Description
			FROM REFERENCE.UserKey
		RAISERROR('Invalid UserKey: "%s"', 16, 1, @UserKey)
		RETURN
	END

	IF @JobProfileID IS NOT NULL
	BEGIN
		IF EXISTS(SELECT null FROM SEIDR.JobProfile WHERE JobProfileID = @JobProfileID)
		BEGIN
			RAISERROR('JobProfileID %d is already in use.', 16, 1, @JobProfileID)
			RETURN
		END
	END

	DECLARE @DupeJobProfileID int
	SELECT TOP 1 @DupeJobProfileID = JobProfileID
				FROM SEIDR.JobProfile WITH (NOLOCK)
				WHERE Active = 1
				AND Description = @Description
				AND OrganizationID = @OrganizationID
				AND Userkey1 = @UserKey
				AND ISNULL(UserKey2, '') = ISNULL(@UserKey2, '')
				AND ISNULL(ProjectID, 0) = ISNULL(@ProjectID, 0)
	IF @@ROWCOUNT > 0
	BEGIN
		RAISERROR('Profile with description already exists: %d', 16, 1, @DupeJobProfileID)
		RETURN
	END
	IF @SafetyMode = 1
	BEGIN
		DECLARE @SafetyFailed bit = 0
		SELECT TOP 1 @DupeJobProfileID = JobProfileID
		FROM SEIDR.JobProfile WITH (NOLOCK)
		WHERE Active = 1
		AND OrganizationID = @OrganizationID
		AND UserKey1 = @UserKey
		AND (@LoadProfileID is null or LoadProfileID is null or LoadProfileID = @LoadProfileID)
		--AND ISNULL(LoadProfileID, 0) = ISNULL(@LoadProfileID, 0)
		IF @@ROWCOUNT > 0
		BEGIN
			SELECT ''[ORGANIZATION_USERKEY_LoadProfileID DUPLICATE] WHERE 1=0
			SELECT *
			FROM SEIDR.vw_JobProfile WITH (NOLOCK)
			WHERE OrganizationID = @OrganizationID
			AND UserKey1 = @UserKey
			--AND ISNULL(LoadProfileID, 0) = ISNULL(@LoadProfileID, 0)
			AND (@LoadProfileID is null or LoadProfileID is null or LoadProfileID = @LoadProfileID)
			
			SET @SafetyFailed = 1
		END

		IF EXISTS(SELECT null FROM SEIDR.JobProfile WITH (NOLOCK)
					WHERE Active = 1 AND LoadProfileID = @LoadProfileID)
		BEGIN
			SELECT ''[LOADPROFILEID DUPLICATE] WHERE 1=0
			SELECT *
			FROM SEIDR.vw_JobProfile WITH (NOLOCK)
			WHERE LoadProfileID = @LoadProfileID
			
			SET @SafetyFailed = 1
		END

		IF EXISTS(SELECT null 
					FROM SEIDR.JobProfile WITH (NOLOCK)
					WHERE Active = 1 AND Description = @Description)
		BEGIN
			SELECT ''[DESCRIPTION DUPLICATE] WHERE 1=0
			SELECT *
			FROM SEIDR.vw_JobProfile WITH (NOLOCK)
			WHERE Description = @Description
			
			SET @SafetyFailed = 1
		END
		IF @SafetyFailed = 1
		BEGIN
			RAISERROR('@Safetymode identified possible duplication of existing profiles. If intended, please pass @Safetymode = 0', 16, 1)
			RETURN
		END
	END

	
	IF @ProjectID IS NOT NULL
	BEGIN		
		IF NOT EXISTS(SELECT null FROM REFERENCE.vw_Project_Organization WITH (NOLOCK) WHERE ProjectID = @ProjectID AND OrganizationID = @OrganizationID)
		BEGIN
			SELECT 
				IIF(OrganizationID = @OrganizationID, 1, 0) [OrgMatch],
				IIF(ProjectID = @ProjectID, 1, 0) [ProjectMatch],
				*	
			FROM REFERENCE.vw_Project_Organization WITH (NOLOCK)
			WHERE OrganizationID = @OrganizationID
			OR ProjectID = @ProjectID

			RAISERROR('Invalid @OrganizationID/@ProjectID combination', 16, 1)
			RETURN
		END
		IF @Safetymode = 1 AND 0 = (SELECT Active FROM REFERENCE.Project WHERE projectID = @ProjectID)
		BEGIN
			RAISERROR('@ProjectID references inactive project. Pass @SafetyMode = 0 if intended.', 16, 1)
			RETURN
		END
	END
	IF @RegistrationDestinationFolder LIKE '%#%'
		SET @RegistrationDestinationFolder = CONFIG.ufn_ShortHandPath(@RegistrationDestinationFolder, @OrganizationID, @ProjectID, @UserKey, @LoadProfileID)

	IF REFERENCE.usp_Organization_VerifyFolder(@OrganizationID, @ProjectID, @RegistrationDestinationFolder, 'SOURCE') = 0
	BEGIN
		IF @ProjectID is null
			SELECT Source_RootFolder FROM REFERENCE.Organization WHERE OrganizationID = @OrganizationID
		ELSE
			SELECT * FROM REFERENCE.vw_Project_Organization WHERE OrganizationID = @OrganizationID OR ProjectID = @ProjectID
		RAISERROR('Invalid Destination Folder....', 16, 1)
		RETURN
	END
	

	IF @JobPriority is null
	BEGIN
		IF @OrganizationID is not null			
		BEGIN
			SELECT @JobPriority = ISNULL(k.[OverrideOrganizationDefaultPriorityCode], o.DefaultPriorityCode)
			FROM REFERENCE.Organization o WITH (NOLOCK)
			CROSS JOIN REFERENCE.UserKey k WITH (NOLOCK)				
			WHERE OrganizationID = @OrganizationID
			AND k.UserKey = @UserKey
		END
		IF @JobPriority is null
			SET @JobPriority = 'NORMAL'
	END

	IF @JobProfileID IS NOT NULL
	BEGIN
		SET IDENTITY_INSERT SEIDR.JobProfile ON;
		
		INSERT INTO SEIDR.JobProfile(JobProfileID, Description, RequiredthreadID, ScheduleID, UserKey, UserKey1, UserKey2, RegistrationDestinationFolder, FileDateMask,
			SuccessNotificationMail, JobPriority, ScheduleNoHistory, OrganizationID, ProjectID, LoadProfileID, StopAfterStepNumber)
		OUTPUT INSERTED.*
		VALUES(@JobProfileID, @Description, @RequiredThreadID, @ScheduleID, @OrganizationID, @UserKey, @UserKey2, @RegistrationDestinationFolder, @FileDateMask,
			@SuccessNotificationMail, @JobPriority, @ScheduleNoHistory, @OrganizationID, @ProjectID, @LoadProfileID, @StopAfterStepNumber)

		SET IDENTITY_INSERT SEIDR.JobProfile OFF;
		RETURN @JobProfileID
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.JobProfile(Description, RequiredthreadID, ScheduleID, UserKey, UserKey1, UserKey2, RegistrationDestinationFolder, FileDateMask,
			SuccessNotificationMail, JobPriority, ScheduleNoHistory, OrganizationID, ProjectID, LoadProfileID, StopAfterStepNumber)
		OUTPUT INSERTED.*
		VALUES(@Description, @RequiredThreadID, @ScheduleID, @OrganizationID, @UserKey, @UserKey2, @RegistrationDestinationFolder, @FileDateMask,
			@SuccessNotificationMail, @JobPriority, @ScheduleNoHistory, @OrganizationID, @ProjectID, @LoadProfileID, @StopAfterStepNumber)
		RETURN SCOPE_IDENTITY()
	END
END