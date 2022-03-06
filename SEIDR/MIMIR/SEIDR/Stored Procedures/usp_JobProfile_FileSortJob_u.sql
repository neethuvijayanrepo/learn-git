
CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileSortJob_u]
	@JobProfileID int,
	@StepNumber tinyint = null,
	@Description varchar(100) = null,
	@FromDate date = '1900-01-01',
	@ThroughDate date = null,
	@Delimiter char(1) = null,
	@TextQualifier varchar(5) = '"',
	@HasHeader bit = 1,
	@SkipLines int = 0,
	@HasTrailer bit = 0,
	@DuplicateHandling varchar(50) = null,
	@SortColumn1 int,
	@SortColumn1_ASC bit = 1,
	@SortColumn2 int = null,
	@SortColumn2_ASC bit = 1,
	@SortColumn3 int = null,
	@SortColumn3_ASC bit = 1,
	@CanRetry bit = null,
	@RetryLimit int = 50,
	@RetryDelay int = 10,
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
	IF @Description is null
		SET @Description = 'File Sort'
	
	DECLARE @JobID int, @JobProfile_JobID int
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
	
	DECLARE @DocMetaDataID int
	
	IF EXISTS(SELECT null FROM SEIDR.DocMetaData WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1 AND FromDate < @FromDate AND ThroughDate is null)
	BEGIN
		RAISERROR('Set ThroughDate on older meta data', 0, 0)
	
		UPDATE SEIDR.DocMetaData
		SET ThroughDate = DATEADD(day, -1, @FromDate)
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND Active = 1 
		AND FromDate < @FromDate 
		AND ThroughDate is null
	END

	IF EXISTS(SELECT null FROM SEIDR.DocMetaData WHERE Jobprofile_JobID = @JobProfile_JobID AND Active =1 AND FromDate = @FromDate)
	BEGIN
		IF @ThroughDate is null
		BEGIN
			IF EXISTS(SELECT null FROM SEIDR.DocMetaData 
						WHERE JobProfile_jobID = @JobProfile_JobID 
						AND Active = 1 
						AND FromDate > @FromDate)
			BEGIN
				SELECT * 
				FROM SEIDR.DocMetaData 
				WHERE JobProfile_jobID = @JobProfile_JobID 
				AND Active = 1 
				AND FromDate >= @FromDate
				RAISERROR('Overlap with other document meta data.', 16, 1)
				RETURN
			END
			--UPDATE SEIDR.DocMetaData
			--SET ThroughDate = null
			--WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
			--AND FromDate = @FromDate
		END
		ELSE -- Ensure no throughdate overlap if changing a previously populated throughDate		
			IF EXISTS(SELECT null
						FROM SEIDR.DocMetaData
						WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
						AND @ThroughDate BETWEEN FromDate AND ISNULL(ThroughDate, '2900-12-31')
						)
			BEGIN
				SELECT *
				FROM SEIDR.DocMetaData
				WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
				AND @ThroughDate BETWEEN FromDate AND ISNULL(ThroughDate, '2900-12-31')
				RAISERROR('Overlap with other documenta meta data.', 16, 2)
				RETURN
			END
			

		UPDATE SEIDR.DocMetaData
		SET ThroughDate = @ThroughDate,
			Delimiter = @Delimiter,
			TextQualifier = @TextQualifier,
			HasHeader = @HasHeader, 
			SkipLines = @SkipLines,
			HasTrailer = @HasTrailer,
			DuplicateHandling = @DuplicateHandling,			
			--===VARIABLE UPDATE===
			--Grab Identity
			@DocMetaDataID = MetaDataID
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND Active = 1 
		AND FromDate = @FromDate
		--AND (@ThroughDate is null and ThroughDate is null or @ThroughDate = ThroughDate)	
		
		UPDATE SEIDR.DocMetaDataColumn
		SET SortPriority = 1, 
			SortAsc = @SortColumn1_ASC
		WHERE MetaDataID = @DocMetaDataID 
		AND Position = @Sortcolumn1
		
		IF @SortColumn2 is not null
			UPDATE SEIDR.DocMetaDataColumn
			SET SortPriority = 2, 
				SortAsc = @SortColumn2_ASC
			WHERE MetaDataID = @DocMetaDataID 
			AND Position = @Sortcolumn2
		
		IF @SortColumn3 is not null
			UPDATE SEIDR.DocMetaDataColumn
			SET SortPriority = 3, 
				SortAsc = @SortColumn3_ASC
			WHERE MetaDataID = @DocMetaDataID 
			AND Position = @Sortcolumn3
	END	
	ELSE IF NOT EXISTS(SELECT null FROM SEIDR.DocMetaData WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1 AND FromDate = @FromDate)
	BEGIN
		IF EXISTS(SELECT null 
					FROM SEIDR.DocMetaData 
					WHERE JobProfile_JobID = @JobProfile_JobID 
					AND Active = 1 
					AND 
						(
							@FromDate BETWEEN FromDate AND ISNULL(ThroughDate, '2900-12-31')
							OR @ThroughDate BETWEEN FromDate AND ISNULL(ThroughDate, '2900-12-31')
						)
					)
		BEGIN
			RAISERROR('Overlaps with other MetaData.', 16, 1)
			RETURN
		END
		DECLARE @msg varchar(5000) = '
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
		RAISERROR(@msg, 0, 0, @JobProfile_JobID, @Delimiter, @SkipLines, @TextQualifier)
		
		SELECT * 
		FROM SEIDR.JobProfile_Job jp
		WHERE JobProfile_JobID = @JobProfile_JobID
		
		RETURN
	END

	SELECT * 
	FROM SEIDR.JobProfile_Job jp
	wHERE JobProfile_JobID = @JobProfile_JobID
	
	SELECT * 
	FROM SEIDR.DocMetaData
	WHERE JobProfile_JobID = @JobProfile_JobID AND Active = 1
	AND FromDate = @FromDate

	SELECT * 
	FROM SEIDR.DocMetaDataColumn
	WHERE MetaDataID = @DocMetaDataID
END