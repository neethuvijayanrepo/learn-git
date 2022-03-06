CREATE PROCEDURE SEIDR.usp_JobProfile_Job_MoveBranch
	@JobProfile_JobID int,
	@NewBranch varchar(30) = null,
	@NewTriggerBranch varchar(30) = null,
	@ModifyTriggerBranch bit = 0
AS
BEGIN
	IF @NewTriggerBranch is not null
		SET @ModifyTriggerBranch = 1
	UPDATE SEIDR.JobProfile_Job
	SET Branch = COALESCE(@NewBranch, Branch),
		TriggerBranch = IIF(@ModifyTriggerBranch = 1, @NewTriggerBranch, TriggerBranch)
	WHERE JobProfile_JobID = @JobProfile_JobID

	DECLARE @JobProfileID int
	DECLARE @NoteText varchar(2000)
	SELECT @JobProfileID = JobProfileID,
		@NoteText = 'Move Branch for Step # ' 
			+ CONVERT(varchar(12), StepNumber) 
			+ ' "' + jpj.Description + '" - '
	FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	WHERE JobProfile_JobID = @JobProfile_JobID

	IF @NewBranch is not null
	BEGIN
		SET @NoteText += ' Branch: "' + @NewBranch + '"'
		IF @ModifyTriggerBranch = 1
			SET @NoteText += ';'
	END
	IF @ModifyTriggerBranch = 1
		SET @NoteText += ' TriggerBranch : ' + ISNULL('"' + @NewTriggerBranch + '"', '(NULL)')
	 
	exec SEIDR.usp_JobProfileNote_i @JobProfileID, @NoteText, 1, null
END	
	