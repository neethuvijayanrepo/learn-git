CREATE PROCEDURE [CONFIG].[usp_Package_u]
	@PackageID int,
	@PackageCategory varchar(128) = null,
	@Name varchar(128) = null,
	@NewThreadID int = null,
	@PackagePath varchar(500) = null,
	@ServerName varchar(130) = null,
	@RemoveServer bit = 0,
	@Editor varchar(128) = null
AS
BEGIN	
	SET XACT_ABORT ON
	IF @Editor is null
		SET @Editor = SUSER_NAME()
	ELSE IF @Editor <> SUSER_NAME()
		SET @Editor = @Editor + '(' + SUSER_NAME() + ')'
	DECLARE @now datetime = GETDATE()

	UPDATE SEIDR.SSIS_Package
	SET Category = ISNULL(@PackageCategory, Category),
		Name = ISNULL(@Name, Name),
		ServerName = CASE WHEN @RemoveServer = 0 then COALESCE(@ServerName, ServerName) end,
		PackagePath = COALESCE(@PackagePath, PackagePath)
	OUTPUT DELETED.PackageID, deleted.Category, Deleted.Name, Deleted.ServerName, Deleted.PackagePath, @Editor, @now 
	INTO CONFIG.SSIS_Package_History(PackageID, Category, Name, ServerName, PackagePath, Editor, ArchiveTime)
	WHERE PackageID = @PackageID

	IF @NewThreadID is not null AND @PackageCategory is not null
		exec SEIDR.usp_PackageCategory_SetThreadID @PackageCategory, @NewThreadID

	SELECT * FROM SEIDR.vw_LoaderJob WHERE PackageID = @PackageID
END