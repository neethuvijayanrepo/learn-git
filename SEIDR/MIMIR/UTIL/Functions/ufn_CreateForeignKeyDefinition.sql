CREATE FUNCTION UTIL.ufn_CreateForeignKeyDefinition
	(
	@Constraint_Object_ID int
	)
RETURNS varchar(max)
AS
BEGIN
	DECLARE @Table varchar(300) 
	
	DECLARE @Result varchar(max) 
	SELECT @Result = 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(fk.PARENT_OBJECT_ID) + '].[' + OBJECT_NAME(fk.PARENT_OBJECT_ID) + '] ' 
	+ CASE WHEN is_Not_trusted = 1 AND Is_Not_For_Replication = 0 then ' WITH NOCHECK' else ' WITH CHECK' end + ' 
ADD CONSTRAINT [' + fk.Name + '] FOREIGN KEY (' 
		+ STUFF( (SELECT ',' + COL_NAME(PARENT_OBJECT_ID, PARENT_COLUMN_ID) FROM sys.foreign_Key_Columns WHERE CONSTRAINT_OBJECT_ID = fk.OBJECT_ID FOR XML PATH ('')), 1, 1, '')
		+ ') 
REFERENCES [' + OBJECT_SCHEMA_NAME(fk.REFERENCED_OBJECT_ID) + '].[' + OBJECT_NAME(fk.REFERENCED_OBJECT_ID) + '](' 
		+ STUFF( (SELECT ',' + COL_NAME(REFERENCED_OBJECT_ID, REFERENCED_COLUMN_ID) FROM sys.foreign_Key_Columns WHERE CONSTRAINT_OBJECT_ID = fk.OBJECT_ID FOR XML PATH ('')), 1, 1, '')
		 + ')'
	+ CASE WHEN delete_referential_action > 0 then ' 
ON DELETE ' + DELETE_REFERENTIAL_ACTION_DESC else '' end 
	+ CASE WHEN update_referential_action > 0 then ' 
ON UPDATE ' + UPDATE_REFERENTIAL_ACTION_DESC else '' end
	+ ';
	'
	FROM sys.foreign_keys fk
	WHERE OBJECT_ID = @CONSTRAINT_OBJECT_ID
	RETURN @Result
	/*
	Referential options:

	[ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
    [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ] 
	*/
END
-- ufn_CreateStatsDefinition