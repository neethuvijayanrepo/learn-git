CREATE PROCEDURE CONFIG.usp_JobProfile_Rollback
	@JobProfileID int,
	@JobProfileHistoryID int
AS
BEGIN
	UPDATE jp
	SET OrganizationID = h.OrganizationID,
		ProjectID = h.ProjectID,
		LoadProfileID = h.LoadProfileID,
		UserKey1 = h.userKey1,
		UserKey2 = h.UserKey2,
		RegistrationFolder = h.RegistrationFolder,
		FileFilter = h.FileFilter,
		FileDateMask = h.FileDateMask,
		RegistrationDestinationFolder = h.RegistrationDestinationFolder,
		ScheduleID = h.ScheduleID,
		ScheduleThroughDate = COALESCE(h.ScheduleThroughDate, jp.ScheduleThroughDate),
		ScheduleFromDate = h.ScheduleFromDate
	FROM SEIDR.JobProfile jp
	JOIN SEIDR.JobProfileHistory h
		ON jp.JobProfileID = h.JobProfileID
	WHERE jp.JobProfileID = @JobProfileID
	AND H.JobProfileHistoryID = @JobProfileHistoryID
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT * 
		FROM SEIDR.JobProfileHistory
		WHERE JobProfileID = @JobProfileID
		ORDER BY JobProfileHistoryID DESC
		RAISERROR('No Matching history found for profile.', 16, 1)
		RETURN
	END
	RAISERROR('Note: ScheduleThroughDate does not clear from history reverting.', 0, 0)

	SELECT *
	FROM SEIDR.vw_JobProfile 
	WHERE JobProfileID = @JobProfileID
END