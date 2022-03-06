CREATE PROCEDURE REFERENCE.usp_Organization_SetThroughDate
	@OrganizationID int,
	@ThroughDate date
AS
BEGIN	
	IF @OrganizationID = 0
	BEGIN
		RAISERROR('Cannot set throughDate on OrganizationID 0.', 16, 1)
		RETURN
	END
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE REFERENCE.Organization
	SET OrganizationThroughDate = @ThroughDate
	WHERE (OrganizationID = @OrganizationID OR ParentOrganizationID = @OrganizationID)
	--AND ThroughDate is null

	IF @@ROWCOUNT > 0 AND @ThroughDate is not null
	BEGIN
		UPDATE p
		SET ThroughDate = o.OrganizationThroughDate
		FROM REFERENCE.Project p
		JOIN REFERENCE.Organization o
			ON p.OrganizationID = o.OrganizationID
		WHERE o.OrganizationThroughDate is not null
		AND p.ThroughDate is null
		AND p.FromDate <= o.OrganizationThroughDate

		UPDATE jp
		SET DD = GETDATE()
		FROM SEIDR.JobProfile jp
		JOIN REFERENCE.Organization o
			ON jp.OrganizationID = o.OrganizationID
		WHERE @OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)
		AND RegistrationValid = 1
		AND OrganizationThroughDate <= GETDATE()
		AND Active = 1

		UPDATE jp
		SET ScheduleThroughDate = o.OrganizationThroughDate
		FROM SEIDR.JobProfile jp
		JOIN REFERENCE.Organization o
			ON jp.OrganizationID = o.OrganizationID
		WHERE @OrganizationID IN (o.OrganizationID, o.ParentOrganizationID)
		AND ScheduleValid = 1
		AND Active = 1
	END
END