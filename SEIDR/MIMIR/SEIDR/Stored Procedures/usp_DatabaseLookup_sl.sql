CREATE PROCEDURE [SEIDR].[usp_DatabaseLookup_sl]	
AS
	SELECT [DatabaseLookupID], 
		[Description], 
		[ServerName],
		[ServerName] as [Server], --Alias for c# connection string wrapper.
		[DatabaseName], 
		[DatabaseName] as [DefaultCatalog],  --Alias for c# connection string wrapper.
		[UserName], 
		[EncryptedPassword],
		[Provider]
	FROM SEIDR.DatabaseLookup lkp	
	WHERE UserName is null or TrustedConnection = 0
RETURN 0
