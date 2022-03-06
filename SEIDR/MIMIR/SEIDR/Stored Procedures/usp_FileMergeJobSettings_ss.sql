CREATE PROCEDURE [SEIDR].[usp_FileMergeJobSettings_ss]
	@JobProfile_JobID int
AS
	SELECT [JobProfile_JobID], 
	[MergeFile], 
	[OutputFilePath], 
	[Overwrite], 
	[InnerJoin], 
	[LeftJoin], 
	[CaseSensitive], 
	[PreSorted], 
	KeepDelimiter, HasTextQualifier,
	RemoveExtraMergeColumns, RemoveDuplicateColumns,
	[LeftInputHasHeader], [RightInputHasHeader], 
	[IncludeHeader], 
	[LeftKey1], [RightKey1], 
	[LeftKey2], [RightKey2], 
	[LeftKey3], [RightKey3] 
	FROM SEIDR.FileMergeJob
	WHERE JobProfile_JobID = @JobProfile_JobID
RETURN 0
