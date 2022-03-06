


CREATE VIEW [UTIL].[vw_Constraints]
AS
	SELECT Name [Constraint], null as OBJECT_ID, OBJECT_NAME [PARENT_OBJECT], OBJECT_ID [PARENT_OBJECT_ID],
			null as PARENT_COLUMN, null as PARENT_COLUMN_ID,
			'UNIQUE' as ConstraintType,
			[Drop] as [DropStatement], 
			CreateStatement,
			null as [Definition]
	FROM UTIL.vw_Indexes
	WHERE is_Unique_Constraint = 1
	UNION ALL
	SELECT Name, OBJECT_ID, PARENT_OBJECT, PARENT_OBJECT_ID,
			PARENT_COLUMN, PARENT_COLUMN_ID,
			'CHECK' as ConstraintType, 
			[DropStatement], CreateStatement,
			[Definition]
	FROM UTIL.vw_Check_Constraints
	UNION ALL
	SELECT name,  OBJECT_ID, PARENT_OBJECT, PARENT_OBJECT_ID,
			PARENT_COLUMN, PARENT_COLUMN_ID,
			'DEFAULT' as ConstraintType, 
			[DropStatement], CreateStatement,
			[Definition]
	FROM UTIL.vw_Default_Constraints