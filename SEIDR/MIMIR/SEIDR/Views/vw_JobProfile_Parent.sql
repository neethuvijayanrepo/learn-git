CREATE VIEW SEIDR.vw_JobProfile_Parent
AS
SELECT JobProfile_Job_ParentID, 
		jpj1.JobProfileID, 
		jpj1.StepNumber,
		jpjp.JobProfile_JobID,
		jpj1.Description [Step],
		jpj2.JobProfileID ParentJobProfileID, 
		jpj2.StepNumber ParentStepNumber,
		jpjp.Parent_JobProfile_JobID [ParentJobProfile_JobID],
		jpj2.Description [ParentStep],
		jpjp.SequenceDayMatchDifference
	FROM SEIDR.JobProfile_Job_Parent jpjp
	JOIN SEIDR.JobProfile_Job jpj1
		ON jpjp.JobProfile_JobID = jpj1.JobProfile_JobID
	JOIN SEIDR.JobProfile_Job jpj2
		ON jpjp.Parent_JobProfile_JobID = jpj2.JobProfile_JobID
	WHERE jpj1.Active = 1