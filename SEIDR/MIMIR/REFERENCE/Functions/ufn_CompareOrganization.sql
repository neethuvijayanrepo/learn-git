CREATE FUNCTION [REFERENCE].[ufn_CompareOrganization](@OrganizationID1 int, @OrganizationID2 int, @Org1ParentRelation bit = 0)
RETURNS BIT
AS
BEGIN
	IF @OrganizationID1 is null
		SET @OrganizationID1 = -1
	IF @ORganizationID2 is null
		SET @OrganizationID2 = -1 --Invalid
	IF -1 IN (@OrganizationID1, @OrganizationID2)
		RETURN 0
	IF @Org1ParentRelation = 1
	BEGIN
		--Organizations match, or OrganizationID1 is a parent to Org2
		IF @OrganizationID1 = 0 OR @OrganizationID1 = @OrganizationID2
		BEGIN
			RETURN 1
		END
		ELSE IF @OrganizationID2 = 0
			RETURN 0

		
		IF EXISTS(SELECT null
					FROM REFERENCE.Organization o WITH (NOLOCK)					
					WHERE o.OrganizationID = @OrganizationID2
					AND o.ParentOrganizationID = @OrganizationID1)
			RETURN 1
		RETURN 0
	END

	IF 0 IN (@OrganizationID1, @OrganizationID2) OR @OrganizationID1 = @OrganizationID2
		RETURN 1

	IF EXISTS(SELECT null
				FROM REFERENCE.Organization o WITH (NOLOCK)					
				WHERE o.OrganizationID = @OrganizationID1
				AND o.ParentOrganizationID = @OrganizationID2)
		RETURN 1
	IF EXISTS(SELECT null
				FROM REFERENCE.Organization o WITH (NOLOCK)					
				WHERE o.OrganizationID = @OrganizationID2
				AND o.ParentOrganizationID = @OrganizationID1)
		RETURN 1
	RETURN 0
END