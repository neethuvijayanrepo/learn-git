
CREATE VIEW UTIL.vw_MissingDefaultConstraints
AS
SELECT OBJECT_SCHEMA_NAME(t.OBJECT_ID) [Schema], t.Name [Table], c.Name [Column], 'ALTER TABLE [' + object_SCHEMA_NAME(t.OBJECT_ID) + '].[' + t.name + '] ADD CONSTRAINT DF_' + t.Name + '_' + c.Name + '
DEFAULT(' + CASE TYPE_NAME(SYSTEM_TYPE_ID) WHEN 'int' then '0' when 'smallint' then '0' when 'bit' then '0'
when 'datetime' then 'GETDATE()' when 'smalldatetime' then 'GETDATE()' else '???????????' end+ ') FOR [' + c.Name + ']' [Script]
FROM sys.tables t
JOIN sys.columns c
	ON c.OBJECT_ID = t.OBJECT_ID
WHERE NOT EXISTS(SELECT null FROM sys.default_constraints WHERE parent_object_id = c.OBJECT_ID AND PARENT_COLUMN_ID = c.COLUMN_ID)
AND c.is_Nullable = 0 and c.is_identity = 0 AND c.is_computed = 0
--AND t.OBJECT_ID = OBJECT_ID('APP.PayerMaster')
AND c.Name NOT IN ('FacilityID', t.Name +  'Code', 'Description', REPLACE(t.Name, 'Master', 'Code'), 'LoadBatchID', 'AccountID')
AND OBJECT_SCHEMA_NAME(t.OBJECT_ID) NOT IN ('ETL', 'EXPORT', 'TEMP', 'dbo', 'LOOKUP', 'UTIL')
AND t.NAME NOT LIKE '%[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
AND c.Name NOT LIKE '%DateSerial%'
AND c.Name NOT LIKE '%RowHash'
AND NOT EXISTS(SELECT null FROM sys.procedures WHERE name = 'sp_MSIns_' + OBJECT_SCHEMA_NAME(t.OBJECT_ID) + t.name) --Not replicated...