CREATE VIEW SEIDR.vw_JobProfile_ChildProfile
AS
	SELECT  jp.JobProfileID,'SP' as [ChildType], sp.JobProfileID [ChildJobProfileID], SpawnJobID, null [JobProfile_Job_ParentID]
	FROM SEIDR.JobProfile jp
	JOIN SEIDR.JobProfile_Job jpj
		ON jp.JobProfileID = jpj.JobProfileID
	JOIN SEIDR.SpawnJob sp
		ON jpj.JobProfile_JobID = sp.JobProfile_JobID
	UNION ALL
	SELECT  p.JobProfileID,'J', jpj.JobProfileID [ChildJobProfileID], null, jpjp.JobProfile_Job_ParentID
	FROM SEIDR.JobProfile_Job_Parent jpjp
	JOIN SEIDR.JobProfile_job jpj
		ON jpjp.JobProfile_JobID = jpj.JobProfile_JobID
	JOIN SEIDR.JobProfile_Job p
		ON jpjp.Parent_JobProfile_JobID = p.JobProfile_JobID