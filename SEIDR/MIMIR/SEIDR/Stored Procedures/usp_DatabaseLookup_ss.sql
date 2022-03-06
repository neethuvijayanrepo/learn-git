CREATE PROCEDURE [SEIDR].[usp_DatabaseLookup_ss]
	@DatabaseLookupID int = null,
	@Description varchar(128) = null
AS
	IF @Description is null AND @DatabaseLookupID is null
	BEGIN
		RAISERROR('Invalid parameters. @Description or @DatabaseLookupID must be provided' , 16, 1)
		RETURn
	END
		
	SELECT * 
	FROM SEIDR.DatabaseLookup 
	WHERE (@DatabaseLookupID is null or DatabaseLookupID = @DatabaseLookupID)
	AND (@Description is null or Description = @Description)

RETURN 0
