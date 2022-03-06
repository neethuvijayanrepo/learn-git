CREATE PROCEDURE [SEIDR].[usp_FileSizeCheckJob_CheckFile]
	@JobExecutionID bigint,
	@JobProfile_JobID int,
	@ProcessingDate date,
	@FilePath varchar(260),
	@FileSize bigint,
	@AllowContinue bit output,
	@Deviation bigint output,
	@Message varchar(1000) output,
	@Empty bit output,
	@LargeFile bit output
AS
BEGIN	
	SELECT @Empty = 0, @LargeFile = 0
	DECLARE @HistoryID int, @SizeCheckJobID int,
		@SizeCompare int, @PathCompare varchar(260), @DateCompare date

	DECLARE @DaysBack tinyint, @Multiplier float, @EmptyFileSize int, @CheckLargeFiles bit, @PercentThreshold float

	DECLARE @DayOfWeek tinyint = DATEPART(dw, @ProcessingDate)

	SELECT @SizeCheckJobID = j.FileSizeCheckJobID,
		@HistoryID = h.FileSizeHistoryID,
		@AllowContinue = ISNULL(h.AllowContinue, 0), --Allow manual override, so long as the FilePath/Size/Date information matches.
		@SizeCompare = h.FileSize,
		@PathCompare = h.FilePath,
		@DateCompare = h.FileDate,
		@DaysBack = j.DaysBack,
		@Multiplier = j.StandardDeviationMultiplier,
		@PercentThreshold = j.PercentThreshold,
		@CheckLargeFiles = j.CheckLargeFiles,
		@EmptyFileSize = j.EmptyFileSize
	FROM SEIDR.FileSizeCheckJob j
	LEFT JOIN SEIDR.FileSizeHistory h
		ON j.FileSizeCheckJobID = h.FileSizeCheckJobID
		AND h.JobExecutionID = @JobExecutionID
		AND h.Active = 1
	WHERE j.JobProfile_JobID = @JobProfile_JobID
	
	IF @HistoryID IS NOT NULL
	BEGIN
		IF @SizeCompare <> @FileSize 
		OR @PathCompare <> @FilePath
		OR @DateCompare <> @ProcessingDate
		BEGIN
			UPDATE SEIDR.FileSizeHistory
			SET DD = GETDATE()
			WHERE FileSizeHistoryID = @HistoryID

			SELECT @HistoryID = null,
				@AllowContinue = 0
		END
		ELSE
		BEGIN
			SET @message = 'Use AllowContinue from Database (Use SEIDR.usp_FileSizeHistory_u to override)'
			RETURN 1 --Just use the AllowContinue from table.
		END
	END

	DECLARE @ignore bit = 0
	IF @DayOfWeek = 1
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileSunday, IgnoreSunday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE IF @DayofWeek = 2
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileMonday, IgnoreMonday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE IF @DayofWeek = 3
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileTuesday, IgnoreTuesday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE IF @DayofWeek = 4
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileWednesday, IgnoreWednesday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE IF @DayofWeek = 5
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileThursday, IgnoreThursday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE IF @DayofWeek = 6
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileFriday, IgnoreFriday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END
	ELSE
	BEGIN
		SELECT @ignore = IIF(@FileSize <= @EmptyFileSize, IgnoreEmptyFileSaturday, IgnoreSaturday)
		FROM SEIDR.FileSizeCheckJob WITH (NOLOCK)
		WHERE JobProfile_JobID = @JobProfile_JobID
	END

	IF @Ignore = 1
	BEGIN
		SET @Message = 'Ignore Size Check due to day of week'
		SET @AllowContinue = 1
		RETURN 10 --ignore
	END

	IF @FileSize <= @EmptyFileSize
	BEGIN
		SET @AllowContinue = 0
		SET @Message = 'Empty File - Not ignoring Empty file for day'

		INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID, FilePath, FileSize,
			DayOfWeek, FileDate, JobExecutionID, AllowContinue)
		VALUES(@SizeCheckJobID, @FilePath, @FileSize,
			@dayOfWeek, @ProcessingDate, @JobExecutionID, @AllowContinue)	

		SET @Empty = 1
		RETURN
	END


	DECLARE @MinDate date = DATEADD(day, -@DaysBack, @ProcessingDate)	

	IF NOT EXISTS(SELECT null 
					FROM SEIDR.FileSizeHistory h					
					WHERE FileSizeCheckJobID = @SizeCheckJobID
					AND [DayOfWeek] = @DayOfWeek
					AND FileDate BETWEEN @MinDate AND @ProcessingDate
					AND Active = 1
					AND FileSize > @EmptyFileSize
					AND AllowContinue = 1 --Match conditions used to get the average.
					)
	BEGIN
		SET @AllowContinue = 1
		SET @message = 'No Data to compare against. Auto Pass Validation'

		INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID, FilePath, FileSize,
			DayOfWeek, FileDate, JobExecutionID, AllowContinue)
		VALUES(@SizeCheckJobID, @FilePath, @FileSize,
			@dayOfWeek, @ProcessingDate, @JobExecutionID, @AllowContinue)	
		
		RETURN 11 --No Data To Compare
	END

	DECLARE @AverageSize bigint, @HistoryDeviation float
	SELECT @HistoryDeviation = @Multiplier * STDEV(CASE WHEN PassValidation = 1 then FileSize end), 
			-- NOTE: Even though files can be forced to pass validation, 
			-- they still shouldn't really be used for the standard deviation afterward until they pass automatically, to avoid too much skewing
			@AverageSize = AVG(FileSize)
	FROM SEIDR.FileSizeHistory WITH (NOLOCK)
	WHERE FileSizeCheckJobID = @SizeCheckJobID
	AND FileDate BETWEEN @mindate and @ProcessingDate
	AND Active = 1
	AND [DayOfWeek] = @DayOfWeek
	AND AllowContinue = 1
	AND FileSize > @EmptyFileSize --Empty files should not contribute to average.
		

	IF @FileSize > @AverageSize
	AND @CheckLargeFiles = 0
	BEGIN
		SET @AllowContinue = 1
		SET @message = 'Large File - Ignoring Size check'
		INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID, FilePath, FileSize,
			DayOfWeek, FileDate, JobExecutionID, AllowContinue, AverageAtEvaluation)
		VALUES(@SizeCheckJobID, @FilePath, @FileSize,
			@dayOfWeek, @ProcessingDate, @JobExecutionID, @AllowContinue, @AverageSize)	
		RETURN 12 -- Large File, skip check
	END
	IF @fileSize = @AverageSize
	BEGIN
		SET @AllowContinue = 1
		SET @Message = 'File Matches Average'
		INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID, FilePath, FileSize,
			DayOfWeek, FileDate, JobExecutionID, AllowContinue, AverageAtEvaluation)
		VALUES(@SizeCheckJobID, @FilePath, @FileSize,
			@dayOfWeek, @ProcessingDate, @JobExecutionID, @AllowContinue, @AverageSize)	
		RETURN 13 -- Average matched.
	END
	
	IF @historyDeviation is null --Should pretty much only happen if we only have one record within the daysBack range.
	BEGIN
		SET @Message = 'Insufficient History - switching to default Percentage check (10)'
		SET @PercentThreshold = 10
	END

	IF @PercentThreshold is not null
	BEGIN
		/*
		@Percent Threshold is a float value between 5 and 99.999...
		Multiply by .01 to treat as percent, (e.g., 5 -> .05), and multiply against the average size. 
		
		E.g., FileSize 200 with threshold 5 -> .01 * 5 * 200 -> 10
		Require 10 >= ABS(@FileSize - 200) . 
		So size would need to be between 190 and 210 
			* 190 - 200 => ABS(-10) -> 10: 10 >= 10 (Pass)
			* 210 - 200 => ABS(10) -> 10: 10 >= 10 (Pass)
			* 209 - 200 => ABS(9) -> 9 : 10 >= 9 (Pass)
			* 189 - 200 => ABS(-11) -> 11: 10 >= 11 (Fail)
			* 215 - 200 => ABS(15) -> 15: 10 >= 15 (fail)
		*/
		SET @HistoryDeviation = .01 * @PercentThreshold * @AverageSize
	END

	SET @Deviation =  ABS(@FileSize - @AverageSize)	

	IF @HistoryDeviation < @Deviation -- Actual deviation exceeds allowed (history) deviation
	BEGIN
		SET @AllowContinue = 0
		IF @FileSize > @AverageSize
			SET @LargeFile = 1
	END
	ELSE
		SET @AllowContinue = 1

	
	INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID, FilePath, FileSize,
		DayOfWeek, FileDate, JobExecutionID, AllowContinue, 
		AverageAtEvaluation, DeviationAtEvaluation)
	VALUES(@SizeCheckJobID, @FilePath, @FileSize,
		@dayOfWeek, @ProcessingDate, @JobExecutionID, @AllowContinue, 
		@AverageSize, @HistoryDeviation)
	RETURN 0
END
