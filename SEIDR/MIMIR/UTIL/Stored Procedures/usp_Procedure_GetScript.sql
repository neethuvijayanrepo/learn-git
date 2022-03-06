
CREATE PROCEDURE [UTIL].[usp_Procedure_GetScript]
	@ProcObjectID int,
	@ConfigurationObject varchar(260),
	@KeyPhrase nvarchar(1000),
	@SQL varchar(6000) out,
	@Debug bit = 0,
	@DropSettingParameters bit = 0
AS
BEGIN	
	SET NOCOUNT ON
	-- DECLARE @ProcObjectID int = OBJECT_ID('SEIDR.usp_JObProfile_LoaderJob_iu'), @SQL varchar(6000), @ConfigurationObject varchar(260) = 'SEIDR.vw_LoaderJob', @KeyPhrase nvarchar(1000) = 'JobProfile_JobID = 500'
	SET @SQL = 'exec ' + QUOTENAME(OBJECT_SCHEMA_NAME(@ProcObjectID)) + '.' + QUOTENAME(OBJECT_NAME(@ProcObjectID)) + CHAR(13) + CHAR(10) + CHAR(9)
	DECLARE @VO int = OBJECT_ID(@ConfigurationObject)
	DECLARE @ParmCount int
	SELECT @ParmCount = COUNT(*)
	FROM sys.parameters 
	WHERE OBJECT_ID = @ProcObjectID
	
	DECLARE @SupportShortHand bit = 0
	IF OBJECT_DEFINITION(@ProcObjectID) LIKE '%ufn_ShortHandPath%'
	BEGIN
		SET @SupportShortHand = 1
		IF @Debug = 1
			RAISERROR('ShortHand Support Enabled', 0, 0)
	END
	ELSE IF @Debug = 1
		RAISERROR('ShortHand Support Disabled', 0, 0)
	DECLARE @parmName varchar(128), @ParmType varchar(30), @Length smallint, @Precision tinyint, @Scale tinyint, @ParameterID tinyint
	DECLARE c_Parameters CURSOR LOCAL FAST_FORWARD
	FOR 
	SELECT Name, ParameterType, Max_Length, Precision, Scale, Parameter_ID
	FROM UTIL.vw_ProcedureInfo	
	WHERE OBJECT_ID = @ProcObjectID
	ORDER BY Parameter_ID ASC

	DECLARE @parmSQL nvarchar(2000)
	DECLARE @parmSQLParameters nvarchar(1000) = '@parmDesc varchar(500) output'

	OPEN c_Parameters
	FETCH NEXT FROM C_Parameters INTO @ParmName, @ParmType, @Length, @Precision, @Scale, @parameterID
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @Column nvarchar(128) = QUOTENAME(SUBSTRING(@ParmName,2, 128))
		DECLARE @ShortHandColumn nvarchar(128) = QUOTENAME('SHORTHAND' + SUBSTRING(@ParmName,2, 128))	
		IF @SupportShortHand = 0 
		OR NOT EXISTS(SELECT null FROM sys.columns WHERE OBJECT_ID = @VO AND QUOTENAME(name) = @ShortHandColumn)
		BEGIN
			SET @ShortHandColumn = null
		END
		IF NOT EXISTS(SELECT null FROM sys.columns WHERE OBJECT_ID = @VO AND QUOTENAME(Name) = @Column)
		BEGIN			
			IF @ShortHandColumn is not null
			BEGIN
				IF @Debug = 1	
					RAISERROR('Replace Original Column (%s) with shorthand version (%s)', 0, 0, @Column, @ShortHandColumn)
				SET @Column = @ShortHandColumn
				SET @ShortHandColumn = null				
			END
			ELSE
			BEGIN
				IF @ParmName = '@SafetyMode'
					SELECT @SQL += @ParmName + ' = 1' --Default safety mode to true
				else if @DropSettingParameters = 1
				BEGIN				
					FETCH NEXT FROM C_Parameters INTO @ParmName, @ParmType, @Length, @Precision, @Scale, @parameterID
					CONTINUE --Skip.
				END
				ELSE
					SELECT @SQL += @ParmName + ' = ????????'

				IF @parameterID < @ParmCount
					SELECT @SQL += ',' + CHAR(13) + CHAR(10) + CHAR(9)
				FETCH NEXT FROM C_Parameters INTO @ParmName, @ParmType, @Length, @Precision, @Scale, @parameterID
				CONTINUE
			END
		END

		DECLARE @parmDesc varchar(500) = null
		IF @ParmType LIKE '%int' OR @ParmType = 'bit'
		BEGIN
			IF @parmName LIKE '%ID'
			BEGIN
				DECLARE @FKCheck varchar(130) = SUBSTRING(@parmName, 1, LEN(@ParmName) - 2)
				IF EXISTS(SELECT null 
							FROM UTIL.vw_ProcedureInfo
							WHERE OBJECT_ID = @ProcObjectID
							AND [Name] = @FKCheck)
				BEGIN
					RAISERROR('Skipping Foreign Key Identity - Description is available as a parameter.', 0, 0)
					
					FETCH NEXT FROM C_Parameters INTO @ParmName, @ParmType, @Length, @Precision, @Scale, @parameterID
					CONTINUE
				END
			END

			SET @ParmSQL = N'SELECT @ParmDesc = CONVERT(varchar(32), ' + @Column + ')'		
		END
		IF @ParmType LIKE '%Char'
		BEGIN
			IF @ShortHandColumn IS NOT NULL
			BEGIN
				IF @Debug =1 
					RAISERROR('Using ShortHand Column - "%s"', 0, 0, @ShortHandColumn)
				SET @ParmSQL = N'SELECT @ParmDesc = ' + @ShortHandColumn + ' + '''''',	--'''''' + ' + @Column
			END
			ELSE
				SET @ParmSQL = N'SELECT @ParmDesc = ' + @Column
		END
		ELSE
		BEGIN
			SET @ParmSQL = N'SELECT @ParmDesc = CONVERT(varchar(500), ' + @Column + ')'
		END
		SET @ParmSQL += N' FROM ' + @ConfigurationObject + N' WITH (NOLOCK) WHERE (' + @KeyPhrase + ')'
		
		exec sp_executeSql @ParmSQL, @ParmSQLParameters, @ParmDesc = @parmDesc out
		--SELECT @ParmDesc, @Column
		IF @ParmDesc is not null
		BEGIN
			IF @ParmType LIKE '%int' OR @ParmType = 'bit'
				SET @SQL += @ParmName + ' = ' + @ParmDesc 
			ELSE
				SET @SQL += @ParmName + ' = ''' + @ParmDesc + ''''
		END
		ELSE
			SET @SQL += @ParmName + ' = NULL'
		IF @parameterID < @ParmCount
			SELECT @SQL += ',' + CHAR(13) + CHAR(10) + CHAR(9)
		FETCH NEXT FROM C_Parameters INTO @ParmName, @ParmType, @Length, @Precision, @Scale, @parameterID
	END
	CLOSE c_Parameters
	DEALLOCATE c_Parameters

	--SELECT @SQL

END