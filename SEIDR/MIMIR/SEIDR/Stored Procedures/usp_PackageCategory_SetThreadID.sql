CREATE PROCEDURE [SEIDR].[usp_PackageCategory_SetThreadID]
	@PackageCategory varchar(128),
	@ThreadID int
AS
	UPDATE jpj
	SET RequiredThreadID = @ThreadID
	FROM SEIDR.JobProfile_Job jpj
	JOIN SEIDR.LoaderJob lj
		ON jpj.JobProfile_JobID = lj.JobProfile_JobID
	JOIN SEIDR.SSIS_Package p
		ON lj.PackageID = p.PackageID
	WHERE jpj.Active = 1
	AND p.Category = @PackageCategory

RETURN 0
