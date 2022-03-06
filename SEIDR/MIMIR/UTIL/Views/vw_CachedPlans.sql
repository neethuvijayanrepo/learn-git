CREATE VIEW UTIL.vw_CachedPlans 
as
SELECT OBJECTID as OBJECT_ID, OBJECT_SCHEMA_NAME(OBJECTID) [SchemaName], OBJECT_NAME(OBJECTID) [ObjectName], UseCounts, 
DBID as DB_ID, ObjType, Size_In_Bytes, Query_Plan,
OBJECT_DEFINITION(OBJECTID) [Definition],
'DBCC FREEPROCCACHE (' + master.dbo.Fn_varbintohexstr(plan_handle) + ');' [RemovePlanFromCache]
FROM sys.dm_exec_cached_plans c
CROSS APPLY sys.dm_exec_query_plan(plan_handle)
-- vw_OutdatedStats