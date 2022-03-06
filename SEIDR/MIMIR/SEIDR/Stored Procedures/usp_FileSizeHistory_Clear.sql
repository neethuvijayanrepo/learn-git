CREATE PROCEDURE [SEIDR].[usp_FileSizeHistory_Clear]
	@FileSizeCheckJobID int,
	@DateThrough date = null,
	@DateFrom date = null,
	@DaysToClear int = null,	
	@UserName varchar(260) = null
AS	
	SET XACT_ABORT ON
	IF @userName is null
		SET @userName = SUSER_NAME()
	ELSE IF @userName <> SUSER_NAME()
		SET @userName = @userName + ' (' + SUSER_NAME() + ')'
	IF @DateThrough is null AND @DaysToClear is null
	BEGIN
		RAISERROR('Must provide a @DateThrough if not providing DaysToClear', 16, 1)
		RETURN
	END
	SET NOCOUNT ON
	IF @DateFrom is null
	BEGIN
		SELECT @DateFrom = MIN(FileDate)
		FROM SEIDR.FileSizeHistory WITH (NOLOCK)
		WHERE FileSizeCheckJobID = @FileSizeCheckJobID
	END 

	IF @DateThrough IS NULL
	BEGIN
		IF @DaysToClear IS NULL
		BEGIN
			SET @DateThrough = @DateFrom
			RAISERROR('No @DateThrough - using @DateFrom as @DateThrough', 0, 0)
		END
		ELSE
		BEGIN
			SET @DateThrough = DATEADD(day, @DaysToClear, @DateFrom)
			RAISERROR('Setting @DateThrough to %d days after @DateFrom', 0, 0, @DaysToClear)
		END
	END
	ELSE IF @DateThrough < @DateFrom
	BEGIN
		RAISERROR('@DateThrough must be equal or greater to the @DateFrom', 16, 1)
		RETURN
	END
	DECLARE @msgFrom varchar(20), @msgThrough varchar(20)
	SET @msgFrom = CONVERT(varchar(20), @DateFrom, 0)
	SET @msgThrough = CONVERT(varchar(20), @DateThrough, 0)
	RAISERROR('@DateFrom: "%s", @DateThrough: "%s"', 0, 0, @msgFrom, @msgThrough)
	
	DECLARE @jobExecutionList UTIL.udt_BigIntID
	
	UPDATE SEIDR.FileSizeHistory
	SET DD = GETDATE()
	OUTPUT INSERTED.JobExecutionID INTO @jobExecutionList(ID)
	WHERE FileSizeCheckJobID = @FileSizeCheckJobID
	AND Active = 1
	AND FileDate BETWEEN @DateFrom AND @DateThrough

	INSERT INTO SEIDR.JobExecution_Note(JobExecutionID, JobProfile_JobID, [Auto], Technical, NoteText, UserName, StepNumber)
	SELECT l.ID, j.JobProfile_JobID, 1, 1, 'Bulk Deactivated File Size History', @UserName, jpj.StepNumber
	FROM  SEIDR.FileSizeCheckJob j WITH (NOLOCK)
	JOIN SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		ON j.JobProfile_JobID = jpj.JobProfile_JobID
	CROSS JOIN @jobExecutionList l
	WHERE j.FileSizeCheckJobID = @FileSizeCheckJobID	
RETURN 0
