CREATE trigger [DbTrg_backup_objects] on 
database 
for DDL_TABLE_VIEW_EVENTS, DDL_FUNCTION_EVENTS, DDL_PROCEDURE_EVENTS, DDL_TRIGGER_EVENTS, DDL_ASSEMBLY_EVENTS
as set nocount on 
declare @data xml 
set @data = EVENTDATA() 
IF (@data.value('(/EVENT_INSTANCE/EventType)[1]', 'varchar(50)') NOT IN ( 'UPDATE_STATISTICS', 'ALTER_INDEX')
	AND @data.value('(/EVENT_INSTANCE/LoginName)[1]', 'varchar(256)') NOT LIKE '%ImpactLoaderUser%')
	insert into UTIL.DDL_ChangeLog(DatabaseName, EventType, ObjectName, ObjectType, SqlCommand, LoginName, ObjectSchema) 
	values( @data.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'varchar(256)'), 
	@data.value('(/EVENT_INSTANCE/EventType)[1]', 'varchar(50)'), 
	@data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'varchar(256)'), 
	@data.value('(/EVENT_INSTANCE/ObjectType)[1]', 'varchar(25)'), 
	@data.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'varchar(max)'), 
	@data.value('(/EVENT_INSTANCE/LoginName)[1]', 'varchar(256)'),
	@data.value('(/EVENT_INSTANCE/SchemaName)[1]', 'varchar(256)') )