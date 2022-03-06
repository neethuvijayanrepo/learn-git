CREATE FUNCTION [SEIDR].[ufn_GetJobProfile_JobID]
(
	@JobProfileID int,
	@StepNumber smallint,
	@ExecutionStatusCode varchar(2),
	@ExecutionNameSpace varchar(128),
	@TriggerBranch varchar(30) -- int
)
RETURNS INT
AS
BEGIN
	DECLARE @isError bit = 0
	SELECT @IsError = IsError
	FROM SEIDR.ExecutionStatus WITH (NOLOCK)
	WHERE ExecutionStatusCode = @ExecutionStatusCode
	AND [NameSpace] = @ExecutionNameSpace

	DECLARE @JobProfile_JobID int
	SELECT TOP 1 @JobProfile_JobID = JobProfile_JobID
	FROM SEIDR.JobProfile_Job
	WHERE JobProfileID = @JobProfileID
	AND Active = 1
	AND (
		TriggerExecutionNameSpace is null AND @isError = 0 
		or @ExecutionNameSpace = TriggerExecutionNameSpace
		)
	AND (
		TriggerExecutionStatusCode is null AND @isError = 0 
		or @ExecutionStatusCode = TriggerExecutionStatusCode
		)
	AND StepNumber = @StepNumber
	-- If null, can trigger from Any branch. Else, trigger off specific branch only.
	AND (TriggerBranch is null	or TriggerBranch = @TriggerBranch)	
	ORDER BY TriggerBranch desc, 
			TriggerExecutionNameSpace desc, 
			TriggerExecutionStatusCode desc,
			CASE				
				-- Try to maintain current branch if multiple trigger matches. 
				-- Note: @TriggerBranch should not be null, based on JobExecution.Branch, which is not nullable.
				WHEN Branch = @TriggerBranch then 1 
				else 0 
				end desc 

	RETURN @JobProfile_JobID
END
