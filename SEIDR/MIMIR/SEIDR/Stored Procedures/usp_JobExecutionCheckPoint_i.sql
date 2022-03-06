CREATE PROCEDURE [SEIDR].[usp_JobExecutionCheckPoint_i]
	@JobExecutionID bigint,
	@JobProfile_JobID int,
	@JobID int,
	@CheckPointKey varchar(10),
	@CheckPointNumber int,
	@Message varchar(300),
	@ThreadID int,
	@CheckPointDuration int,
	@CheckPointID int = null output
AS
	INSERT INTO SEIDR.JobExecutionCheckPoint(JobExecutionID, JobProfile_JobID, JobID,
		CheckPointKey, CheckPointMessage, CheckPointNumber, ThreadID, CheckPointTime)
	VALUES(@JobExecutionID, @JobProfile_JobID, @JobID,
		@CheckPointKey, @Message, @CheckPointNumber, @ThreadID, @CheckPointDuration)
	SELECT @CheckPointID = SCOPE_IDENTITY()
RETURN 0
