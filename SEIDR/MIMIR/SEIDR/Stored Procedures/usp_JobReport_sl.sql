CREATE PROCEDURE [SEIDR].[ReportConfiguration_sl]
	@ProcessingDate datetime
AS

	SELECT r.*, 
		COALESCE(db.ServerName, @@SERVERNAME) [ServerName],
		COALESCE(db.DatabaseName, DB_NAME()) [DatabaseName]
	FROM SEIDR.JobReport r
	LEFT JOIN SEIDR.DatabaseLookup db
		ON r.DatabaseLookupID = db.DatabaseLookupID
	WHERE DATEDIFF(day, r.LastExecution, @ProcessingDate) >= 1 	
	
	RETURN 0
