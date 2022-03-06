IF NOT EXISTS(SELECT null FROM REFERENCE.UserKey WHERE UserKey = 'LETTER')
BEGIN
	INSERT INTO REFERENCE.userKey(UserKey, Description, Inbound, Outbound, VendorSpecific)
	VALUES('LETTER', 'Metrix Letters to Vendors', 0, 1, 0)
END
GO
IF NOT EXISTS(SELECT Null FROM METRIX.ExportType WHERE Description = 'Statement Export')
BEGIN
	INSERT INTO METRIX.ExportType(Description)
	VALUES('Statement Export')
END
INSERT INTO METRIX.Vendor(Description)
SELECT *
FROM 
(
	SELECT 'TransUnion'
	UNION ALL 
	SELECT 'LiveVox'
    UNION ALL
    SELECT 'LexisNexis'
	UNION ALL
	SELECT 'Nordis'
)q(Vendor)
WHERE NOT EXISTS(SELECT Null FROM METRIX.Vendor WHERE Description = q.Vendor)
GO


DECLARE @SE_Notification varchar(300) = 'ryan.langer@guidehouse.com,elias.zeidy@guidehouse.com'
DECLARE @BI_SE_Notification varchar(300) = @SE_Notification +
	 ',clay.strople@guidehouse.com,anthony.bessire@guidehouse.com,tameeko.williams@guidehouse.com,tranisha.davis@guidehouse.com,omar.lopez.gorostieta@guidehouse.com'

exec SEIDR.usp_JobProfile_i @Description = 'Metrix Statement/Letter Generation', @OrganizationID = 0, @ProjectID = null, @LoadProfileID = null,
	@UserKey = 'LETTER', @ScheduleNoHistory = 1, @JobProfileID = 1098

--Run without ExportBatch from schedule
exec [SEIDR].[usp_JobProfile_ExportSettings_iu] @JobProfileID = 1098,
	@JobName = 'StatementXMLGenerationJob',
	@StepNumber = 1,
	@Description = 'Identify Projects to generate Self Pay Letters on',
	@MetrixDatabaseLookup = 'METRIX',
	@VendorName = 'NORDIS',
	@ExportType = 'Statement Export',
	@CanRetry = 0,
	@ThreadID = 6,
	@TriggerBranch = 'MAIN',
	@FailureNotificationMail = @SE_Notification 

--Run with ExportBatch after initial schedule execution.
exec [SEIDR].[usp_JobProfile_ExportSettings_iu] @JobProfileID = 1098,
	@JobName = 'StatementXMLGenerationJob',
	@StepNumber = 1,
	@Description = 'Generate self pay letter file for client',
	@ArchiveLocation = '\\ncimtxfls02\vendor_export_archive_prd\Nordis\',
	@MetrixDatabaseLookup = 'METRIX',
	@VendorName = 'NORDIS',
	@ExportType = 'Statement Export',
	@CanRetry = 0,
	@ThreadID = 6,
	@TriggerBranch = 'EXPORT',
	@FailureNotificationMail = @BI_SE_Notification

exec SEIDR.usp_JobProfile_ExportSettings_iu	
	@JobProfileID = 1098, @StepNumber = 1,  
	@Description = 'Metrix Export Status update job for File generation Failure',
	@JobName = 'MetrixExportStatusUpdateJob', --@ArchiveLocation = 'C:\Temp\LiveVox\Automation',
	@MetrixDatabaseLookup = 'METRIX',
	@Branch = 'FAILURE', @TriggerBranch = 'EXPORT', -- Update ExportBatch table on failure
	@TriggerExecutionStatus = 'F', @TriggerExecutionNameSpace = 'SEIDR', 
	--uncaught exceptions/errors (Caught "errors" would use a different status code, and then stop execution since they a job execution with error status will only continue running when the next step is triggered by that explicit status)
	--Example caught error: "METRIX_EXPORT.ND" - no data to export, nothing to do after the step. 
	--Would still send the failure notification that no data was eligible to export (from the step that actually "failed"), though
	@FailureNotificationMail = @SE_Notification

	
exec SEIDR.usp_JobProfile_FTPJob_iu @JobProfileID = 1098,
	@StepNumber = 2,
	@TriggerBranch = 'EXPORT',
	@FTPOperation = 'SEND',
	@FTPAccount = 'NORDIS DIRECT',
	@LocalPath = null,
	@RemotePath = '/',
	@CanRetry = 1,
	@RetryLimit = 6,
	@RetryDelay = 10, --Try for one hour
	@FailureNotificationMail = @BI_SE_Notification
	
	
exec SEIDR.usp_JobProfile_ExportSettings_iu	
	@JobProfileID = 1098, @StepNumber = 2,  
	@Description = 'Metrix Export Status update job for FTP Send Failure',
	@JobName = 'MetrixExportStatusUpdateJob', --@ArchiveLocation = 'C:\Temp\LiveVox\Automation',
	@MetrixDatabaseLookup = 'METRIX',
	@Branch = 'FAILURE', @TriggerBranch = 'EXPORT',
	@TriggerExecutionStatus = 'F', @TriggerExecutionNameSpace = 'SEIDR',
	@FailureNotificationMail = @SE_Notification

exec SEIDR.usp_JobProfile_ExportSettings_iu @JobProfileID = 1098,
	@JobName = 'StatementXMLGenerationFollowupJob',
	@Description = 'Finish Letter delivery (update ExportBatch and DateSent)',
	@StepNumber = 3,
	@MetrixDatabaseLookup = 'METRIX',
	@VendorName = 'NORDIS',
	@ExportType = 'Statement Export',
	@CanRetry = 1,
	@ThreadID = 6,
	@TriggerBranch = 'EXPORT',
	@FailureNotificationMail = @SE_Notification
	 