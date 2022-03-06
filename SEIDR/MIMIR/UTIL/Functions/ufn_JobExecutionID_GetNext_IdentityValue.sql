CREATE FUNCTION UTIL.[ufn_JobExecutionID_GetNext_IdentityValue]()
RETURNS BIGINT
AS
BEGIN	
	DECLARE  @GapStart bigint
	
	SELECT TOP 1 @GapStart = GapStart 
	FROM UTIL.vw_JobExecution_IdentityGap
	ORDER BY GapStart

	IF @@ROWCOUNT = 0
	BEGIN
		SELECT @GapStart = COALESCE(MAX(JobExecutionID), 0) + 1
		FROM SEIDR.JobExecution	
	END
	RETURN @GapStart
END