CREATE PROCEDURE SEIDR.usp_Job_Validate
	@JobList SEIDR.udt_JobMetaData readonly
as
BEGIN
	UPDATE j
	SET ThreadName = l.ThreadName,
		SingleThreaded = l.SingleThreaded,
		Description = l.Description,
		Loaded = 1,
		LastLoad = GETDATE(),
		NotificationTime = l.NotificationTime,
		ConfigurationTable = l.ConfigurationTable,
		AllowRetry = l.AllowRetry,
		DefaultRetryTime = l.DefaultRetryTime,
		NeedsFilePath =l.NeedsFilePath
	FROM SEIDR.Job j
	JOIN @JobList l
		ON j.JobName = l.JobName
		AND j.JobNameSpace = l.JobNameSpace
	
	UPDATE j
	SET Loaded = 0
	FROM SEIDR.Job j
	WHERE NOT EXISTS(SELECT null 
					FROM @JobList l 
					WHERE l.JobName = j.JobName 
					AND l.JobNameSpace = j.JobNameSpace)
	AND Loaded = 1
	
	INSERT INTO SEIDR.Job(JobName, JobNameSpace, ThreadName, SingleThreaded, Description, NotificationTime,ConfigurationTable,AllowRetry,DefaultRetryTime,NeedsFilePath)
	SELECT JobName, JobNameSpace, ThreadName, SingleThreaded, Description, NotificationTime,ConfigurationTable,AllowRetry,DefaultRetryTime,NeedsFilePath
	FROM @JobList l
	WHERE NOT EXISTS(SELECT null FROM SEIDR.[Job] WHERE JobName = l.JobName AND JobNameSpace = l.JobNameSpace)
END