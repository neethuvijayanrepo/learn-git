CREATE PROCEDURE [TEST].[usp_CheckJob]
	@JobList SEIDR.udt_JobMetaData readonly
AS
	
	INSERT INTO SEIDR.Job(JobName, JobNameSpace, ThreadName, SingleThreaded, Description, NotificationTime,ConfigurationTable,AllowRetry,DefaultRetryTime,NeedsFilePath)
	SELECT JobName, JobNameSpace, ThreadName, SingleThreaded, Description, NotificationTime,ConfigurationTable,AllowRetry,DefaultRetryTime,NeedsFilePath
	FROM @JobList l
	WHERE NOT EXISTS(SELECT null FROM SEIDR.[Job] WHERE JobName = l.JobName AND JobNameSpace = l.JobNameSpace)
RETURN 0
