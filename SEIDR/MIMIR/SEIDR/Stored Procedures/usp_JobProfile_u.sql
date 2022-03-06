CREATE PROCEDURE [SEIDR].[usp_JobProfile_u]
	@JobProfileID int,	
	@OrganizationID int = null,	
	@ProjectID smallint,
	@LoadProfileID int,
	@JobPriority varchar(10) = null,
	@UserKey varchar(50) = null,
	@UserKey2 varchar(50)= null,
	@RemoveUserKey2 bit = 0,
	@Track bit = null,
	@RequiredThreadID tinyint = null,
	@RemoveThreadID bit = 0,
	@Description varchar(256) = null,
	@SafetyMode bit = 0,
	@SuccessNotificationMail varchar(500) = null,
	@RemoveSuccessNotification bit = 0,
	@StopAfterStepNumber tinyint = null,
	@RemoveStopAFterStepNumber bit = 0
AS
BEGIN
	IF NOT EXISTS(SELECT Null FROM SEIDR.JobProfile WHERE JobProfileID = @JobProfileID AND Active = 1)
	BEGIN
		RAISERROR('Active JobProfile record not found for JobProfileID %d', 16, 1, @JobProfileID)
		RETURN
	END


	IF @Description LIKE '%TO METRIX%' OR @Description LIKE '%To PROCLAIM%' OR @Description LIKE '%TO SOURCE%'
	BEGIN
		RAISERROR('Please use a description that is for the entire profile. Do not reuse the Black Magic job name if multiple jobs are being combined.
Example: "CLIENT - TX - TO SOURCE" should just be "CLIENT - TX" or "CLIENT - Transaction" or something similar.', 16, 1)
		RETURN
	END
	SET @UserKey = NULLIF(LTRIM(RTRIM(@UserKey)), '')
	IF @UserKey is NOT null AND NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = @UserKey)
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
	
	SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '')
	DECLARE @CurrentLoadProfileID int, @CurrentUserKey varchar(50), @CurrentOrganizationID int
	
	SELECT @CurrentLoadProfileID = LoadProfileID,
		@CurrentUserKey = UserKey1,
		@CurrentOrganizationID = OrganizationID, 
		@Description = COALESCE(@Description, Description)
	FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID
	
	IF @OrganizationID is null
		SET @OrganizationID = @CurrentOrganizationID
	IF @UserKey is null
		SET @UserKey = @CurrentUserKey

	IF @SafetyMode = 1 
	AND (@LoadProfileID <> @CurrentLoadProfileID OR @UserKey <> @CurrentUserKey OR @OrganizationID <> @CurrentOrganizationID) --Changes
	BEGIN
		DECLARE @SafetyFailed bit = 0
		SELECT TOP 1 @JobProfileID = JobProfileID
		FROM SEIDR.JobProfile WITH (NOLOCK)
		WHERE Active = 1
		AND JobProfileID <> @JobProfileID
		AND OrganizationID = @OrganizationID
		AND UserKey1 = @UserKey
		AND (@LoadProfileID is null or LoadProfileID is null or LoadProfileID = @LoadProfileID)		
		IF @@ROWCOUNT > 0
		BEGIN
			SELECT ''[ORGANIZATION_USERKEY_LoadProfileID DUPLICATE] WHERE 1=0
			SELECT *
			FROM SEIDR.vw_JobProfile WITH (NOLOCK)
			WHERE OrganizationID = @OrganizationID
			AND JobProfileID <> @JobProfileID
			AND UserKey1 = @UserKey
			--AND ISNULL(LoadProfileID, 0) = ISNULL(@LoadProfileID, 0)
			AND (@LoadProfileID is null or LoadProfileID is null or LoadProfileID = @LoadProfileID)
			
			SET @SafetyFailed = 1
		END

		IF EXISTS(SELECT null FROM SEIDR.JobProfile WITH (NOLOCK)
					WHERE Active = 1 
					AND JobProfileID <> @JobProfileID
					AND LoadProfileID = @LoadProfileID)
		BEGIN
			SELECT ''[LOADPROFILEID DUPLICATE] WHERE 1=0
			SELECT *
			FROM SEIDR.vw_JobProfile WITH (NOLOCK)
			WHERE LoadProfileID = @LoadProfileID
			AND JobProfileID <> @JobProfileID
			
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
			AND JobProfileID <> @JobProfileID
			
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

	
	UPDATE SEIDR.JobProfile
	SET Description = COALESCE(@Description, Description),
		OrganizationID = @OrganizationID,
		ProjectID = @ProjectID,
		JobPriority = COALESCE(@JobPriority, JobPriority),
		LoadProfileID = @LoadProfileID,
		UserKey1 = @UserKey,
		UserKey2 = CASE WHEN @RemoveUserKey2 = 0 then COALESCE(@UserKey2, UserKey2) end,
		RequiredThreadID = CASE WHEN @RemoveThreadID = 0 then COALESCE(@RequiredThreadID, RequiredThreadID) end,
		Track = COALESCE(@Track, Track),
		SuccessNotificationMail = CASE WHEN @RemoveSuccessNotification = 0 then COALESCE(@SuccessNotificationMail, SuccessNotificationMail) end,
		StopAfterStepNumber = CASE WHEN @RemoveStopAfterStepNumber = 0 then COALESCE(@StopAfterStepNumber, StopAfterStepNumber) end
		--,Editor = SUSER_NAME() --trigger
	WHERE JobProfileID = @JobProfileID
	AND Active = 1
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('No Active JobProfile record found.', 16, 1)
		RETURN
	END
	SELECT * 
	FROM SEIDR.vw_JobProfile
	WHERE JobProfileID = @JobProfileID
END