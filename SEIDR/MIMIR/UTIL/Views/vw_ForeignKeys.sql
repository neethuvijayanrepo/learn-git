﻿CREATE VIEW [UTIL].[vw_ForeignKeys]
as
	SELECT fk.OBJECT_ID [ForeignKeyID],
		fk.Name [ForeignKey],
		fk.PARENT_OBJECT_ID,
		OBJECT_SCHEMA_NAME(fk.PARENT_OBJECT_ID) + '.' + OBJECT_NAME(fk.PARENT_OBJECT_ID) [PARENT_OBJECT],
		fk.REFERENCED_OBJECT_ID,
		OBJECT_SCHEMA_NAME(fk.REFERENCED_OBJECT_ID) + '.' + OBJECT_NAME(fk.REFERENCED_OBJECT_ID) REFERENCED_OBJECT,
		fk.IS_NOT_TRUSTED,
		fk.IS_DISABLED,
		fk.DELETE_REFERENTIAL_ACTION_DESC,
		fk.UPDATE_REFERENTIAL_ACTION_DESC,
		STUFF((SELECT ',' + COL_NAME(fkc.PARENT_OBJECT_ID, fkc.PARENT_COLUMN_ID)
				FROM sys.foreign_key_columns fkc
				WHERE CONSTRAINT_OBJECT_ID = fk.OBJECT_ID
				FOR XML PATH ('')), 1, 1, '') Parent_Columns,
		STUFF((SELECT ',' + COL_NAME(fkc.REFERENCED_OBJECT_ID, fkc.REFERENCED_COLUMN_ID)
				FROM sys.foreign_key_columns fkc
				WHERE CONSTRAINT_OBJECT_ID = fk.OBJECT_ID
				FOR XML PATH ('')), 1, 1, '') Referenced_Columns,
		REPLACE(REPLACE(STUFF((SELECT ', '  +  OBJECT_SCHEMA_NAME(PARENT_OBJECT_ID) + '.' + OBJECT_NAME(PARENT_OBJECT_ID) + '.' + COL_NAME(PARENT_OBJECT_ID, PARENT_COLUMN_ID)
			+ ' -> ' + OBJECT_SCHEMA_NAME(REFERENCED_OBJECT_ID) + '.' + OBJECT_NAME(REFERENCED_OBJECT_ID) + '.' + COL_NAME(REFERENCED_OBJECT_ID, REFERENCED_COLUMN_ID)
			FROM sys.foreign_key_columns
			WHERE CONSTRAINT_OBJECT_ID = fk.OBJECT_ID
			FOR XML PATH ('')), 1, 2, ''), ', ', ',' + CHAR(13) + CHAR(10)), '-&gt;', '->') ColumnMappings,
		(SELECT COUNT(*) FROM sys.foreign_key_columns WHERE CONSTRAINT_OBJECT_ID = OBJECT_ID) NumberOfColumns,
		'ALTER TABLE [' + OBJECT_SCHEMA_NAME(PARENT_OBJECT_ID) + '].[' + OBJECT_NAME(PARENT_OBJECT_ID) + ']
CHECK CONSTRAINT [' + fk.Name + '];' [CheckStatement],
		'ALTER TABLE [' + OBJECT_SCHEMA_NAME(PARENT_OBJECT_ID) + '].[' + OBJECT_NAME(PARENT_OBJECT_ID) + ']
DROP CONSTRAINT [' + fk.Name + '];' [DropStatement],
		UTIL.ufn_CreateForeignKeyDefinition(fk.OBJECT_ID) [CreateStatement]
		FROM sys.foreign_keys fk


-- vw_Indexes