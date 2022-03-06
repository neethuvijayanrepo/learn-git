
CREATE FUNCTION [REFERENCE].[ufn_CompareProfile_Organization](@JobProfile1 int, @JobProfile2 int)
RETURNS BIT
AS
BEGIN	
	IF EXISTS(SELECT null		
				FROM SEIDR.JobProfile 
				WHERE JobProfileID IN (@JobProfile1, @JobProfile2) 
				AND OrganizationID = -1)
		RETURN 0 --Invalid
	IF EXISTS(SELECT null
				FROM SEIDR.JobProfile jp WITH (NOLOCK)
				JOIN REFERENCE.Organization o WITH (NOLOCK)
					ON jp.OrganizationID = o.OrganizationID
				JOIN SEIDR.JobProfile jp2 WITH (NOLOCK)
					ON jp2.OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)
				WHERE Jp.JobProfileID = @JobProfile1
				AND jp2.JobProfileID = @JobProfile2
				)
		RETURN 1
	IF EXISTS(SELECT null
				FROM SEIDR.JobProfile jp WITH (NOLOCK)
				JOIN REFERENCE.Organization o WITH (NOLOCK)
					ON jp.OrganizationID = o.OrganizationID
				JOIN SEIDR.JobProfile jp2 WITH (NOLOCK)
					ON jp2.OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)
				WHERE Jp.JobProfileID = @JobProfile2
				AND jp2.JobProfileID = @JobProfile1)
		RETURN 1
	RETURN 0
END