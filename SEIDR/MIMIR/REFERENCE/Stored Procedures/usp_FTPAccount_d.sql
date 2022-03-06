CREATE PROCEDURE REFERENCE.usp_FTPAccount_d
	@FTPAccountID int,
	@Delete bit =1,
	@SafetyMode bit = 1
AS
BEGIN
	IF @SafetyMode = 1
	AND EXISTS(SELECT null 
				FROM SEIDR.vw_FTPJob 
				WHERE FTPAccountID = @FTPAccountID)
	BEGIN
		SELECT *
		FROM SEIDR.vw_FTPJob 
		WHERE FTPAccountID = @FTPAccountID
		RAISERROR('FTPAccount is in use - deactivating or reactivating may have an immediate impact. Please pass @SafetyMode = 0 to continue.', 16, 1)
		RETURN
	END

	IF @Delete = 1
	BEGIN		
		UPDATE SEIDR.FTPaccount
		SET DD = COALESCE(DD, GETDATE())
		WHERE FTPAccountID = @FTPAccountID			
	END
	ELSE
	BEGIN
		UPDATE SEIDR.FTPAccount
		SET DD = null
		WHERE FTPAccountID = @FTPAccountID
	END
END