
CREATE PROCEDURE [REFERENCE].[usp_JobProfile_u_GetConfigurationScript]
	@JobProfileID int
AS
BEGIN
	SET NOCOUNT ON
	--	DECLARE @JobProfileID int =495
	DECLARE @KeyPhrase nvarchar(1000) = N'JobProfileID = ' + CONVERT(nvarchar(30), @JobProfileID)
	DECLARE @Desc varchar(500) = 'SEIDR.usp_JobProfile_u', @SQL varchar(6000)
	
	DECLARE @ProcID int = OBJECT_ID(@Desc)
	exec UTIL.usp_Procedure_GetScript
		@ProcObjectID = @ProcID,
		@ConfigurationObject = 'SEIDR.vw_JobProfile',
		@KeyPhrase = @KeyPhrase,
		@SQL = @SQL out
	

	DECLARE @outSQL nvarchar(500) = 'SELECT @SQL as ' + QUOTENAME(@Desc)
	execute sp_executesql @OutSql, N'@SQL varchar(6000)', @SQL= @SQL

END