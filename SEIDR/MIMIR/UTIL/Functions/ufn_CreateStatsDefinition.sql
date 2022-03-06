

CREATE FUNCTION [UTIL].[ufn_CreateStatsDefinition]
(
	@StatName varchar(200),
	@ObjectID int
)
RETURNS VARCHAR(max)
AS
BEGIN
	
	DECLARE @Result varchar(max) = ''
	SELECT --'['+ SCHEMA_NAME(SCHEMA_ID) + '].[' + t.Name +']' [Table],  'Stats',
	@Result = 'CREATE STATISTICS [' + s.Name + '] ON [' + OBJECT_SCHEMA_NAME(@ObjectID) + '].[' + OBJECT_NAME(@ObjectID)+ ']('
	+ STUFF((SELECT ', [' +  CAST(col.Name as varchar(100) )+ ']' [text()]
				FROM  sys.stats_columns AS sc2    
				INNER JOIN sys.columns AS col   
					ON sc2.object_id = col.object_id AND col.column_id = sc2.column_id
				WHERE s.object_id = sc2.object_id AND s.stats_id = sc2.stats_id 				
				FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') + ')'--, @OrderCount - @Stats, s.Name
	+ CASE WHEN Has_Filter = 1 then 'WHERE ' + Filter_Definition else '' end
	FROM sys.stats AS s
	WHERE OBJECT_ID = @ObjectID
	AND User_Created = 1
	AND s.Name = @StatName

	RETURN @Result
END 



-- ufn_CreateIndexDefinition