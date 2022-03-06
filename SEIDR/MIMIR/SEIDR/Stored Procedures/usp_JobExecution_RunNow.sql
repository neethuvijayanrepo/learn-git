CREATE PROCEDURE [SEIDR].[usp_JobExecution_RunNow] 
	@JobExecutionID bigint,
	@NoWaitLimit bit = 0,
	@ForceSequence bit = null,
	@UserNote varchar(2000) = null
AS
BEGIN
	SET XACT_ABORT ON

	IF EXISTS(SELECT null 
				FROM SEIDR.JobExecution je
				WHERE jobExecutionID = @JobExecutionID 
				AND JobProfile_JobID IS NULL)
	BEGIN
		RAISERROR('JobExecution is not in valid state for running now. (Null JobProfile_JobID)', 16, 1)
		RETURN
	END
	IF EXISTS(SELECT null FROM SEIDR.JobExecution WITH (NOLOCK) WHERE JobExecutionID = @JobExecutionID AND (PrioritizeNow = 1 OR IsWorking = 1))
	BEGIN
		RAISERROR('JobExecution is already prioritized.', 16, 1)
		RETURN
	END

	SET @UserNote = UTIL.ufn_CleanField(@UserNote)
	IF @UserNote is null
		SET @UserNote = 'Set Prioritize Now via RunNow'

	EXEC SEIDR.usp_JobExecution_Note_i
		@JobExecutionID = @JobExecutionID,
		@NoteText = @UserNote,
		@Technical = 0--, @Auto = 1

	UPDATE SEIDR.JobExecution
	SET PrioritizeNow = 1, 
		ForceSequence = COALESCE(@ForceSequence, ForceSequence)
	WHERE JobExecutionID = @JobExecutionID

	DECLARE @LU datetime, @i int = 0
	SELECT @LU = LU 
	FROM SEIDR.JobExecution WITH (NOLOCK) 
	WHERE JobExecutionID = @JobExecutionID

	WHILE EXISTS(SELECT null 
				FROM SEIDR.JobExecution WITH (NOLOCK)
				WHERE jobExecutionID = @JobExecutionID
				AND LU = @LU
				AND JobProfile_JobID IS NOT NULL)
	BEGIN
		SET @I = @I + 1
		RAISERROR('@LU unchanged at Loop %d', 0, 0, @i) WITH NOWAIT
		WAITFOR DELAY '00:00:10'
		
		IF @NoWaitLimit = 0 AND @I > 6
		BEGIN
			SELECT @LU [Previous LU]
			SELECT IsWorking, InWorkQueue, JobPriority, RetryCount, ExecutionStatus,  LU
			FROM SEIDR.JobExecution WITH (NOLOCK)
			WHERE JobExecutionID = @JobExecutionID 
			
			RETURN
		END
	END
	SELECT @LU [Previous LU], * 
	FROM REFERENCE.vw_JobExecution 
	WHERE JobExecutionID = @JobExecutionID
END