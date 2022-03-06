CREATE VIEW [UTIL].[vw_DataPopHelper]
AS
SELECT t.OBJECT_ID, OBJECT_SCHEMA_NAME(t.OBJECT_ID) [Schema], t.Name [Table],
CASE WHEN EXISTS(SELECT null FROM sys.columns WHERE OBJECT_ID = t.OBJECT_ID AND IS_IDENTITY = 1) then 'SET IDENTITY_INSERT [' + OBJECT_SCHEMA_NAME(t.OBJECT_ID) + '].[' + t.Name + '] ON;' end [IdentOn],
'INSERT INTO [' + OBJECT_SCHEMA_NAME(t.OBJECT_ID) + '].[' + t.Name + ']('
	+ STUFF(
			(SELECT ', ' + c.Name
				FROM sys.columns c
				WHERE OBJECT_ID = t.OBJECT_ID AND Is_computed = 0
				FOR XML PATH ('')), 1, 2, '') + ')
VALUES ' [InsertStatement],
'SELECT [Script] = ''('' + ' + 
		STUFF(
			(SELECT '+'',''+' + CHAR(10) + '	' +
 				CASE WHEN is_Nullable = 1 then 'COALESCE('
				else '' end
				+ CASE 
					WHEN TYPE_NAME(SYSTEM_TYPE_ID) IN ('datetime', 'smalldatetime', 'timespan', 'uniqueidentifier')
					then ''''''''' + CONVERT(varchar(8000), [' + Name + ']) + '''''''''
					WHEN TYPE_NAME(SYSTEM_TYPE_ID) NOT LIKE '%CHAR%' AND TyPE_NAME(SYSTEM_TYPE_ID) <> 'text'
					then 'CONVERT(varchar(8000), [' + Name + '])'

					else ''''''''' + REPLACE([' + Name + '], '''''''', '''''''''''') + '''''''''
					end
				+ CASE WHEN is_Nullable = 1 then ', ''NULL'')' else ''
				end
				FROM sys.columns c
				WHERE OBJECT_ID = t.OBJECT_ID AND Is_computed = 0
				FOR XML PATH('') )
				, 1, 5, '') + '+ '')''
FROM [' + OBJECT_SCHEMA_NAME(t.OBJECT_ID) + '].[' + t.Name + ']'  [DataSelectInsertStatement],
CASE WHEN EXISTS(SELECT null FROM sys.columns WHERE OBJECT_ID = t.OBJECT_ID AND IS_IDENTITY = 1) then 'SET IDENTITY_INSERT [' + OBJECT_SCHEMA_NAME(t.OBJECT_ID) + '].[' + t.Name + '] OFF;' end [IdentOff]
,SelectStatement = 
'SELECT '+ STUFF(
			(SELECT ', ' + CHAR(10) + '	' + QUOTENAME(c.Name)
				FROM sys.columns c
				WHERE OBJECT_ID = t.OBJECT_ID AND Is_computed = 0
				FOR XML PATH('') )
				, 1, 2, '') + '
FROM [' + OBJECT_SCHEMA_NAME(OBJECT_ID) + '].' + QUOTENAME(OBJECT_NAME(OBJECT_ID))
FROM sys.tables t
WHERE OBJECT_SCHEMA_NAME(OBJECT_ID) NOT IN ('sys', 'dbo')

-- vw_ForeignKeys