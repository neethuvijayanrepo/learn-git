CREATE FUNCTION [UTIL].[ufn_JobExecutionID_GetNext_IdentityRange] (@Volume int)
RETURNS @Data TABLE(RowID int identity(1, 1), JobExecutionID bigint primary key)
AS
BEGIN
	-- DECLARE @Volume int = 3
	DECLARE  @GapStart bigint
	--SELECT * 
	--FROM UTIL.vw_JobExecution_IdentityGap
	--CROSS APPLY UTIL.ufn_GetRange(GapStart, GapStart + @Vol - 1) c --Inclusive of GapStart
	--WHERE GapRecordCount >= @Vol


	SELECT TOP 1 @GapStart = GapStart 
	FROM UTIL.vw_JobExecution_IdentityGap
	WHERE GapRecordCount >= @Volume
	ORDER BY GapStart

	IF @GapStart is not null
	BEGIN
		INSERT INTO @Data(JobExecutionID)
		SELECT Number
		FROM UTIL.ufn_GetRange(@GapStart, @GapStart + @Volume - 1)c
	END
	ELSE
	BEGIN
		--	DECLARE @Vol bigint = 20
		DECLARE @nextID bigint 
		SELECT @nextID = COALESCE(MAX(JobExecutionID), 0)
		FROM SEIDR.JobExecution
	
		INSERT INTO @Data(JobExecutionID)
		SELECT Number
		FROM UTIL.ufn_GetRange(@nextID + 1, @nextID + @Volume)
	END
	RETURN
END