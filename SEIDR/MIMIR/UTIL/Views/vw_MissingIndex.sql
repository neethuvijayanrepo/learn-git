

CREATE view [UTIL].[vw_MissingIndex]
AS
select 
'CREATE INDEX IDX_' + o.Name COLLATE DATABASE_DEFAULT + '_' + ISNULL(REPLACE(REPLACE(REPLACE(d.Equality_Columns COLLATE DATABASE_DEFAULT, '[',''),']',''),', ','_') ,'')
+  ISNULL('_' + REPLACE(REPLACE(REPLACE(d.InEquality_Columns, '[',''),']',''),', ','_'),'')
+  ISNULL('_' + REPLACE(REPLACE(REPLACE(d.included_columns, '[',''),']',''),', ','_'),'')
+ ' ON ' + d.Statement
+ '(' + ISNULL(d.Equality_Columns,'') + CASE WHEN (d.Equality_Columns + d.inequality_columns) IS NOT NULL THEN ',' ELSE ''END + ISNULL(d.inequality_Columns,'') + ')'
+ ISNULL(' INCLUDE ( ' + d.Included_Columns + ')' , '') as CreateStatement,
'CREATE INDEX IDX_' + o.Name + '_' + CAST(d.Index_Handle as varchar(15))
+ ' ON ' + d.Statement COLLATE DATABASE_DEFAULT
+ '(' + ISNULL(d.Equality_Columns,'') + CASE WHEN (d.Equality_Columns + d.inequality_columns) IS NOT NULL THEN ',' ELSE ''END + ISNULL(d.inequality_Columns,'') + ')'
+ ISNULL(' INCLUDE ( ' + d.Included_Columns + ')' , '') as ShortCreateStatement,
d.database_id, 
d.object_id, 
d.index_handle, 
d.equality_columns, 
d.inequality_columns, 
d.included_columns, 
d.statement as fully_qualified_object,

gs.* 
from sys.dm_db_missing_index_groups g

join sys.dm_db_missing_index_group_stats gs on gs.group_handle = g.index_group_handle
join sys.dm_db_missing_index_details d on g.index_handle = d.index_handle
JOIN sys.Objects o ON d.[Object_id]=o.object_id