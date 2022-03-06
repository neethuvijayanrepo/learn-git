CREATE PROCEDURE [CONFIG].[usp_JobExecution_SetStopAfterStepNumber_Range]
	@JobProfileID int,
	@StopAfterStepNumber tinyint,
	@ProcessingDateFrom datetime = null,
	@ProcessingDateThrough datetime = null,
	@UserName varchar(130) = null,
	@FilterStepNumber tinyint = null --If not null, only update JobExecutions currently using this StopAfterStepNumber
AS
BEGIN
	IF @ProcessingDateFrom is null
	BEGIN
		SET @ProcessingDateFrom = 0
		RAISERROR('Auto Setting @ProcessingDateFrom far in the past', 0,0)
	END
	ELSE
		SET @ProcessingDateFrom = CONVERT(Date, @ProcessingDateFrom)
	IF @UserName is null
		SET @UserName = SUSER_NAME()

	IF @ProcessingDateThrough is null
		SELECT @ProcessingDateThrough = MAX(ProcessingDate)
		FROM SEIDR.JobExecution je WITH (NOLOCK)
		WHERE Active = 1 
		AND JobProfileID = @JobProfileID
		AND (@StopAfterStepNumber is null AND StopAfterStepNumber is not null
			OR @StopAfterStepNumber is not null and (StopAfterStepNumber <> @StopAfterStepNumber OR StopAfterStepNumber is null)
			)
		AND (@FilterStepNumber is null or StopAfterStepNumber = @FilterStepNumber)
		AND JobProfile_JobID IS NOT NULL
		AND NOT EXISTS(SELECT null FROM SEIDR.ExecutionStatus s WITH (NOLOCK) WHERE s.ExecutionStatus = je.ExecutionStatus AND IsComplete = 1)
	
	print '@ProcessingDateFrom:' + FORMAT(@ProcessingDateFrom, 'yyyy-MM-dd')
	print '@ProcessingDateThrough:' + ISNULL(FORMAT(@ProcessingDateThrough, 'yyyy-MM-dd'),'')

	IF @ProcessingDateFrom > @ProcessingDateThrough OR @ProcessingDateThrough is null
	BEGIN
		RAISERROR('Invalid @ProcessingDateFrom/Through combination', 16, 1)
		RETURN
	END

	DECLARE @NoteText varchar(2000) = 'Bulk update StopAfterStepNumber to ' + COALESCE(CONVERT(varchar, @StopAfterStepNumber), '(NULL)')

	DECLARE @NoteInfo table(JobExecutionID bigint, StepNumber smallint, JobProfile_JobID int)

	UPDATE SEIDR.JobExecution
	SET StopAfterStepNumber = @StopAfterStepNumber
	OUTPUT INSERTED.JobExecutionID, INSERTED.StepNumber, INSERTED.JobProfile_JobID 
	INTO @NoteInfo(JobExecutionID, StepNumber, JobProfile_JobID)	
	WHERE Active = 1
	AND JobProfileID = @JobProfileID
	AND ProcessingDate BETWEEN @ProcessingDateFrom AND @ProcessingDateThrough
	AND JobProfile_JobID IS NOT NULL
	AND (@FilterStepNumber is null or StopAfterStepNumber = @FilterStepNumber)

	INSERT INTO SEIDR.JobExecution_Note(JobExecutionID, NoteText, Technical, UserName, StepNumber, JobProfile_JobID, Auto)
	SELECT INSERTED.JobExecutionID, @NoteText, 1, @userName, INSERTED.StepNumber, INSERTED.JobProfile_JobID, 1
	FROM @NoteInfo INSERTED

	RETURN 0
END