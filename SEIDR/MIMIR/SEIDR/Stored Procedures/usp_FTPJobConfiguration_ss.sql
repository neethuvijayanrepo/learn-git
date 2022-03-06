CREATE PROCEDURE [SEIDR].[usp_FTPJobConfiguration_ss]   
 @JobProfile_JobID int  
AS  
BEGIN  
  
	SELECT   
	[FTP].[FTPJobID]  
	,[FTP].[JobProfile_JobID]  
	,[FTP].[FTPAccountID]  
	,[FTP].[FTPOperationID]  
	,[FTP].[LocalPath]  
	,[FTP].[RemotePath]  
	,[FTP].[RemoteTargetPath]
	,[FTP].[Overwrite]  
	,[FTP].[Delete]  
	,[FTP].[DateFlag]  
	,[FTP].[Active]  
	,[AC].[FTPProtocolID]  
	,[AC].[Server]  
	,[AC].[UserName]  
	,[AC].[Password]  
	,[AC].[Port]  
	,[AC].[Passive]  
	,[AC].[Fingerprint]  
	,[AC].[PpkFileName]  
	,[OP].[Operation]  
	,[OP].[OperationName]  
	,[PR].Protocol
	,[AC].[TransferResumeSupport]
	FROM [SEIDR].[FTPJob] FTP 
	INNER JOIN [SEIDR].[FTPAccount] AC 
		on FTP.FTPAccountID = AC.FTPAccountID 
		AND ac.DD IS NULL 
		AND ac.OrganizationID IS NOT NULL
	INNER JOIN [SEIDR].[FTPProtocol] PR  
		ON PR.[FTPProtocolID] = AC.[FTPProtocolID] 
	INNER JOIN [SEIDR].[FTPOperation] OP 
		ON [OP].[FTPOperationID] = [FTP].[FTPOperationID]  
	WHERE [FTP].[JobProfile_JobID] = @JobProfile_JobID  
	AND FTP.Active = 1

	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('Invalid/Inactive FTP Job Configuration. Make sure that the FTP Account is valid.', 16, 1)
	END
  
END