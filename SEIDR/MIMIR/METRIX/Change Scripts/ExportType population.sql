
IF NOT EXISTS(SELECT null FROM METRIX.ImportType WHERE Description = '277 EDI Import')
BEGIN
   INSERT INTO METRIX.ImportType(Description)
   VALUES('277 EDI Import'), ('277 EDI Import Multi Batch')

   INSERT INTO METRIX.ExportType(Description, ImportTypeID)
   SELECT REPLACE(REPLACE(Description, '277', '276'), 'Import', 'Export'), ImportTypeID
   FROM METRIX.ImportType
   WHERE Description IN ('277 EDI Import', '277 EDI Import Multi Batch')
END
GO

IF NOT EXISTS(SELECT null FROM METRIX.ImportType WHERE Description = 'Skip Tracing Import')
BEGIN
   INSERT INTO METRIX.ImportType(Description)
   VALUES('Skip Tracing Import')

   INSERT INTO METRIX.ExportType(Description, ImportTypeID)
   SELECT 'SkipTracing Export', ImportTypeID
   FROM METRIX.ImportType
   WHERE Description IN ('Skip Tracing Import')
END
GO

IF NOT EXISTS(SELECT Null FROM METRIX.ExportType WHERE Description = 'Statement Export')
BEGIN
	INSERT INTO METRIX.ExportType(Description)
	VALUES('Statement Export')
END

IF NOT EXISTS(SELECT Null FROM METRIX.ExportType WHERE Description = 'VendorBalance Export')
BEGIN
	INSERT INTO METRIX.ExportType(Description)
	VALUES('VendorBalance Export')
END


INSERT INTO METRIX.Vendor(Description)
SELECT *
FROM 
(
	SELECT 'TransUnion'
	UNION ALL 
	SELECT 'LiveVox'
    UNION ALL
    SELECT 'LexisNexis'
	UNION ALL
	SELECT 'Nordis'
	UNION ALL
	SELECT 'PatientCo'
)q(Vendor)
WHERE NOT EXISTS(SELECT Null FROM METRIX.Vendor WHERE Description = q.Vendor)
GO