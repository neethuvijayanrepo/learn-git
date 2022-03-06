CREATE PROCEDURE SEIDR.usp_JobProfile_sl_FileWatch
	@ThreadID int,
	@ThreadCount int
AS
BEGIN
	
	DECLARE @MatchID int = @ThreadID
	IF @ThreadID = @ThreadCount  --Mod is 0 based, ThreadID 1 based
		SET @MatchID = 0

	SELECT * 
	FROM SEIDR.JobProfile
	WHERE Active = 1
	AND JobProfileID % @ThreadCount = @MatchID --Mod is 0 based, ThreadID 1 based
	AND RegistrationValid = 1
END
