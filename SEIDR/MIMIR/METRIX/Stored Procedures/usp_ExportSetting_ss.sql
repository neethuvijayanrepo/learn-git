CREATE PROCEDURE [METRIX].[usp_ExportSetting_ss]
	@JobProfile_JobID int
AS
	SELECT es.ArchiveLocation,
	es.MetrixDatabaseLookupID, 
	es.VendorID, v.Description [VendorName],
	es.ExportTypeID, ex.Description [ExportType],
	es.ImportTypeID, im.Description [ImportType]
	FROM METRIX.ExportSettings es
	LEFT JOIN METRIX.Vendor v
		ON es.VendorID = v.VendorID
	LEFT JOIN METRIX.ExportType ex
		ON es.ExportTypeID = ex.ExportTypeID
	LEFT JOIN METRIX.ImportType im
		ON es.ImportTypeID = im.ImportTypeID
	WHERE JobProfile_JobID = @JobProfile_JobID
	
	RETURN 0