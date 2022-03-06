
CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_GetConfigurationScript]
	@JobProfile_JobID int,
	@DropSettingParameters bit = 0,
	@VariableJobProfileID bit = 0
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Replace varchar(70) = '@JobProfileID = '
	DECLARE @ReplaceNew varchar(70) = '@JobProfileID = @JobProfileID'
	--	DECLARE @JobProfile_JobID int = 511 --500
	DECLARE @SQL varchar(6000), @View varchar(256), @Desc varchar(150)
	DECLARE @ParmCount int, @VO int, @SPROC int
	SELECT  
		@ParmCount = COUNT(*), 
		--@SQL = QuotedProcedureName,
		@SPROC = j.OBJECT_ID,  
		@View = ConfigurationView, 
		@Desc = CONVERT(varchar, jpj.StepNumber) + ': ' + jpj.Description,
		@Replace += CONVERT(varchar(70), jpj.JobProfileID)
	FROM SEIDR.vw_Job_ConfigurationProcedure j
	JOIN SEIDR.JobProfile_Job jpj
		ON j.JobID = jpj.JobID
	WHERE jpj.JobProfile_JobID = @JobProfile_JobID
	GROUP BY j.OBJECT_ID, ConfigurationView, jpj.StepNumber, jpj.Description, jpj.JobProfileID
	
	SET @Desc += '(' + CONVERT(varchar, @JobProfile_JobID) + ')'
	--SELECT @SQL, @ParmCount, @View
	SET @VO = OBJECT_ID(@View)
	IF @VO is null
	BEGIN
		RAISERROR('JobProfile_JobID %d: No Configuration view found to get parameters for "%s"', 16, 1, @JobProfile_JobID, @SQL)
		RETURN
	END
	DECLARE @KeyPhrase nvarchar(1000) = N'JobProfile_JobID = ' + CONVERT(nvarchar(30), @JobProfile_JobID)
	DECLARE @outSQL nvarchar(500) = 'SELECT @SQL as ' + QUOTENAME(@Desc)
	IF @View IN ('SEIDR.vw_SpawnJob') -- 1-Many
	BEGIN
		DECLARE spawn_cursor CURSOR FAST_FORWARD
		FOR
		SELECT @KeyPhrase + ' AND SpawnJobID = ' + CONVERT(varchar(30), SpawnJobID)
		FROM SEIDR.SpawnJob
		WHERE Active = 1
		AND JobProfile_JobID = @JobProfile_JobID
		OPEN spawn_cursor
		FETCH NEXT FROM spawn_cursor INTO @KeyPhrase
		WHILE @@FETCH_STATUS = 0
		BEGIN
			exec UTIL.usp_Procedure_GetScript
			@ProcObjectID = @SPROC,
			@ConfigurationObject = @View,
			@KeyPhrase = @KeyPhrase,
			@SQL = @SQL out,
			@DropSettingParameters = @DropSettingParameters


			IF @VariableJobProfileID = 1
				SELECT @SQL = REPLACE(@SQL, @Replace, @ReplaceNew)

			execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL	
			FETCH NEXT FROM spawn_cursor INTO @KeyPhrase
		END
		CLOSE spawn_cursor
		DEALLOCATE spawn_cursor
	END
	ELSE
	BEGIN
		exec UTIL.usp_Procedure_GetScript
		@ProcObjectID = @SPROC,
		@ConfigurationObject = @View,
		@KeyPhrase = @KeyPhrase,
		@SQL = @SQL out,
		@DropSettingParameters = @DropSettingParameters
	
		IF @VariableJobProfileID = 1
			SELECT @SQL = REPLACE(@SQL, @Replace, @ReplaceNew)
		execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL
	END	
END