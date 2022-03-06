CREATE PROCEDURE [SEIDR].[usp_Log_i]
	@ThreadID smallint,
	@ThreadType varchar(50),
	@ThreadName varchar(100),
	@LogMessage varchar(2000),
	@MessageType varchar(5),
	@JobProfileID int = null,
	@JobExecutionID bigint = null,
	@JobProfile_JobID int = null                
AS
	INSERT INTO SEIDR.Log(ThreadID, ThreadType, ThreadName,
		LogMessage, MessageType,
		JobProfileID, JobExecutionID, JobProfile_JobID)
	VALUES(@ThreadID, @ThreadType, @ThreadName,
		@LogMessage, @MessageType,
		@JobProfileID, @JobExecutionID, @JobProfile_JobID)

RETURN 0
