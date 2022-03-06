
CREATE FUNCTION CONFIG.ufn_GetShortHandPath
(
	@FilePath varchar(500),
	@OrganizationID int,
	@ProjectID int,
	@UserKey varchar(50),
	@LoadProfileiD int
)
RETURNS VARCHAR(500)
AS
BEGIN
	DECLARE @Profile varchar(30) = COALESCE(CONVERT(varchar(30), @LoadProfileID), '')

	SELECT @FilePath 
	= REPLACE(REPLACE(REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(
			REPLACE(REPLACE(
			REPLACE(@FilePath, 
						@Profile, '#PROFILEID'),
						#Source, '#SOURCE\'),
						#FTP, '#FTP\'),
						#METRIX, '#METRIX\'),
						#EXPORT, '#EXPORT\'),
						#PROCLAIM, '#PROCLAIM\'),
						'DAILY_LOADS\Preprocessing\', '#PREPROCESS\'),
						'DAILY_LOADS\', '#DAILY\'),
						'MASTER_LOADS\', '#MASTER\'),
						--'Preprocessing\', '#PREPROCESS\'),						
						'_' + @UserKey+ '\', '_#KEY\'),
						'\' + @UserKey+ '\', '\#KEY\'),
						'#METRIX\#PREPROCESS', '#PREPROCESS'), '#METRIX\#DAILY', '#DAILY'), '#METRIX\#MASTER', '#MASTER')	
	FROM REFERENCE.vw_Organization_Folder f	
	WHERE f.OrganizationID = @OrganizationID
	AND (f.ProjectID IS NULL AND @ProjectID IS NULL
		OR @ProjectID = f.ProjectID)

	RETURN @FilePath
END
