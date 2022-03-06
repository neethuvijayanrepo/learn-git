 
-- =============================================
-- Author:		Ryan
-- Create date: A while ago
-- Description:	For recreating index after dropping to make a column change 
--				(Specifically in a helper sproc for changing datatype of a primary key used in foreign keys)
--				Does not actually do the update. Partitioned indexes not supported.
-- ============================================= 
CREATE FUNCTION [UTIL].[ufn_CreateIndexDefinition] 
(
	-- Add the parameters for the function here
	@IndexName varchar(200),
	@ObjectID int	
)
RETURNS varchar(max)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result varchar(max) = ''
	DECLARE @NL varchar(2) = CHAR(13) + CHAR(10)
	DECLARE @TAB varchar(1) = CHAR(9)
	DECLARE @View bit = 0
	IF EXISTS(SELECT null FROM sys.Views WHERE OBJECT_ID = @OBJECTID)
		SET @View = 1


	-- Add the T-SQL statements to compute the return value here
	SELECT @Result += CASE WHEN is_primary_key = 0 and is_unique_Constraint = 0 then 'CREATE ' + 
					CASE WHEN ind.is_unique = 1 or @View =1 AND type_desc = 'CLUSTERED' then 'UNIQUE ' else '' end 
					+ ind.type_desc 
					+ ' INDEX [' + ind.Name + '] ON [' + OBJECT_SCHEMA_NAME(ind.Object_ID) + '].[' + OBJECT_NAME(ind.object_Id) + ']'
					ELSE 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(ind.Object_ID) + '].[' + OBJECT_NAME(ind.object_Id) + '] ADD CONSTRAINT [' + ind.Name + '] '
					+ CASE WHEN is_primary_Key = 1 then 'PRIMARY KEY ' else 'UNIQUE ' end 
					+ CASE WHEN type_desc = 'CLUSTERED' then 'CLUSTERED' else 'NONCLUSTERED' end
					END
					+ @NL + '('  + @NL + @TAB				
					+ STUFF((SELECT @TAB + ', [' +  Name COLLATE Latin1_General_CI_AI + ']' + CASE WHEN is_descending_key = 1 then ' DESC' else ' ASC' end + @NL [text()]	
					FROM  sys.index_columns i
					JOIN sys.columns c
					ON i.object_id = c.Object_id
					AND i.column_id = c.column_id
					WHERE index_id = ind.index_id and i.object_id = ind.object_ID
					AND is_included_column = 0
					ORDER BY index_column_id ASC
					FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,3,'')			
				+ ')' 			
				+ CASE WHEN NOT EXISTS(SELECT null FROM sys.index_columns WHERE OBJECT_ID = ind.object_id AND index_id = ind.index_id AND is_included_column = 1) then ''
				ELSE @NL + 'INCLUDE (' + @NL + @TAB + STUFF((SELECT @TAB + ', [' +  Name COLLATE Latin1_General_CI_AI + ']' + @NL  [text()]
					FROM  sys.index_columns i
					JOIN sys.columns c
					ON i.object_id = c.Object_id
					AND i.column_id = c.column_id
					WHERE index_id = ind.index_id and i.object_id = ind.object_ID
					AND is_included_column = 1
					ORDER BY index_column_id ASC
					FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,3,'')
					+ ')' end  COLLATE Latin1_General_CI_AI
				+ CASE WHEN has_filter =1 then @NL + @TAB + 'WHERE ' + FILTER_DEFINITION + @NL ELSE '' END
				+ CASE WHEN is_unique_Constraint  = 1 then '' else
				+ 'WITH (PAD_INDEX = ' + CASE WHEN is_padded = 1 then 'ON' else 'OFF' end 
				+ ', STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = ON  ' 
				+ Case WHEN @@Version like '%Enterprise%' AND ind.is_unique = 0 THEN ', ONLINE=ON ' ELSE '' END
				+ Case WHEN @@Version like '%Enterprise%' THEN ', DATA_COMPRESSION = PAGE ' ELSE '' END
				+ CASE WHEN is_primary_key = 1 then '' else ', DROP_EXISTING = OFF ' end
				 + ', ALLOW_ROW_LOCKS = ' + CASE WHEN allow_row_locks = 1 then 'ON' else 'OFF' end 
				+ ', ALLOW_PAGE_LOCKS = ' + CASE WHEN allow_page_locks = 1 then 'ON' else 'OFF' end  
				+ CASE WHEN fill_factor <> 0 then ', FILLFACTOR = ' + CAST(fill_factor as varchar) else '' end 
				+ ') ON [PRIMARY]' end
				+ @NL + 'GO'
			FROM sys.indexes ind WITH (NOLOCK) 
			WHERE ind.Name = @IndexName
			AND ind.Object_ID = @OBJECTID

	-- Return the result of the function
	RETURN @Result

END


-- ufn_CheckEnvironment