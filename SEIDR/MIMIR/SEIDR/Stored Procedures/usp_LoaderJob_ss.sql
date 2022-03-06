CREATE PROCEDURE [SEIDR].[usp_LoaderJob_ss] 
	@JobProfile_JobID int
AS
BEGIN
	DECLARE @ProjectID int

	SELECT @ProjectID = ProjectID
	FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
	JOIN SEIDR.JobProfile jp WITH (NOLOCK)
		ON jpj.JobProfileID = jp.JobProfileID
	WHERE jpj.JobProfile_JobID = @JobProfile_JobID

	SELECT LJ.[JobProfile_JobID]
		  ,LJ.[ServerInstanceName]
		  ,LJ.[AndromedaServer]
		  ,LJ.[OutputFolder]
		  ,LJ.[PackageID]
		  --,LJ.[CB]
		  --,LJ.[IsValid]
		  ,PKG.[Category]
		  ,PKG.[Name]
		  ,PKG.[ServerName]
		  ,PKG.[PackagePath]
		  ,LJ.[FacilityID]
		  ,@ProjectID as ProjectID
		  ,LJ.OutputFileName
		  ,@@SERVERNAME as [SEIDR_ServerInstanceName]
		  ,LJ.DatabaseName

		  ,LJ.DatabaseConnectionManager
    	  ,LJ.DatabaseConnection_DatabaseLookupID
		  , LJ.Misc
		  , LJ.Misc2
		  , LJ.Misc3
		  , LJ.SecondaryFilePath
		  , LJ.TertiaryFilePath
	FROM [SEIDR].[LoaderJob] LJ
	INNER JOIN [SEIDR].[SSIS_Package] PKG 
		ON LJ.PackageID = PKG.PackageID
	WHERE LJ.[JobProfile_JobID]=@JobProfile_JobID 
	AND LJ.[IsValid]=1
END
GO
