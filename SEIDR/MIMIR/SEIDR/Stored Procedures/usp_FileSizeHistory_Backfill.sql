CREATE PROCEDURE [SEIDR].[usp_FileSizeHistory_Backfill]
	@FileSizeCheckJobID int,
	@BackfillStepNumber tinyint,
	@MinFileSize int = null, -- Minimum size for including to history. If no value passed, use EmptyFileSize property to limit to files larger than the empty file size.
	@DaysBack int = 180
AS
BEGIN
	DECLARE @JobProfileID int,
		@MinDate date = GETDATE() - @DaysBack
	SELECT @JobProfileID = JobProfileID, 
			@MinFileSize = ISNULL(@MinFileSize, EmptyFileSize + 1)
	FROM SEIDR.vw_FileSizeCheckJob 
	WHERE FileSizeCheckJobID = @FileSizeCheckJobID

	INSERT INTO SEIDR.FileSizeHistory(FileSizeCheckJobID,
		FilePath, FileSize, DayOfWeek, FileDate, 
		JobExecutionID, AllowContinue)
	SELECT @FileSizeCheckJobID,
		FilePath, FileSize, DATEPART(dw, ProcessingDate), ProcessingDate,
		JobExecutionID, 1
	FROM SEIDR.JobExecution_ExecutionStatus je		
	WHERE StepNumber = @BackFillStepNumber		
	AND FileSize >= @MinFileSize
	AND ProcessingDate >= @MinDate
	AND IsLatestForExecutionStep = 1
	AND JobExecutionID = ANY(SELECT JobExecutionID 
								FROM SEIDR.JobExecution WITH (NOLOCK) 
								WHERE JobProfileID = @JobProfileID 
								AND Active = 1)		
	DECLARE @RC int = @@ROWCOUNT
	RAISERROR('BackFill inserted %d File Size records.', 0, 0, @RC)
	RETURN 0
END