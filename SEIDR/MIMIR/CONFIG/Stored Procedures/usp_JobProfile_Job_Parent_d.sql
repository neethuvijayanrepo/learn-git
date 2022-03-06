CREATE PROCEDURE CONFIG.usp_JobProfile_Job_Parent_d
	@JobProfile_Job_ParentID int
AS
BEGIN
	INSERT INTO SEIDR.JobProfile_Job_Parent_Archive(JobProfile_Job_ParentID, JobProfile_JobID, Parent_JobPRofile_JobID, SequenceDayMatchDifference)
	SELECT JobProfile_Job_ParentID, JobProfile_JobID, Parent_JobProfile_JobID, SequenceDaymatchDifference
	FROM SEIDR.JobProfile_Job_Parent
	WHERE JobProfile_Job_ParentID = @JobProfile_Job_ParentID
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Parent Relation not found.', 16, 1)
		RETURN
	END
	DELETE SEIDR.JobProfile_Job_Parent 
	WHERE JobProfile_Job_ParentID = @JobProfile_Job_ParentID
END