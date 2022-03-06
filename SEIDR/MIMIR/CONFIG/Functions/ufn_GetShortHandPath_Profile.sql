CREATE FUNCTION CONFIG.ufn_GetShortHandPath_Profile
	(
		@FilePath varchar(500),
		@JobProfileID int
	)
RETURNS Varchar(500)
AS
BEGIN
	DECLARE @OrganizationID int, @ProjectID int, @UserKey varchar(50), @LoadProfileID int
	SELECT @OrganizationiD = OrganizationID,
			@ProjectID = ProjectID,
			@UserKey = UserKey1,
			@LoadProfileID = LoadProfileID
	FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID

	RETURN CONFIG.ufn_GetShortHandPath(@FilePath, @OrganizationID, @ProjectID, @UserKey, @LoadProfileID)
END