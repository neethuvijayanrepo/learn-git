CREATE PROCEDURE [SEIDR].[usp_JobExecution_Resolve]
	@JobExecutionID bigint,
	@Resolution varchar(10) = NULL
AS
BEGIN
	IF @Resolution is null
	BEGIN
		IF EXISTS(SELECT null 
					FROM SEIDR.JobExecution je WITH (NOLOCK)
					JOIN SEIDR.JobExecution je2  WITH (NOLOCK)
						ON je.JobProfileID = je2.JobProfileID
						AND je.ProcessingDate = je2.ProcessingDate
					WHERE je.JobExecutionID = @JobExecutionID
					AND je.JobExecutionID <> je2.JobExecutionID)
		BEGIN
			SET @Resolution = 'DUPLICATE'
		END
		ELSE IF 7 < (SELECT DATEDIFF(day, LU, GETDATE())
						FROM SEIDR.JobExecution  WITH (NOLOCK)
						WHERE JobExecutionID = @JobExecutionID)
		BEGIN
			SET @Resolution = 'NotNeeded' --If setting DD over a week after it's done anything, assume not needed rather than manual.
		END
		ELSE
			SET @Resolution = 'MANUAL'
	END
	ELSE IF @Resolution NOT IN ('UNRESOLVED', 'MANUAL', 'DUPLICATE', 'NOTNEEDED')
	BEGIN
		RAISERROR('Invalid Resolution: %s. Valid resolutions: "MANUAL", "DUPLICATE", "NotNeeded", "UNRESOLVED"', 16, 1, @Resolution)
		RETURN
	END	
	IF @Resolution = 'UNRESOLVED'
	BEGIN
		UPDATE SEIDR.JobExecution
		SET DD = null, 
			Duplicate = 0,
			Manual = 0,
			NotNeeded = 0
		WHERE JobExecutionID = @JobExecutionID
	END
	ELSE
	BEGIN		
		DECLARE @Manual bit = 0, @Duplicate bit = 0, @NotNeeded bit = 0
		IF @Resolution = 'MANUAL'
			SET @Manual = 1
		ELSE IF @Resolution = 'DUPLICATE'
			SET @Duplicate = 1
		ELSE 
			SET @NotNeeded = 1

		UPDATE SEIDR.JobExecution
		SET DD = COALESCE(DD, GETDATE()),			
			[Manual] = @Manual,
			Duplicate = @Duplicate,
			NotNeeded = @NotNeeded
		WHERE JobExecutionID = @JobExecutionID
	END

	DECLARE @nt varchar(2000) = 'RESOLUTION: ' + @Resolution
	exec SEIDR.usp_JobExecution_Note_i 
		@JobExecutionID, 
		@NoteText = @nt, 
		@Technical = 0

	SELECT JobExecutionID, Active, Manual, Duplicate, NotNeeded
	FROM SEIDR.JobExecution
	WHERE JobExecutionID = @JobExecutionID
END