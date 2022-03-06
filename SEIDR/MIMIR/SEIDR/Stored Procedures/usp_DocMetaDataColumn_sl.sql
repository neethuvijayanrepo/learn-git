CREATE PROCEDURE [SEIDR].[usp_DocMetaDataColumn_sl]
	@JobProfile_JobID int,
	@ProcessingDate datetime
AS
BEGIN
	DECLARE @MetaDataID int

	SELECT TOP 1 @MetaDataID = MetaDataID
	FROM SEIDR.DocMetaData
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND @ProcessingDate >= FromDate
	AND @ProcessingDate < ThroughDate
	AND [Active] = 1
	AND [IsCurrent] = 0
	ORDER BY MetaDataID desc --shouldn't be active overlaps, but most recent. Multiple with fromDate = throughDate might have unexpected behavior ? But not really too worried about that.

	IF @@ROWCOUNT = 0
	BEGIN
		SELECT TOP 1 @MetaDataID = MetaDataID
		FROM SEIDR.DocMetaData
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND @ProcessingDate >= FromDate
		AND Active = 1
		AND [IsCurrent] = 1
		
		IF @@ROWCOUNT = 0
			RETURN -1
	END

	SELECT * 
	FROM SEIDR.DocMetaData 
	WHERE MetaDataID = @MetaDataID


	SELECT * 
	FROM SEIDR.DocMetaDataColumn c 
	WHERE MetaDataID = @MetaDataID
	ORDER BY c.Position ASC
	

	RETURN 0
END