
CREATE VIEW UTIL.vw_ProcedureInfo
AS
SELECT p.OBJECT_ID, SCHEMA_ID, SCHEMA_NAME(SCHEMA_ID) [Schema],
	(SCHEMA_NAME(SCHEMA_ID)) + '.' + (p.Name) [ProcedureName],
	QUOTENAME(SCHEMA_NAME(SCHEMA_ID)) + '.' + QUOTENAME(p.Name) [QuotedProcedureName],
	parm.Name, parm.Parameter_ID, TYPE_NAME(parm.System_type_id) [ParameterType], 
	Max_Length, [Precision], Scale, 
	Is_Output, Has_Default_Value, Default_Value, Is_Cursor_Ref,
	NULLIF(XML_Collection_id, 0) [XML_Collection_ID],
	Is_ReadOnly,
	Is_Nullable
	--,OBJECT_DEFINITION(p.OBJECT_ID)
FROM sys.procedures p
JOIN sys.parameters parm
	ON p.OBJECT_ID = parm.OBJECT_ID