CREATE FUNCTION [SEIDR].[ufn_CheckParentSchedule](@JobProfile_JobID int, @ProcessingDate datetime)  
RETURNS BIT
AS  
BEGIN  
 DECLARE @RET_VAL BIT = 1
  IF
  Exists(SELECT TOP 1 JP.JobProfile_JobID 
		 FROM SEIDR.vw_JobProfile_Job_Parent JP WITH(NOLOCK)
		 WHERE JP.JobProfile_JobID=@JobProfile_JobID 
		 AND JP.ProcessingDate=@ProcessingDate 
		 AND JP.History_JobProfile_JobID IS NULL)
  BEGIN
  SET @RET_VAL = 0
  END
  
 RETURN @RET_VAL  
END