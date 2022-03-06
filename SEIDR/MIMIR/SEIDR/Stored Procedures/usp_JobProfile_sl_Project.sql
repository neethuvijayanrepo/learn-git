

/* SP with parameter OrganizationId */
CREATE PROCEDURE SEIDR.usp_JobProfile_sl_Project
(
@OrganizationId  int
)
AS
BEGIN
	SELECT DISTINCT ProjectID 
	FROM SEIDR.JobProfile WITH (NOLOCK)
	WHERE OrganizationID = @OrganizationId 
	AND ProjectID IS NOT NULL 
	AND Active = 1
END