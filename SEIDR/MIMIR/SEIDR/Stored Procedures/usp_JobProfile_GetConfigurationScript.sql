CREATE PROCEDURE [SEIDR].[usp_JobProfile_GetConfigurationScript]
	@JobProfileID int,
	@DropSettingParameters bit = 0,
	@VariableProfileID bit = 0
AS
BEGIN
	SET NOCOUNT ON
	--	DECLARE @JobProfileID int = 7 -- 4
	DECLARE @KeyPhrase nvarchar(1000) = N'JobProfileID = ' + CONVERT(nvarchar(30), @JobProfileID)
	DECLARE @Desc varchar(500) = 'SEIDR.usp_JobProfile_i', @SQL varchar(6000)
	
	DECLARE @ProcID int = OBJECT_ID(@Desc)
	exec UTIL.usp_Procedure_GetScript
		@ProcObjectID = @ProcID,
		@ConfigurationObject = 'SEIDR.vw_JobProfile',
		@KeyPhrase = @KeyPhrase,
		@SQL = @SQL out,
		@DropSettingParameters = @DropSettingParameters
	
	IF @VariableProfileID = 1
		SELECT @SQL = REPLACE(@SQL, 'EXEC', 'EXEC @JobProfileID = ')
	DECLARE @outSQL nvarchar(500) = 'SELECT @SQL as ' + QUOTENAME(@Desc)
	execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL

	DECLARE @JobProfile_JobID int, @StepNumber int
	DECLARE StepCursor CURSOR FAST_FORWARD
	FOR 
	SELECT JobProfile_JobID, StepNumber
	FROM SEIDR.JobProfile_Job
	WHERE JobProfileID = @JobProfileID
	AND Active = 1
	ORDER BY StepNumber asc
	OPEN StepCursor
	
	FETCH NEXT FROM StepCursor INTO @JobProfile_JobID, @StepNumber
	WHILE @@FETCH_STATUS = 0
	BEGIN
		exec SEIDR.usp_JobProfile_Job_GetConfigurationScript @JobProfile_JobID = @JobProfile_JobID, @DropSettingParameters = @DropSettingParameters, @VariableJobProfileID = @VariableProfileID
		FETCH NEXT FROM StepCursor INTO @JobProfile_JobID, @StepNumber	
	END
	CLOSE StepCursor
	DEALLOCATE StepCursor

	DECLARE @Replace varchar(70) = '@JobProfileID = ' + CONVERT(varchar(70), @JobProfileID)
	DECLARE @ReplaceNew varchar(70) = '@JobProfileID = @JobProfileID'

	IF EXISTS(SELECT null FROM SEIDR.JObProfile WHERE JobProfileID = @JobProfileID AND RegistrationValid = 1)
	BEGIN
		SET @Desc = 'Registration Info'
		SET @ProcID = OBJECT_ID('SEIDR.usp_JobProfile_SetRegistrationInfo')
		exec UTIL.usp_Procedure_GetScript
			@ProcObjectID = @ProcID,
			@ConfigurationObject = 'SEIDR.vw_JobProfile',
			@KeyPhrase = @KeyPhrase,
			@SQL = @SQL out,
			@DropSettingParameters = @DropSettingParameters
		
		IF @VariableProfileID = 1
			SELECT @SQL = REPLACE(@SQL, @Replace, @ReplaceNew)
		SET @outSQL = 'SELECT @SQL as ' + QUOTENAME(@Desc)
		execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL
	END
	IF EXISTS(SELECT null FROM SEIDR.JObProfile WHERE JobProfileID = @JobProfileID AND ScheduleID is not null)
	BEGIN
		SET @Desc = 'Schedule Info'
		SET @ProcID = OBJECT_ID('SEIDR.usp_JobProfile_SetSchedule')
		exec UTIL.usp_Procedure_GetScript
			@ProcObjectID = @ProcID,
			@ConfigurationObject = 'SEIDR.vw_JobProfile',
			@KeyPhrase = @KeyPhrase,
			@SQL = @SQL out,
			@DropSettingParameters = @DropSettingParameters

		
		IF @VariableProfileID = 1
			SELECT @SQL = REPLACE(@SQL, @Replace, @ReplaceNew)

		SET @outSQL = 'SELECT @SQL as ' + QUOTENAME(@Desc)
		execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL
	END
	IF EXISTS(SELECT null FROM SEIDR.vw_JobProfile_Parent WHERE JobPRofileiD = @JobProfileID)
	BEGIN
		SET @Desc = 'Step Parenting'
		SET @ProcID = OBJECT_ID('SEIDR.usp_JobProfile_SetParentStep')

		DECLARE parentSequence cursor FAST_FORWARD
		FOR
		SELECT 'JobProfile_Job_ParentID = ' + CONVERT(varchar(30), JobProfile_Job_ParentID),
				'Step #' + CONVERT(varchar(30), StepNumber) + ' - PARENTING'
		FROM SEIDR.vw_JobProfile_Parent
		WHERE JobProfileID = @JobProfileID

		OPEN parentSequence
		FETCH NEXT FROM parentSequence INTO @KeyPhrase, @Desc
		WHILE @@FETCH_STATUS = 0
		BEGIN			
			exec UTIL.usp_Procedure_GetScript
				@ProcObjectID = @ProcID,
				@ConfigurationObject = 'SEIDR.vw_JobProfile_Parent',
				@KeyPhrase = @KeyPhrase,
				@SQL = @SQL out,
				@DropSettingParameters = @DropSettingParameters
				
			IF @VariableProfileID = 1
				SELECT @SQL = REPLACE(@SQL, @Replace, @ReplaceNew)
			SET @outSQL = 'SELECT @SQL as ' + QUOTENAME(@Desc)
			execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL
			
			FETCH NEXT FROM parentSequence INTO @KeyPhrase, @Desc
		END
		CLOSE parentSequence
		DEALLOCATE parentSequence
		
	END
END