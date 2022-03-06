CREATE PROCEDURE REFERENCE.[usp_UserKey_u]
	@UserKey varchar(50),
	@Description varchar(130) = null,
	@OverrideOrganizationDefaultPriorityCode varchar(10) = null,
	@RemovePriorityOverride bit = 0,
	@Inbound bit = null,
	@Outbound bit = null,
	@VendorSpecific bit = null
AS
	UPDATE REFERENCE.UserKey
	SET Description = COALESCE(@Description, Description),
		OverrideOrganizationDefaultPriorityCode = CASE WHEN  @RemovePriorityOverride = 0 then COALESCE(@OverrideOrganizationDefaultPriorityCode, OverrideOrganizationDefaultPriorityCode) end,
		Inbound = COALESCE(@Inbound, Inbound),
		Outbound = COALESCE(@Outbound, Outbound),
		VendorSpecific = COALESCE(@VendorSpecific, VendorSpecific),
		LU = GETDATE()
	WHERE userKey = @UserKey
RETURN 0
