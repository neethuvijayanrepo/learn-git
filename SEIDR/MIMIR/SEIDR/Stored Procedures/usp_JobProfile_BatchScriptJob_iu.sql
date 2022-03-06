CREATE PROCEDURE [SEIDR].[usp_JobProfile_BatchScriptJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,	
	
	--@Description varchar(100) = null,	
	@BatchScriptPath varchar(500),
	@Parameter3 varchar(300) = null,
	@Parameter4 varchar(300) = null,	
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null, 	
	@FailureNotificationMail varchar(500)= null,
	@SequenceSchedule varchar(300) = null,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
	SET XACT_ABORT ON
	
	DECLARE @Description varchar(100) 
	SET @Description = 'Batch Script: ' + UTIL.ufn_PathItem_GetName(@BatchScriptPath)

	IF @BatchScriptPath NOT LIKE '%.BAT'
	BEGIN
		RAISERROR('Invalid script. @BatchScriptPath must point to a Batch file.', 16, 1)
		RETURN
	END

	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'BatchScriptJob' 
	AND JobNameSpace = 'Scripting'
	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Could not identify @JobID', 16, 1)
		RETURN
	END
	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = 0,
		@RetryLimit = 1, --Not nullable, but we don't care about it.
		@RetryDelay = null,
		@JobID = @JobID,
		@ThreadID=@ThreadID,
		@FailureNotificationMail = @FailureNotificationMail,
		@JobProfile_JobID = @JobProfile_JobID out,
		@SequenceSchedule=@SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch, 
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN

	IF EXISTS(SELECT null FROM SEIDR.BatchScriptJob WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1)
	BEGIN
		UPDATE SEIDR.BatchScriptJob
		SET
			BatchScriptPath = @BatchScriptPath,
			Parameter3 = @Parameter3,
			Parameter4 = @Parameter4		
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND Active = 1
	END
	ELSE
	BEGIN
		INSERT INTO SEIDR.BatchScriptJob(JobProfile_JobID, BatchScriptPath, Parameter3, Parameter4)
		VALUES(@JobProfile_JobID, @BatchScriptPath, @Parameter3, @Parameter4)
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job 
	WHERE JobProfile_JobID = @JobProfile_JobID

	SELECT * 
	FROM SEIDR.BatchScriptJob
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND Active = 1

RETURN 0
