CREATE VIEW [UTIL].[vw_Indexes]
AS
SELECT i.Object_ID, i.Index_ID, OBJECT_SCHEMA_NAME(OBJECT_ID) [Schema], QUOTENAME(OBJECT_SCHEMA_NAME(OBJECT_ID)) + '.' + QUOTENAME(OBJECT_NAME(OBJECT_ID)) [Object_Name], i.Name, i.[TYPE], TYPE_DESC, Is_Unique,
Is_Primary_Key, Is_Unique_Constraint, cc.KeyCount, ic.[IncludedCount],
CASE WHEN IS_PRIMARY_KEY = 1 or is_unique_constraint = 1 
then 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(OBJECT_ID)) + '.' + QUOTENAME(OBJECT_NAME(OBJECT_ID)) + '
DROP CONSTRAINT ' + name
else 'DROP INDEX ' + QUOTENAME(NAME) + ' ON ' + QUOTENAME(OBJECT_SCHEMA_NAME(OBJECT_ID)) + '.' + QUOTENAME(OBJECT_NAME(OBJECT_ID))
end [Drop],
'IF NOT EXISTS(SELECT null FROM sys.indexes WHERE name = ''' + i.Name + ''')
' + DEF [CreateStatement], Fill_Factor, Is_Padded, Is_Disabled, Is_Hypothetical, i.[Allow_Row_Locks], i.[Allow_Page_Locks], i.Filter_Definition
FROM sys.indexes i WITH (NOLOCK)
CROSS APPLY(SELECT UTIL.UFN_CreateIndexDefinition(i.Name, i.OBJECT_ID)) C(DEF)
CROSS APPLY(SELECT COUNT(*) [KeyCount]
			FROM sys.index_columns WITH (NOLOCK) WHERE INDEX_ID = i.INDEX_ID AND OBJECT_ID = i.OBJECT_ID AND Is_Included_Column = 0)cc
CROSS APPLY(SELECT COUNT(*) [IncludedCount]
			FROM sys.index_columns WITH (NOLOCK) WHERE INDEX_ID = i.INDEX_ID AND OBJECT_ID = i.OBJECT_ID AND Is_Included_Column = 1)ic
WHERE type > 0 AND OBJECT_SCHEMA_NAME(OBJECT_ID) <> 'sys'

-- vw_CachedPlans