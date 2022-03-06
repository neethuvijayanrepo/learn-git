CREATE PROCEDURE REFERENCE.usp_Project_SetThroughDate 
	@ProjectID int,
	@ThroughDate date
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE REFERENCE.Project
	SET ThroughDate = @ThroughDate
	WHERE ProjectID = @ProjectID

	IF @@ROWCOUNT > 0 AND @ThroughDate is not null	
	BEGIN	
		UPDATE jp
		SET DD = GETDATE()
		FROM SEIDR.JobProfile jp
		WHERE jp.ProjectID = @ProjectID
		AND RegistrationValid = 1
		AND @ThroughDate <= GETDATE()
		AND Active = 1

		UPDATE jp
		SET ScheduleThroughDate = @ThroughDate
		FROM SEIDR.JobProfile jp
		WHERE jp.ProjectID = @ProjectID
		AND ScheduleValid = 1
		AND Active = 1		
	END
END