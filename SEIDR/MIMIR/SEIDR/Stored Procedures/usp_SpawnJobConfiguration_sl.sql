CREATE PROCEDURE [SEIDR].[usp_SpawnJobConfiguration_sl]
(
	@JobProfile_JobID INT
)
AS  
BEGIN    
	SELECT 
		sj.[SpawnJobID],
		sj.[JobProfile_JobID],
		sj.[JobProfileID],
		sj.[SourceFile]
	FROM 
		[SEIDR].[SpawnJob] sj
	WHERE
		JobProfile_JobID = @JobProfile_JobID
		AND sj.Active = 1
END