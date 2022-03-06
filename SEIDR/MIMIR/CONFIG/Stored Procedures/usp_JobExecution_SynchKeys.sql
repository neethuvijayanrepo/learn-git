
CREATE PROCEDURE CONFIG.usp_JobExecution_SynchKeys
	@JobExecutionID int
AS
BEGIN
	IF EXISTS(SELECT null FROM CONFIG.vw_JobExecution_JobProfile_KeyMismatch WHERE JobExecutionID = @JobExecutionID)
	BEGIN
		SELECT *
		FROM CONFIG.vw_JobExecution_JobProfile_KeyMismatch 
		WHERE JobExecutionID = @JobExecutionID
	END
	ELSE
		SELECT JobExecutionID, OrganizationID, Organization, ProjectID, Project, CRCM, Modular, ProjectActive, FullUserKey
		FROM REFERENCE.vw_JobExecution 
		WHERE JobExecutionID = @JobExecutionID
	
	UPDATE je
	SET OrganizationID = jp.OrganizationID,
		ProjectID = jp.ProjectID,
		UserKey1 = jp.UserKey1,
		UserKey2 = jp.userKey2
	FROM SEIDR.JobExecution je
	JOIN SEIDR.JobProfile jp
		ON je.JobProfileID = jp.JobProfileiD
	WHERE je.JobExecutionID = @JobExecutionID
	AND je.Active = 1
	AND (je.OrganizationID <> jp.OrganizationID OR ISNULL(je.ProjectID, -1) <> ISNULL(jp.ProjectID, -1)
			OR je.UserKey1 <> jp.UserKey1 OR ISNULL(Je.UserKey2, '') <> ISNULL(jp.UserKey2, '')
			)

	IF @@ROWCOUNT > 0	
		SELECT OrganizationID, Organization, ProjectID, Project, CRCM, Modular, ProjectActive, FullUserKey
		FROM REFERENCE.vw_JobExecution 
		WHERE JobExecutionID = @JobExecutionID
	ELSE
		RAISERROR('No Change.', 16, 1)
END