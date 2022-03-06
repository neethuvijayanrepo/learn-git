	CREATE VIEW UTIL.vw_Stats
	AS
	SELECT 
		STATS_ID, Name, OBJECT_ID, QUOTENAME(OBJECT_SCHEMA_NAME(OBJECT_ID)) + '.' + QUOTENAME(OBJECT_NAME(OBJECT_ID)) [Object], 
			Has_Filter, Filter_Definition, cc.ColumnCount,
			'DROP STATISTICS [' + OBJECT_SCHEMA_NAME(OBJECT_ID) + '].[' + OBJECT_NAME(OBJECT_ID) + '].[' + Name + '];' [DropStatement],
			UTIL.ufn_CreateStatsDefinition(s.name, OBJECT_ID) [CreateStatement],
			STUFF((SELECT ',' + COL_NAME(OBJECT_ID, COLUMN_ID)
				FROM sys.stats_columns sc
				WHERE OBJECT_ID = s.OBJECT_ID AND sc.STATS_ID = s.STATS_ID
				FOR XML PATH ('')), 1, 1, '') Referenced_Columns
	FROM sys.stats s
	CROSS APPLY(SELECT COUNT(*) [ColumnCount] 
				FROM sys.stats_Columns 
				WHERE OBJECT_ID = s.OBJECT_ID AND STATS_ID = s.STATS_ID)cc
	WHERE NOT EXISTS(SELECT null FROM sys.indexes WHERE OBJECT_ID = s.OBJECT_ID and INDEX_ID = s.STATS_ID)
	AND USER_CREATED = 1