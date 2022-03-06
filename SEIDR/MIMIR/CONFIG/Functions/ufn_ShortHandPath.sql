CREATE FUNCTION [CONFIG].[ufn_ShortHandPath]
(
	@FilePath varchar(500),
	@OrganizationID int,
	@ProjectID int,
	@UserKey varchar(50), 
	@LoadProfileID int
)
RETURNS varchar(500)
AS
BEGIN
	SELECT @FilePath = REPLACE(REPLACE(@FilePath, '#ORGANIZATION#', '#ORGANIZATION\#'), '#PROJECT#', '#PROJECT\#')

	IF @FilePath LIKE '#DAILY%' OR @FilePath LIKE '#PREPROCESS%' OR @FilePath LIKE '#MASTER%'
		SET @FilePath = '#METRIX\' + @FilePath

	SELECT @FilePath = REPLACE(@FilePath, '#ORGANIZATION', 
											COALESCE(o.Metrix_RootFolderName, REPLACE(UTIL.ufn_CleanField(o.Description), ' ', ''))
											)
	FROM REFERENCE.Organization o
	WHERE OrganizationID = @OrganizationID

	DECLARE @FilePath2 varchar(500)
	IF @UserKey NOT LIKE '%\' 
		SET @UserKey = @UserKey + '\'
	DECLARE @Profile varchar(50) = CONVERT(varchar(30), @LoadProfileID) + '\'	
	SELECT @FilePath2 = 				
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(REPLACE(
						REPLACE(@FilePath, '#FTP\', [#FTP]), '#FTP', [#FTP]),
											'#SOURCE\', [#SOURCE]), '#Source', [#SOURCE]),
											'#METRIX\', [#METRIX]), '#METRIX', [#METRIX]),
											'#PRODUCTION\', [#METRIX]), '#PRODUCTION', [#METRIX]),
											'#PROCLAIM\', [#PROCLAIM]), '#PROCLAIM', [#PROCLAIM]),
											--'#SANDBOX\', [#SANDBOX]), '#SANDBOX', [#SANDBOX]),
											--'#UAT\', [#UAT]), '#UAT', [#UAT]),
											'#EXPORT\', #EXPORT), '#EXPORT', [#EXPORT]),
											'#PROJECT', COALESCE(f.Project, '')),
											'#PREPROCESS\', 'Daily_Loads\Preprocessing\'), '#PREPROCESS', 'Daily_Loads\Preprocessing\'),
											'#DAILY\', 'Daily_Loads\'), '#DAILY', 'Daily_Loads\'),
											'#MASTER\', 'Master_Loads\'), '#MASTER', 'Master_Loads\')
	FROM REFERENCE.vw_Organization_Folder f	
	WHERE f.OrganizationID = @OrganizationID
	AND (f.ProjectID IS NULL AND @ProjectID IS NULL
		OR @ProjectID = f.ProjectID)
	IF @UserKey is not null
		SET @FilePath2 = REPLACE(REPLACE(@FilePath2, '#KEY\', @UserKey), '#KEY', @UserKey)
	IF @Profile IS NOT NULL
		SET @FilePath2 = 
						REPLACE(REPLACE(
						REPLACE(REPLACE(@FilePath2, '#LOADPROFILEID\', @Profile), '#LOADPROFILEID', @Profile),
													'#PROFILEID\', @Profile), '#PROFILEID', @Profile)
	RETURN @FilePath2
END
