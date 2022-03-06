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

/*
	Note: 
	For a file in the same folder, you can call as either of the following, and it will be equivalent:
	:r ScheduleCheck.sql
	:r .\ScheduleCheck.sql
	:r ..\BuildTests\ScheduleCheck.sql
	:r ..\..\MIMIR\BuildTests\ScheduleCheck.sql

*/
:r ScheduleCheck.sql
