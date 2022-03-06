CREATE PROCEDURE [SEIDR].[usp_JobProfile_CloneSteps]
	@JobProfileID int,
	@TargetJobProfileID int OUTPUT
AS
	SET XACT_ABORT ON
	IF @TargetJobProfileID is null
	BEGIN
		INSERT INTO SEIDR.JobProfile(Description, ScheduleFromDate, RequiredThreadID)
		SELECT 'CLONE: ' + j.Description, CONVERT(date, GETDATE()), RequiredThreadID
		FROM SEIDR.JobProfile j
		WHERE JobProfileID = @JobProfileID

		SELECT @TargetJobProfileID = SCOPE_IDENTITY()		
	END
	ELSE IF EXISTS(SELECT null FROM SEIDR.JobProfile_Job WHERE JobProfileID = @TargetJobProfileID AND Active = 1)
	BEGIN
		RAISERROR('@TargetJobProfileID already has active steps configured.', 16, 1)
		RETURN
	END

	INSERT INTO SEIDR.JobProfile_Job(JobProfileID, JobID, StepNumber, CanRetry, RetryDelay, 
		TriggerExecutionNameSpace, TriggerExecutionStatusCode, 
		FailureNotificationMail, RequiredThreadID, SequenceScheduleID)
	SELECT @TargetJobProfileID, JobID, StepNumber, CanRetry, RetryDelay, 
		TriggerExecutionNameSpace, TriggerExecutionStatusCode,
		FailureNotificationMail, RequiredThreadID, SequenceScheduleID
	FROM SEIDR.JobProfile_Job
	WHERE JobProfileID = @JobProfileID
	AND Active = 1


RETURN 0
