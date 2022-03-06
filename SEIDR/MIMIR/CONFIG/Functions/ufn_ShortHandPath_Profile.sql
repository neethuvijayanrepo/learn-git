CREATE FUNCTION CONFIG.ufn_ShortHandPath_Profile
(@FilePath varchar(500),
	@JobProfileID int
)
RETURNS varchar(500)
AS
BEGIN
	DECLARE @OrganizationID int, @ProjectID int, @UserKey varchar(50), @LoadProfileID int
	SELECT @OrganizationID = OrganizationID, @ProjectID = ProjectID, @UserKey = UserKey1, @LoadProfileID = LoadProfileID 
	FROM SEIDR.JobProfile WITH (NOLOCK)
	WHERE JobPRofileID = @JobProfileID

	RETURN CONFIG.ufn_ShortHandPath(@FilePath, @OrganizationID, @ProjectID, @UserKey, @LoadProfileID)
END