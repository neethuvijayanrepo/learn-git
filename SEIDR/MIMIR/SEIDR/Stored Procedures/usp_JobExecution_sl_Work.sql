CREATE PROCEDURE [SEIDR].[usp_JobExecution_sl_Work]
	@ThreadID int,
	@ThreadCount int,
	@BatchSize int = 5
as
BEGIN
	SET XACT_ABORT ON
	BEGIN TRAN

	DECLARE @MatchID int = @ThreadID
	IF @ThreadID = @ThreadCount  --Mod is 0 based, ThreadID 1 based
		SET @MatchID = 0

	SELECT TOP (@BatchSize) *
	INTO #jobs
	FROM SEIDR.vw_JobExecution
	WHERE CanQueue = 1
	AND InSequence = 1
	AND Loaded = 1
	AND (
		RequiredThreadID is null 
		or (RequiredThreadID % @ThreadCount) = @MatchID
		)
	ORDER BY RequiredThreadID desc, WorkPriority desc, ProcessingDate asc, ExecutionPriority DESC

	SET @BatchSize = @@ROWCOUNT	


	UPDATE je
	SET InWorkQueue = 1
	FROM SEIDR.JobExecution je
	JOIN #jobs j
		ON je.JobExecutionID = j.JobExecutionID
	WHERE InWorkQueue = 0
	
	IF @@ROWCOUNT <> @BatchSize
	BEGIN		
		ROLLBACK
		RETURN 50
	END
	
	COMMIT

	SELECT * 
	FROM #jobs

/*
--Deadlocks, leave out for now since it shouldn't really be a big deal
	INSERT INTO SEIDR.Log
	(
		ThreadID, MessageType, 
		LogMessage, 
		JobProfileID, JobProfile_JobID, JobExecutionID, 
		ThreadName, LogTime
	)
	SELECT 
		@ThreadID, 'QUEUE', 
		'Pulling into WorkQueue' + CASE WHEN RequiredThreadID is not null then ' [REQUIRED THREAD]' else '' end, 
		JobProfileID, JobProfile_JobID, JobExecutionID, 
		'N/A', GETDATE()
	FROM #Jobs
*/
	
END
