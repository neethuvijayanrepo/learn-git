CREATE PROCEDURE [SEIDR].[usp_JobProfile_FileValidationJob_MarkMetaDataRefresh]
	@JobProfileID int,
	@StepNumber tinyint,
	
	
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null
AS
BEGIN
	SET XACT_ABORT ON
	
	
	DECLARE @JobID int, @JobProfile_JobID int
	SELECT @JobID = JobID
	FROM SEIDR.Job 
	WHERE JobName = 'FileValidationJob' 
	AND JobNameSpace = 'FileSystem'

	SELECT TOP 1 @JobProfile_JobID = JobProfile_JobID
	FROM SEIDR.JobProfile_Job 
	WHERE JobProfileiD = @JobProfileiD
	AND stepNumber = @StepNumber
	AND (@TriggerExecutionStatus is null or TriggerExecutionStatusCode = @TriggerExecutionStatus)
	AND (@TriggerExecutionNameSpace is null or TriggerExecutionNameSpace = @TriggerExecutionNameSpace)
	ORDER BY CASE WHEN @TriggerExecutionStatus is null then TriggerExecutionStatusCode end desc,
		TriggerExecutionStatusCode asc
	
	IF @@ROWCOUNT = 0 OR @@ERROR <> 0
	BEGIN
		RAISERROR('Could not identify Step.', 16, 1)
		RETURN
	END
	
	
	UPDATE SEIDR.FileValidationJob
	SET DoMetaDataConfiguration = 1
	WHERE JobProfile_JobID = @JobProfile_JobID
	

	SELECT * 
	FROM SEIDR.vw_FileValidationJob
	WHERE JobProfile_JobID = @JobProfile_JobID
END
GO

