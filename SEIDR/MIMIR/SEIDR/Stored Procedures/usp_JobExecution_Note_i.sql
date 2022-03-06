
CREATE PROCEDURE [SEIDR].[usp_JobExecution_Note_i]
	@JobExecutionID bigint,
	@NoteText varchar(2000),
	@StepNumber smallint = null,
	@Technical bit = 0
AS	
	SET @NoteText = UTIL.ufn_CleanField(@NoteText)

	INSERT INTO SEIDR.JobExecution_Note
		(JobExecutionID, StepNumber, JobProfile_JobID, NoteText, Technical, Auto)
	SELECT je.JobExecutionID, je.StepNumber, je.JobProfile_JobID, @NoteText, @Technical, 0
	FROM SEIDR.vw_JobExecution je
	WHERE JobExecutionID = @JobExecutionID
	AND (StepNumber = @StepNumber or @StepNumber is null)

	IF @@ROWCOUNT = 0
	BEGIN
		IF @StepNumber is not null
		BEGIN
			print 'Using History to insert...'
			INSERT INTO SEIDR.JobExecution_Note(JobExecutionID, StepNumber, JobProfile_JobID, NoteText, Technical, Auto)
			SELECT h.JobExecutionID, h.StepNumber, h.JobProfile_JobID, @NoteText, @Technical, 0
			FROM SEIDR.vw_JobExecutionHistory h
			WHERE JobExecutionID = @JobExecutionID
			AND StepNumber = @StepNumber 
			AND IsLatestForExecutionStep = 1

			IF @@ROWCOUNT > 0
				RETURN
		END
		
		print 'Not in view - Inserting with no joins'
		INSERT INTO SEIDR.JobExecution_Note(JobExecutionID, StepNumber, NoteText, Technical, Auto)
		SELECT je.JobExecutionID, je.StepNumber, @NoteText, @Technical, 0
		FROM SEIDR.JobExecution je
		WHERE JobExecutionID = @JobExecutionID
	END
RETURN 0
