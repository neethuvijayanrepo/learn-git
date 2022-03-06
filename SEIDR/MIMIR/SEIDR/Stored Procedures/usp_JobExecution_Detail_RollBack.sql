CREATE PROCEDURE [SEIDR].[usp_JobExecution_Detail_RollBack] 
@JobExecution_ExecutionStatusID int,
@rollback_query VARCHAR(max)=null
AS
BEGIN
	SELECT @rollback_query=[RollbackCommand]
	from [SEIDR].[vw_JobExecution_Rollback_Helper] 
	where JobExecution_ExecutionStatusID=@JobExecution_ExecutionStatusID

	SET @rollback_query = RIGHT(@rollback_query, LEN(@rollback_query) - 5) ;
	
	EXECUTE (@rollback_query);
	SET @Rollback_query = null --No reason to provide this to the UI.
END