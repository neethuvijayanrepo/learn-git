CREATE PROCEDURE SEIDR.usp_JobExecution_StartWork
	@JobExecutionID bigint
as
BEGIN
	UPDATE SEIDR.JobExecution
	SET IsWorking = 1,
		InWorkQueue = 0,
		PrioritizeNow = 0 --ToDo: start recording the ThreadID doing the work here? Then add to the trigger for archiving to status history
	WHERE JobExecutionID = @JobExecutionID
	AND IsWorking = 0
	AND JobProfile_JobID IS NOT NULL -- must be mapped to show up in view. If this isn't mapped, then it's probably either in an error or complete state.

	IF @@ROWCOUNT = 1
	BEGIN
		SELECT * FROM SEIDR.vw_JobExecution WHERE JobExecutionID = @JobExecutionID
		
		IF @@ROWCOUNT = 0
		BEGIN
			UPDATE SEIDR.JobExecution
			SET IsWorking = 0 --Somehow... not actually eligible to work.
			WHERE JobExecutionID = @JobExecutionID
		END
	END
	else 
	BEGIN
		UPDATE SEIDR.JobExecution
		SET InWorkQueue = 0
		WHERE JobExecutionID = @JobExecutionID
		return 50
	END
	
	RETURN 0
END
