CREATE PROCEDURE [SEIDR].[usp_JobExecution_RegisterSpawnJob]
(
        @SpawningJobExecutionID	BIGINT,
		@ProcessingDate			DATETIME,
		@SpawnJobList			SEIDR.udt_SpawnJob READONLY        
)
AS
BEGIN
	INSERT INTO SEIDR.JobExecution
	(
		JobProfileID, 
		UserKey, 
		UserKey1, 
		UserKey2,
		StepNumber,
		ExecutionStatusCode, 
		FilePath, 
		FileSize, 
		FileHash, 
		ProcessingDate,
		OrganizationID,
		ProjectID,
		LoadProfileID,
		SpawningJobExecutionID
	)
	SELECT 
		sjl.JobProfileID, 
		jp.UserKey, 
		jp.UserKey1, 
		jp.UserKey2, 
		1, 
		'SP',
		sjl.SourceFile, 
		sjl.FileSize, 
		sjl.FileHash, 
		@ProcessingDate, 
		jp.OrganizationID, 
		jp.ProjectID, 
		jp.LoadProfileID,
		@SpawningJobExecutionID
	FROM 
		@SpawnJobList sjl
		JOIN SEIDR.JobProfile jp
			ON sjl.JobProfileID = jp.JobProfileID
		--JOIN SEIDR.JobExecution se WITH(NOLOCK) ON se.JobExecutionID = @SpawningJobExecutionID
	RETURN 0
END

GO
