CREATE PROCEDURE [SEIDR].[usp_FileSystem_ss_JobExecution]
	@JobProfile_JobID int
AS
	SELECT *, [OutputPath] as Destination
	FROM SEIDR.FileSystemJob FS 
	left outer join SEIDR.DatabaseLookup DL 
		on FS.DatabaseLookUpID = DL.DatabaseLookupID
	WHERE JobProfile_JobID = @JobProfile_JobID
	AND Active = 1
RETURN 0


