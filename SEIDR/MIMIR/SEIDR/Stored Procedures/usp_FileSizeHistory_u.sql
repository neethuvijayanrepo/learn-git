CREATE PROCEDURE [SEIDR].[usp_FileSizeHistory_u]
	@JobExecutionID bigint,
	@UpdateAllSizeChecksForExecution bit = 1,
	@FileSizeCheckJobID int = null,
	@ForceAllowContinue bit = null,
	@Deactivate bit = 0

AS
	--Called manually, not through service
	IF @Deactivate = 0 AND @ForceAllowContinue is null
	BEGIN
		RAISERROR('Must specify a change.', 16, 1)
	END
	IF @FileSizeCheckJobID IS NULL
	AND @UpdateAllSizeChecksForExecution = 0
	BEGIN
		SELECT TOP 1 @FileSizeCheckJobID = j.FileSizeCheckJobID
		FROM SEIDR.Log l WITH (NOLOCK)
		JOIN SEIDR.FileSizeCheckJob j WITH (NOLOCK)
			ON l.JobProfile_JobID = j.JobProfile_JobID
		WHERE l.JobExecutionID = @JobExecutionID
		ORDER BY l.Id desc
	END
	ELSE IF @UpdateAllSizeChecksForExecution = 1
		SET @FileSizeCheckJobID = null

	IF @FileSizeCheckJobID IS NULL
	BEGIN
		UPDATE SEIDR.FileSizeHistory WITH (ROWLOCK)
		SET DD = CASE WHEN @Deactivate = 1 THEN GETDATE() END,
			AllowContinue = ISNULL(@ForceAllowContinue, AllowContinue)
		WHERE JobExecutionID = @JobExecutionID
		AND Active = 1
	END
	ELSE
	BEGIN		
		UPDATE SEIDR.FileSizeHistory WITH (ROWLOCK)
		SET DD = CASE WHEN @Deactivate = 1 THEN GETDATE() END,
			AllowContinue = ISNULL(@ForceAllowContinue, AllowContinue)
		WHERE FileSizeCheckJobID = @FileSizeCheckJobID
		AND JobExecutionID = @JobExecutionID
		AND Active = 1
		
	END
RETURN 0
