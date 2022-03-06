CREATE PROCEDURE [SEIDR].[usp_Job_RegisterFile]
        @JobProfileID int,
        @FilePath varchar(255),        
        @FileSize bigint,
        @FileDate date,
		@FileHash varchar(88),
		@StepNumber smallint,
		@QueueAfterRegister bit = 1    
AS
BEGIN
	IF EXISTS(SELECT null 
				FROM SEIDR.JobExecution je WITH (NOLOCK)
				WHERE je.FilePath = @FilePath
				AND je.DD IS NULL -- Filtered unique index usage
				AND je.JobProfileID = @JobProfileID
				UNION ALL
				SELECT NULL
				FROM SEIDR.vw_JobExecutionHistory h --History may be null if StopAfterStepNumber = 0, or if RetryCount < RetryLimit and StepNumber = 1
				JOIN SEIDR.JobExecution je WITH (NOLOCK)
					ON h.JobExecutionID = je.JobExecutionID 
				WHERE je.JobProfileID = @JobProfileID 
				AND je.Active = 1 -- Different index usage from above
				AND h.FilePath = @FilePath
			)
	BEGIN		
		RETURN -1;
	END
	

	DECLARE @JobExecutionID bigint
	SELECT @JobExecutionID = COALESCE(MAX(JobExecutionID) + 1, 1)
	FROM SEIDR.JobExecution
	
	SET IDENTITY_INSERT SEIDR.JobExecution ON;

	INSERT INTO SEIDR.JobExecution(JobExecutionID, JobProfileID, UserKey, UserKey1, UserKey2,
				StepNumber, ExecutionStatusCode, 
				FilePath, FileSize, FileHash, ProcessingDate,OrganizationID,ProjectID,LoadProfileID)
	SELECT @JobExecutionID, @JobProfileID, UserKey, UserKey1, UserKey2, 
				@StepNumber, 'R',
				@FilePath, @FileSize, @FileHash, @FileDate,OrganizationID,ProjectID,LoadProfileID
	FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID

	IF @@ROWCOUNT = 0 OR @@ERROR <> 0
		RETURN 1 --Error.

	--SELECT @JobExecutionID = SCOPE_IDENTITY()
	SET IDENTITY_INSERT SEIDR.JobExecution OFF;	

	SELECT * FROM SEIDR.vw_JobExecution WHERE JobExecutionID = @JobExecutionID AND CanQueue = 1 AND InSequence = 1
	IF @@ROWCOUNT > 0 AND @QueueAfterRegister = 1
	BEGIN
		UPDATE SEIDR.JobExecution
		SET InWorkQueue = 1
		WHERE JobExecutionID = @JobExecutionID
	END
	RETURN 0
END





