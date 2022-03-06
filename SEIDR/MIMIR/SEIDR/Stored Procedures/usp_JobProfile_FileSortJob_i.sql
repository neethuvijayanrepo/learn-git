CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileSortJob_i]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	@FromDate date = null,	
	
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
	DECLARE @CanRetry bit = 0,
			@RetryLimit int = 0,
			@RetryDelay int = null	

	
	DECLARE @JobID int , @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileSortJob' 
	AND JobNameSpace = 'FileSystem'
	
	exec SEIDR.usp_JobProfile_Job_iu
		@JobProfileID = @JobProfileID,
		@StepNumber = @StepNumber out,
		@Description = @Description,
		@TriggerExecutionStatus = @TriggerExecutionStatus,
		@TriggerExecutionNameSpace = @TriggerExecutionNameSpace,
		@CanRetry = @CanRetry,
		@RetryLimit = @RetryLimit,
		@RetryDelay = @RetryDelay,
		@JobID = @JobID,
		@ThreadID=@ThreadID,
		@JobProfile_JobID = @JobProfile_JobID out,
		@FailureNotificationMail = @FailureNotificationMail,
		@SequenceSchedule = @SequenceSchedule,
		@TriggerBranch = @TriggerBranch,
		@Branch = @Branch,
		@RetryCountBeforeFailureNotification = @RetryCountBeforeFailureNotification
	IF @@ERROR <> 0 
		RETURN

	IF NOT EXISTS(SELECT null FROM SEIDR.DocMetaData WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1)
	BEGIN
		RAISERROR('MetaData will be pulled from file the first time that it attempts to run.', 0, 0)
		RETURN
	END
	
	DECLARE @HasHeader bit = 0, 
			@HasTrailer bit = 0, 
			@SkipLines int = 0,
			@Delimiter char(1) = '|',
			@TextQualifier varchar(5) = '"'
	SELECT @HasHeader = HasHeader,
			@HasTrailer = HasTrailer,
			@SkipLines = SkipLines,
			--@FromDate = ThroughDate + 1,
			@Delimiter = Delimiter,
			@TextQualifier = TextQualifier
	FROM SEIDR.DocMetaData
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND Active = 1
	AND IsCurrent = 1

	IF @FromDate is null
	BEGIN
		SET @Fromdate = CONVERT(date, GETDATE())
	END

	DECLARE @msg varchar(5000) = '
	SELECT * 
	FROM [SEIDR].[vw_DocMetaData] WITH (NOLOCK)
	WHERE JobProfile_JobID = %d
	AND IsCurrent = 1

	DECLARE @ColumnMetaData SEIDR.udt_DocMetaDataColumn
	INSERT INTO @ColumnMetaData(ColumnName, Position, Max_Length, SortASC, SortPriority)
	SELECT cm.ColumnName, cm.Position, cm.Max_Length, 
				CASE cm.ColumnName
					WHEN ''@@@@'' THEN 0 --DESC
				ELSE 1 --DEFAULT ASC
				END
				as SortASC, 
				CASE cm.ColumnName
					WHEN ''@@@@'' THEN 1 --List columns to be sorted...
				END
				as SortPriority
	FROM SEIDR.DocMetaDataColumn cm
	JOIN SEIDR.DocMetaData md
		ON cm.MetaDataID = md.MetaDataID
	WHERE md.IsCurrent = 1 AND md.Active = 1
	AND md.JobProfile_JobID = %d
		
	SELECT * FROM @ColumnMetaData 
	/*
	--Populate ColumnMeta Data first, if intending to specify. 
	--Specify Sort order (ASC) and SortPriority (Order) in this variable as well. Sort Priority only needs to be populated for columns used to sort
	exec SEIDR.usp_DocMetaData_i
		@JobProfile_JobID, 
		@Delimiter = ''%s'',
		@HasHeader = ' + CASE @HasHeader WHEN 1 then '1' else '0' end + ',
		@HasTrailer = ' + CASE @HasTrailer WHEN 1 then '1' else '0' end +',
		@SkipLines = %d,
		@TextQualifier = ''%s'',
		@ProcessingDate = ''' + CONVERT(varchar(30), @FromDate) + ''',
		@ColumnMetaData = @columnMetaData
		*/'	
	RAISERROR(@msg, 0, 0, @JobProfile_JobID, @JobProfile_JobID, @Delimiter, @SkipLines, @TextQualifier)
			
END