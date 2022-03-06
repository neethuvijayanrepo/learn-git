CREATE PROCEDURE [SEIDR].[usp_JobExecution_i_ss]
	@JobProfileID int,
	@ProjectID int,
	@OrganizationID int,
	@ProcessingDate datetime,
	@FilePath varchar(260),
	@ParentJobExecutionID bigint,
	@StepNumber tinyint = 1,
	@InitializationStatusCode varchar(2) = 'PD', --Allow generic usage - from Export, will use default value, but otherwise may want to specify some other status (e.g., allow immediate kick off the spawned JobExecution)
	@Branch varchar(30) = 'MAIN',
	@UserKeyOverride int = null,
	@JobExecutionID bigint = null output
AS
	
	DECLARE @DupeCheck bigint
	IF @FilePath is not null
	AND EXISTS(SELECT null 
				FROM SEIDR.JobExecution WITH(NOLOCK)
				WHERE SpawningJobExecutionID = @ParentJobExecutionID
				AND JobProfileID = @JobProfileID
				AND Branch = @Branch
				--AND ProcessingDate = @ProcessingDate -- really driving off file path, assuming that more recent has the correct ProcessingDate
				AND filePath = @FilePath) --Note: really only concerned about this when FilePath is populated and matching.
	BEGIN
		SELECT @DupeCheck = JobExecutionID 
		FROM SEIDR.JobExecution
		WHERE SpawningJobExecutionID = @ParentJobExecutionID
		AND JobProfileID = @JobProfileID
		AND Branch = @Branch
		--AND ProcessingDate = @ProcessingDate
		AND filePath = @FilePath
		
		exec SEIDR.usp_JobExecution_Resolve @DupeCheck, 'DUPLICATE'
	END


	--DECLARE @JobExecutionID bigint
	SELECT @JobExecutionID = COALESCE(MAX(JobExecutionID) + 1, 1)
	FROM SEIDR.JobExecution
	

	
	SET IDENTITY_INSERT SEIDR.JobExecution ON;

	INSERT INTO SEIDR.JobExecution(JobExecutionID, JobProfileID, UserKey, UserKey1, UserKey2,
				StepNumber, ExecutionStatusCode, 
				FilePath, ProcessingDate,OrganizationID,ProjectID, SpawningJobExecutionID, Branch)
	SELECT @JobExecutionID, @JobProfileID, COALESCE(@UserKeyOverride, UserKey), UserKey1, UserKey2, 
				@StepNumber, @InitializationStatusCode,
				@FilePath, @ProcessingDate, @OrganizationID, @ProjectID, @ParentJobExecutionID, @Branch
	FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID

	IF @@ROWCOUNT = 0 OR @@ERROR <> 0
		RETURN 1 --Error.

	SET IDENTITY_INSERT SEIDR.JobExecution OFF;	

	SELECT je.*, s.IsError, s.IsComplete
	FROM SEIDR.JobExecution je WITH (NOLOCK)
	JOIN SEIDR.ExecutionStatus s WITH (NOLOCK)
		ON je.ExecutionStatus = (s.[NameSpace] + '.' + s.ExecutionStatusCode)
	WHERE JobExecutionID = @JobExecutionID

RETURN 0
