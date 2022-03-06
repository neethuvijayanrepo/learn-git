
CREATE PROCEDURE [SEIDR].[usp_JobProfile_NordisSplitJob_iu]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@ThreadID int = null,
	
	@FailureNotificationMail varchar(500) = null,
	@SequenceSchedule varchar(300) = null,
	@TriggerBranch varchar(30) = null,
	@Branch varchar(30) = null,
	@RetryCountBeforeFailureNotification smallint = null
AS
BEGIN	
	SET XACT_ABORT ON
	IF @Description IS null
		SELECT @Description = 'Nordis File Split'	
	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'NordisSplitJob' 
	AND JobNameSpace = 'FileSystem'
	

	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = 0,
		@RetryLimit = 0,
		@RetryDelay = null,
		@JobID = @JobID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@ThreadID = @ThreadID,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0
		RETURN
		
	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID	
END