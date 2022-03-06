CREATE PROCEDURE [REFERENCE].[usp_Organization_Contact_i_Bulk]
	@OrganizationID int,
	@UserKeyList varchar(2500),
	@ContactID int,
	@ProjectID smallint = null,
	@FromDate date = null,
	@ThroughDate date = null,
	@UserName varchar(260) = null,
	@KeyListDelimiter varchar(5) = ','
AS
	DECLARE @TranCount int = @@TRANCOUNT
	SET NOCOUNT ON
	SET XACT_ABORT ON
	BEGIN TRY
		BEGIN TRAN
		IF @fromDate is null
			SET @FromDate = CONVERT(date, GETDATE())
		
		DECLARE @userKeyTable UTIL.udt_Varchar500
		INSERT INTO @userKeyTable
		SELECT * 
		FROM [UTIL].[ufn_SplitVarchar500](@KeyListDelimiter, @UserKeyList)

		DELETE k
		FROM @userKeyTable k
		WHERE EXISTS(SELECT null 
						FROM REFERENCE.Organization_Contact
						WHERE ContactID = @ContactID
						AND organizationID = @OrganizationID
						AND ISNULL(@ProjectID, 0) = ISNULL(ProjectID, 0)
						AND UserKey = k.[Value])	


		INSERT INTO REFERENCE.Organization_Contact(OrganizationID, ProjectID, UserKey, 
				ContactID, DC, FromDate, ThroughDate)
		SELECT distinct @OrganizationID, @ProjectID, k.[Value], 
				@ContactID, GETDATE(), @FromDate, @ThroughDate
		FROM @userKeyTable k

		IF @@ROWCOUNT = 0
		BEGIN
			RAISERROR('No Contact links inserted.', 16, 1)
			RETURN
		END
	
	
	
		DECLARE @autoMessage varchar(2000)
		IF @ProjectID IS NOT NULL
		BEGIN
			SELECT @autoMessage = 'Added Contact to Organization "' + o.Organization + '"'
				+ CASE WHEN @ProjectID IS NOT NULL THEN ', Project "' + o.Project + '"' else '' end		
			FROM REFERENCE.vw_Project_Organization o
			WHERE OrganizationID = @OrganizationID
			AND ProjectID = @ProjectID
		END
		ELSE
			SELECT @autoMessage = 'Added Contact to Organization "' + o.Description + '"'
			FROM REFERENCE.Organization o
			WHERE OrganizationID = @OrganizationID

		IF @UserName is null
			SET @UserName = SUSER_NAME()
		ELSE IF @UserName <> SUSER_NAME()
			SET @userName += '(' + SUSER_NAME() + ')'

		DECLARE @DateMessage varchar(2000) = '"
	FromDate: ' + CONVERT(varchar(20), @FromDate, 0)
		IF @ThroughDate is not null
			SET @DateMessage += '
	ThroughDate: ' + CONVERT(varchar(20), @ThroughDate, 0)

		INSERT INTO REFERENCE.ContactNote(ContactID, NoteText, Auto, Author)
		SELECT distinct @ContactID, @autoMessage + ', UserKey = "' + k.[Value] + @DateMessage, 1, @UserName
		FROM @userKeyTable k
		
		COMMIT

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > @TranCount
			ROLLBACK
		;throw
	END CATCH
RETURN 0
