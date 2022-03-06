CREATE PROCEDURE SEIDR.usp_JobProfile_sl_Organization
AS
BEGIN
	SELECT DISTINCT OrganizationId 
	FROM SEIDR.JobProfile WITH (NOLOCK) 
	WHERE OrganizationId IS NOT NULL 
	AND Active = 1
END