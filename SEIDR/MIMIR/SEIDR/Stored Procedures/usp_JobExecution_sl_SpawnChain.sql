CREATE PROCEDURE [SEIDR].[usp_JobExecution_sl_SpawnChain]
	@JobExecutionID int
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	Declare @JobProfileiD int
	SELECT @JobProfileID = JobProfileID 
	FROM SEIDR.JobExecution WITH (NOLOCK)
	WHERE JObExecutionID = @JobExecutionID

	;WITH CTE
	AS
	(
		SELECT sj.JobProfileID, jpj.JobProfileID [ParentJobProfileID]
		FROM SEIDR.SpawnJob sj
		JOIN SEIDR.JobProfile_Job jpj
			ON sj.JObProfile_JobID = jpj.JobProfile_JobID
		WHERE sj.JobProfileID = @JobProfileID 
		AND sj.Active =1 
		UNION ALL
		SELECT  sj.JobProfileID, jpj.JobProfileID [ParentJobProfileID]
		FROM SEIDR.SpawnJob sj
		JOIN CTE
			ON sj.JobProfileID = CTE.ParentJobProfileID
		JOIN SEIDR.JobProfile_Job jpj
			ON sj.JObProfile_JobID = jpj.JobProfile_JobID
		WHERE sj.Active = 1
	)
	SELECT * 
	FROM SEIDR.vw_JobProfile
	WHERE JobProfileID IN (SELECT ParentJobProfileID FROM CTE)
	OR JobProfileID = @JobProfileID

	 ;WITH CTE
	 AS
	 (SELECT je1.JobExecutionID, je1.SpawningJobExecutionID, 0 as [Level]
		FROM SEIDR.JobExecution je1
		WHERE JobExecutionID = @JobExecutionID
		UNION ALL
		SELECT je.JobExecutionID, je.SpawningJobExecutionID, Level + 1
		FROM SEIDR.JobExecution je
		JOIN CTE
			ON je.SpawningJobExecutionID = CTE.JobExecutionID
	) --SELECT * FROM CTE
	SELECT CAST('CHILD' as varchar(6)) as Relation, c.[Level], je.*
	INTO #JobExecInfo
	FROM SEIDR.JobExecution je
	JOIN CTE c
		ON je.JobExecutionID = c.JobExecutionID
	WHERE c.[Level] > 0
	AND je.Active = 1
	

	;WITH CTE
	 AS
	 (SELECT je1.JobExecutionID, je1.SpawningJobExecutionID, 0 as [Level]
		FROM SEIDR.JobExecution je1
		WHERE JobExecutionID = @JobExecutionID
		UNION ALL
		SELECT je.JobExecutionID, je.SpawningJobExecutionID, Level + 1
		FROM SEIDR.JobExecution je
		JOIN CTE
			ON je.JobExecutionID = CTE.SpawningJobExecutionID
	)
	INSERT INTO #JobExecInfo
	SELECT CASE WHEN c.Level = 0 then 'SELF' else 'PARENT' end, c.[Level], je.*
	FROM SEIDR.JobExecution je
	JOIN CTE c
		ON je.JobExecutionID = c.JobExecutionID
	WHERE je.Active = 1

	SELECT * 
	FROM #JobExecInfo
	ORDER BY CASE WHEN Level = 0 then '' else Relation end, [Level], JobExecutionID

	DROP TABLE #JobExecInfo

END