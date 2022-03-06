

/* SP with parameters OrganizationId and ProjectId*/
CREATE PROCEDURE SEIDR.usp_JobProfile_sl_Description
@OrganizationId  int,
@ProjectId int
AS
BEGIN
	SELECT JobProfileID,Description 
	FROM SEIDR.JobProfile WITH (NOLOCK) 
	WHERE OrganizationID = @OrganizationId 
	AND ProjectID = @ProjectId 
	AND Description IS NOT NULL 
	AND Active = 1
END