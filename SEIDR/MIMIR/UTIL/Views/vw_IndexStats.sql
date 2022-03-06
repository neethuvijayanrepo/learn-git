

--Only works as a view in SQL2014 because of the physical stats parameter optimization (cross apply to only get the filtered indexes needed for output)...
CREATE VIEW [UTIL].[vw_IndexStats]	
as	
SELECT 
	Databases.name AS [Database], OBJECT_SCHEMA_NAME(i.OBJECT_ID) [Schema], os.SQLSERVER_START_TIME,
	usageStats.user_Scans, usageStats.user_seeks, user_lookups, 
	'[' + OBJECT_SCHEMA_NAME(i.OBJECT_ID) + '].[' +  OBJECT_NAME(i.OBJECT_ID) + ']' AS [Table],  
	CASE WHEN i.type_desc = 'HEAP' then '[TABLE HEAP]' else i.name end AS [Index],  
	PartitionStats.Partition_Number, 
	p.PartitionsUsed, --Partition info
	PhysicalStats.page_count as [Page_Count],  
	CONVERT(decimal(18,2), (PhysicalStats.page_count * 8) / 1024.0) AS [Total Size (MB)],  
	CONVERT(decimal(18,2), PhysicalStats.avg_fragmentation_in_percent) AS [Frag %], 
	CONVERT(Decimal(18, 2), PhysicalStats.avg_page_space_used_in_percent ) as [PageSpace Used %],
	PartitionStats.row_count AS [Partition Row Count],  
	CONVERT(decimal(18,2), 
	CASE 
		WHEN PartitionStats.Row_Count = 0 then 0 
		else (PhysicalStats.page_count * 8.0 * 1024) 	/ (PartitionStats.row_count ) 
	end) AS [Index Size/Row (Bytes)], 
	Note = CASE 
		WHEN i.TYPE_DESC = 'HEAP' AND Row_Count > 500 then 'NO CLUSTERED INDEX - LARGE HEAP. CONSIDER ADDING CLUSTERED INDEX OR DROPPING TABLE IF NOT IN USE'
		WHEN i.TYPE_DESC = 'HEAP' and Row_Count > 40 then 'Heap but low row count. Consider adding a clustered index if the table is ever queried with sorting'
		WHEN i.TYPE_DESC = 'HEAP' then 'Heap with very Low row count. Clustered Index probably not needed'
		WHEN PartitionStats.PARTITION_NUMBER > 1 then 'Non Primary Partition - May need to look at other partitions for this index'			 
		--WHEN user_lookups > 15 AND type_desc <> 'CLUSTERED' then 'Non clustered index, but lots of of lookups... shouldn''t really happen in a non-heap unless it''s the Primary Key, though'
		--shouldn't need to worry about user lookups
		WHEN user_seeks < 20
			AND user_scans > 20
			AND i.type_desc != 'CLUSTERED'  
			and i.is_unique_constraint = 0 AND i.is_primary_key = 0 AND i.is_unique = 0 
			then 'Low user seeks (' + CONVERT(varchar(20), user_seeks) + '), but index has been scanned '+ CONVERT(varchar(20), user_scans) + ' times. May need to re-evaluate columns used.'
		WHEN user_scans < 20 
			AND user_seeks = 0 
			AND i.type_desc != 'CLUSTERED'  
			and i.is_unique_constraint = 0 AND i.is_primary_key = 0 AND i.is_unique = 0 
			then CASE
				--WHEN DATEDIFF(day, i.create_date/* doesn't exist. :( */, GETDATE()) < @ServerStartupCutoff_For_Unused then 'Index was restarted ' + CONVERT(varchar, DATEDIFF(day, i.create_date, GETDATE())) + ' days ago. UsageStats may not be useful.'
				WHEN DATEDIFF(day, SQLSERVER_START_TIME, GETDATE()) < 3 then 'Server was restarted '+ CONVERT(varchar, DATEDIFF(day, SQLSERVER_START_TIME, GETDATE())) + ' days ago. UsageStats may not be useful.'
				WHEN Row_count < 100 then 'Index has very few rows, may not be a good estimate due to lack of data in table'					   
				WHEN (page_count * 8) / 1024.0 > 1000 then 'Index is large but should be unused. Dropping the index should be safe'
				else 'Index seems to be unused. Should be safe to drop index if there are performance issues.'
			end
		WHEN user_scans < 20
			AND DATEDIFF(day, SQLSERVER_START_TIME, GETDATE()) >= 3
			AND i.type_desc != 'CLUSTERED'  
			and i.is_unique_constraint = 0 AND i.is_primary_key = 0 AND i.is_unique = 0 
			then CASE 
				WHEN user_seeks < 20 then 'Low user seeks ('+ CONVERT(varchar(20), user_seeks) + '). Can probably drop index.'
				ELSE 'Low user scans but at least ten user seeks. Should review manually'
			end
		WHEN avg_Fragmentation_in_Percent >= 25 AND Fragment_Count <= 50 then 'Heavily Fragmented by percentage, but not Count. Probably not causing problems.'
		WHEN avg_fragmentation_in_Percent > 70 then 'DESPERATE FOR REBUILD'
		WHEN avg_Fragmentation_in_percent > 35 then 'Rebuild recommended'	
		WHEN avg_Fragmentation_in_Percent > 30 AND @@VERSION LIKE '%Enterprise Edition%' then 'Rebuild recommended'
		WHEN avg_fragmentation_in_percent >= 25 then 'Reorganize recommended'		
		--WHEN fragment_count < 3000 and avg_fragmentation_in_percent > 5 then 'Small index.  Should be fine to reorganize if looking for performance issues'
		WHEN partitionsUsed > 1 then 'Primary Partition with other partitions - Make sure to look at other partitions for this index'
	END,
	DropStmt = CASE
		WHEN user_scans > 20 OR user_seeks > 20 then null
		WHEN i.type_desc = 'CLUSTERED' then null -- Exclude primary keys, which should not be removed
		WHEN i.is_unique_constraint = 1 OR i.is_primary_key = 1 OR i.is_unique = 1 then null
		WHEN DATEDIFF(day, SQLSERVER_START_TIME, GETDATE()) < 3 then null		
		else 'DROP INDEX [' + i.Name + '] ON  [' + OBJECT_SCHEMA_NAME(i.OBJECT_ID) + '].[' + object_name(i.object_id) + ']; '
	end, --Unused indexes are wasteful!
	ReorgStmt = 'ALTER INDEX [' + i.Name + '] ON  [' + OBJECT_SCHEMA_NAME(i.OBJECT_ID) + '].[' + object_name(i.object_id) + '] ' + 
	CASE  
		WHEN fragment_Count <= 50 then null -- small fragmentation
		WHEN avg_fragmentation_in_percent >= 50 then 'REBUILD' + CASE WHEN PartitionStats.Partition_Number > 1 then ' PARTITION = ' + CONVERT(varchar, PartitionStats.Partition_Number) else '' end
		WHEN avg_fragmentation_in_percent >= 30 
			AND @@VERSION LIKE '%Enterprise Edition%'	then 'REBUILD ' 
				+ CASE WHEN PartitionStats.Partition_Number > 1 then ' PARTITION = ' 
				+ CONVERT(varchar, PartitionStats.Partition_Number) else '' end + 'WITH (ONLINE = ON)'
		WHEN avg_Fragmentation_in_Percent >= 35 then 'REBUILD' + CASE WHEN PartitionStats.Partition_Number > 1 then ' PARTITION = ' + CONVERT(varchar,PartitionStats. Partition_Number) else '' end
		WHEN avg_Fragmentation_in_Percent >=20 then 'REORGANIZE'  + CASE WHEN PartitionStats.Partition_Number > 1 then ' PARTITION = ' + CONVERT(varchar, PartitionStats.Partition_Number) else '' end
	end 	
	+ ';' + CHAR(13) + CHAR(10) + 'RAISERROR(''Finished maintenance on [' + i.Name + '] ! Fragmentation was: '+ CONVERT(varchar, avg_Fragmentation_in_percent) + ''', 0, 1) WITH NOWAIT;'
	,i.OBJECT_ID, i.INDEX_ID
FROM sys.dm_db_index_usage_stats UsageStats 
INNER JOIN sys.indexes i
	ON i.index_id = UsageStats.index_id  
	AND i.object_id = UsageStats.object_id  
CROSS APPLY(SELECT MAX(Partition_Number) PartitionsUsed FROM sys.dm_db_partition_stats WHERE OBJECT_ID = i.OBJECT_ID AND INDEX_ID = i.INDEX_ID)p
CROSS JOIN sys.dm_os_sys_info os
INNER JOIN SYS.databases Databases 
	ON Databases.database_id = UsageStats.database_id  
CROSS APPLY(SELECT *
			FROM sys.dm_db_index_physical_stats (DB_ID(), i.OBJECT_ID, NULL, NULL, 'LIMITED')
			WHERE INDEX_ID = usageStats.INDEX_ID
			) physicalStats
INNER JOIN SYS.dm_db_partition_stats PartitionStats
	ON PartitionStats.index_id = UsageStats.index_id  
	and PartitionStats.object_id = UsageStats.object_id 
	AND physicalStats.Partition_Number = PartitionStats.Partition_Number 
WHERE 1=1  
AND databases.database_id = DB_ID()
	--AND ( 
	--	(UsageStats.user_seeks + usageStats.user_scans + usagestats.user_lookups) < 100  AND i.is_primary_key = 0 AND i.is_unique_constraint = 0 AND i.is_unique = 0
	--	OR 
	--	avg_Fragmentation_In_percent >= 5 AND fragment_count > 500 --Should be a Decently sized fragmentation		
	--)