/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
--ToDo: Remove Operation and (maybe) DD columns. Can't think of a good reason to disable an FTPOperation from the database...
-- Maybe also FTPOperationID. Extra numeric values aren't useful - should just be using operation name, not values that came from BM
INSERT INTO [SEIDR].[FTPOperation](Operation, OperationName, Description)
  SELECT *
  FROM(
  VALUES
  (0, 'SEND', 'Sends a file from the local computer to the FTP server.'),
  (1, 'RECEIVE', 'Saves a file from the FTP server to the local computer.'),
  --(2, 'MAKE_DIR_LOCAL', 'DEPRECATED - Creates a directory on the local computer.'),
  (3, 'MAKE_DIR_REMOTE', 'Creates a directory on the FTP server.'),
  --(4, 'DELETE_LOCAL', 'DEPRECATED - Deletes a file on the local computer.'),
  --(5, 'DELETE_REMOTE  ', 'Deletes a file on the FTP server.'), -- No Reason to include Deprecated operations if they aren't there already.
  (6, 'SYNC_LOCAL', 'Changes from remote directory are applied to local directory'),
  (7, 'SYNC_REMOTE', 'Changes from the local directory are applied to the remote'),
  (8, 'SYNC_BOTH', 'Changes from both the local directory and remote directory are applied.'),
  (9, 'SYNC_REGISTER', 'SYNC_LOCAL, then use FileDateMask of profile to Register files to the same profile.
Should only be used from schedule (ExecutionStatusCode = ''S'', ExecutionNameSpace = ''SEIDR''). 
Should only be step #1. Higher steps must be for the uploads from syncing.'),
	(10, 'MOVE_REMOTE', 'Moves a file from "RemotePath" to the location specified by "RemoteTargetPath". RemoteTargetPath should be a full file path on the remote server.
See https://winscp.net/eng/docs/library_session_movefile')
  )C(Operation, OperationName, Descriptions)
  WHERE NOT EXISTS(SELECT null FROM [SEIDR].[FTPOperation] WHERE Operation = c.Operation)

  --Possible toDo: Remove DD from table? More likely to be useful than disabling an FTPOperation, anyway...
  INSERT INTO [SEIDR].[FTPProtocol](Protocol)
  SELECT *
  FROM(
  VALUES
  ('FTP'),
  ('SFTP'),
  ('FTPS - Explicit SSL'),
  ('FTPS - Explicit TLS'),
  ('FTPS - Implicit')
  )C(Protocol)
  WHERE NOT EXISTS(SELECT null FROM [SEIDR].[FTPProtocol] WHERE Protocol = c.Protocol)