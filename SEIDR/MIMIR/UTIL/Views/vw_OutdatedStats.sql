CREATE VIEW [UTIL].[vw_OutdatedStats]
as
SELECT s.Object_ID, s.Stats_ID,
	OBJECT_SCHEMA_NAME(s.OBJECT_ID) + '.' + OBJECT_NAME(s.OBJECT_ID) [Object], s.Name [Stat], 
	DATEDIFF(day, STATS_DATE(s.OBJECT_ID, s.STATS_ID), GETDATE()) [DaysSinceUpdate], 
	--[RecordChange],
	Modification_Counter, p.steps,
	[Modification_Weight] = Modification_Counter * (Steps + SQRT(Rows_Sampled) ) / Unfiltered_Rows, --Rows, 
	
	/*
	Number of modifications to the table/index/stat, times the number of steps in the histogram + Log10 of the Rows_Sampled. 
	(SQRT or Log to give more weight to the number of steps in the histogram. The fewer steps there, the less likely the stat is to be affected by changes. (Not as much variety in the data included in the stat)
	But if the table doesn't have as man records, this could just be due to not having enough data yet, so add a reduced weight of Rows_Sampled)
	Then divided by total number of Rows to give a ratio relative to the size of the table, give the same number of changes a higher weight when it's a smaller percentage of total rows
	If using Log, I think Values > ~1-2 would probably benefit from a fullscan even if there aren't as many rows. Otherwise, if Steps > 100 or the table simply has a large number 
		of rows, we might be better off with a fullscan, since that means there's probably a lot of variance/room for stats to likely affect the query plan more.
	*/
	 [Update] = 'UPDATE STATISTICS ['+OBJECT_SCHEMA_NAME(s.OBJECT_ID) + '].[' + OBJECT_NAME(s.OBJECT_ID)+'] ['+s.name + '];',
	 [FullScanUpdate] = 'UPDATE STATISTICS ['+OBJECT_SCHEMA_NAME(s.OBJECT_ID) + '].[' + OBJECT_NAME(s.OBJECT_ID)+'] ['+s.name + '] WITH FULLSCAN;',
	 p.last_updated, p.rows, p.rows_sampled, p.unfiltered_rows, p.Unfiltered_rows - p.rows_sampled [Unsampled_Rows], s.no_recompute, s.Has_filter, s.filter_definition
FROM SYS.stats s
CROSS APPLY sys.dm_db_stats_properties(s.OBJECT_ID, s.STATS_ID) p
LEFT JOIN sys.indexes i
	ON s.OBJECT_ID = i.OBJECT_ID
	AND s.Name = i.Name
	AND i.Is_Primary_Key = 1
WHERE auto_created = 0
AND i.index_id is null --stats on the PK probably shouldn't really be a big deal
AND OBJECT_SCHEMA_NAME(s.OBJECT_ID) NOT IN('sys', 'dbo')
AND (Modification_Counter > 10000 OR SQRT(Rows_Sampled) * 10 < Modification_Counter AND [Rows] > 100)
--AND DATEDIFF(day, STATS_DATE(s.OBJECT_ID, s.STATS_ID), GETDATE()) > 0
--ORDER BY  p.Modification_Counter desc, DaysSinceUpdate DESC






-- vw_MissingDefaultConstraints