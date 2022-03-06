CREATE PROCEDURE [SEIDR].[usp_FileValidationDocMetaData_i]
	@JobProfile_JobID int,
	@Delimiter char(1),
	@HasHeader bit,
	@HasTrailer bit,
	@SkipLines int = 0,
	@TextQualifier char(1),
	@ProcessingDate datetime,
	@ColumnMetaData SEIDR.udt_DocMetaDataColumn readonly
AS
	SET XACT_ABORT ON;
	DECLARE @Version int, @NewVersion int, @FromDate datetime

	SELECT @Version = CurrentMetaDataVersion
	FROM SEIDR.FileValidationJob
	WHERE JobProfile_JobID = @JobProfile_JobID
	--AND Active = 1
	IF @@ROWCOUNT != 1
		RETURN 1


	IF EXISTS(SELECT null 
				FROM SEIDR.DocMetaData WITH (NOLOCK)
				WHERE JobProfile_JobID = @JobProfile_JobID
				AND FromDate <= @ProcessingDate
				AND ThroughDate > @ProcessingDate 
				AND Active = 1) 
	BEGIN
		--Overlap. Alternatively, set the old version to inactive? 
		--Probably better to force the user to decide whether to deactivate or change the dates...
		RETURN 2
	END

	DECLARE @MetaDataID int, 
			@ThroughDate datetime

	SELECT @ThroughDate = ThroughDate
	FROM SEIDR.DocMetaData 
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND [Version] = @Version


	--Increment version, null if this is the first meta data set
	SELECT @NewVersion = MAX([Version]) + 1 
	FROM SEIDR.DocMetaData
	WHERE JobProfile_JobID = @JobProfile_JobID

	IF @NewVersion is null
	BEGIN
		SET @NewVersion = 1
		SET @FromDate = '1900-01-01' --Very first one. Set the from date to a min date.
	END
	ELSE
		SET @FromDate = @ProcessingDate

	BEGIN TRAN
	
	IF EXISTS(SELECT null 
				FROM SEIDR.DocMetaData 
				WHERE JobProfile_JobID = @JobProfile_JobID 
				AND [IsCurrent] = 1)
	BEGIN
		UPDATE SEIDR.DocMetaData
		SET ThroughDate = @ProcessingDate
		WHERE JobProfile_JobID = @JobProfile_JobID 
		AND [IsCurrent] = 1  --Checks ThroughDate is null
	END
	

	SELECT @ThroughDate = MIN(FromDate)
	FROM SEIDR.DocMetaData
	WHERE Active = 1
	AND JobProfile_JobID = @JobProfile_JobID
	AND FromDate > @FromDate 
	--If there's any active MetaData from after this file, use its start date as a through date for the new record.
	--Note: aggregate so rowcount is never 0,  value will be null if no match	
	
	IF EXISTS(SELECT null 
				FROM SEIDR.DocMetaData WITH (NOLOCK)
				WHERE JobProfile_JobID = @JobProfile_JobID
				AND Active = 1
				AND FromDate <= @ProcessingDate 
				AND (ThroughDate > @ProcessingDate OR FromDate = ThroughDate AND ThroughDate = @ProcessingDate) --Overlapping. ThroughDate = ProcessingDate is okay unless it matches fromDate, in which case it could cause some confusion.
				)
	BEGIN
		UPDATE md
		SET DD = GETDATE()
		FROM SEIDR.DocMetaData md --Other, older meta data that overlaps with this fromData, Deactivate.
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND Active = 1
		AND FromDate <= @ProcessingDate 
		AND (ThroughDate > @ProcessingDate OR FromDate = ThroughDate AND ThroughDate = @ProcessingDate)
	END

	INSERT INTO SEIDR.DocMetaData
	(
		JobProfile_JobID, 
		Version, Delimiter,
		HasHeader, HasTrailer, 
		SkipLines, TextQualifier, 
		FromDate, ThroughDate
	)
	VALUES 
	(
		@JobProfile_JobID, 
		@NewVersion, @Delimiter, 
		@HasHeader, @HasTrailer, 
		@SkipLines, @TextQualifier, 
		@FromDate, @ThroughDate
	)
	SELECT @MetaDataID = SCOPE_IDENTITY()


	INSERT INTO SEIDR.DocMetaDataColumn(MetaDataID, ColumnName, Position, Max_Length)
	SELECT @MetaDataID, ColumnName, Position, null /* No Fixed width _inference_.*/
	FROM @ColumnMetaData

	--If this is the latest MetaData for the file, set CurrentMetaDataVersion to point to this version
	IF @ThroughDate IS NULL
	BEGIN
		UPDATE j
		SET CurrentMetaDataVersion = @NewVersion, DoMetaDataConfiguration = 0
		FROM SEIDR.FileValidationJob j
		WHERE JobProfile_JobID = @JobProfile_JobID
	END


	COMMIT

RETURN 0
