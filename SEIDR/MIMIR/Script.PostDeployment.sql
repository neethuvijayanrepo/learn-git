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

:r .\SEIDR\ChangeScripts\FTP.sql
:r .\SEIDR\ChangeScripts\Service.sql
GO
:r ".\SEIDR\ChangeScripts\PGPOperation Insert.sql"

go
:r .\SEIDR\ChangeScripts\Schedule.sql

GO
:r .\SEIDR\ChangeScripts\ExecutionStatus_for_Pending_Details.sql

GO
:r .\SEIDR\ChangeScripts\ExecutionStatus_for_Metrix_Export_C.sql