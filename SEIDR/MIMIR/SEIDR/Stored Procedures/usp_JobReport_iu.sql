CREATE PROCEDURE [SEIDR].[JobReport_iu]
	@JobReportID int out,	
	@ReportName varchar(130) = null,
	@SQLProcedure varchar(258) = null,
	@Recipient varchar(500) = null,
	@ArchiveFolder varchar(300) = null,
	@Mode tinyint = null,
	@LastExecution datetime = null,
	@DatabaseLookupID int = null,
	@DatabaseLookupDescription varchar(70) = null
AS
	IF @DatabaseLookupID is null AND @DatabaseLookupDescription is not null
		SELECT @DatabaseLookupID = DatabaseLookupID 
		FROM SEIDR.DatabaseLookup WITH (NOLOCK)
		WHERE [Description] = @DatabaseLookupDescription

	IF @LastExecution is not null
	BEGIN
		UPDATE SEIDR.[JobReport]
		SET LastExecution = @LastExecution
		WHERE JobReportID = @JobReportID
	END
	ELSE IF @JobReportID is null
	BEGIN
		INSERT INTO SEIDR.[JobReport](ReportName, SQLProcedure, Recipient,
		Mode, ArchiveFolder, DatabaseLookupID)
		VALUES(@ReportName, @SQLProcedure, @Recipient, 
		@Mode, @ArchiveFolder, @DatabaseLookupID)
	END
	ELSE
	BEGIN
		UPDATE SEIDR.[JobReport]
		SET ReportName = @ReportName,
			SQLProcedure = @SQLProcedure,
			Recipient = @Recipient,
			Mode = @Mode,
			ArchiveFolder = @ArchiveFolder,
			DatabaseLookupID = @DatabaseLookupID
		WHERE JobReportID = @JobReportID
	END

RETURN 0
