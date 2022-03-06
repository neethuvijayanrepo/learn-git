
CREATE FUNCTION REFERENCE.ufn_Check_Project_Organization(@ProjectID smallint, @OrganizationID int)
RETURNS BIT
AS
BEGIN
	IF @ProjectID Is null 
		RETURN 1
	IF @OrganizationID = 0
		RETURN 1
	IF EXISTS(SELECT null FROM REFERENCE.vw_Project_Organization WITH (NOLOCK) WHERE ProjectID = @ProjectID And OrganizationID = @OrganizationID)
		RETURN 1
	RETURN 0
END