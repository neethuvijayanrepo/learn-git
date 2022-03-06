/*
exec SEIDR.usp_JobProfile_GetConfigurationScript 783
exec SEIDR.usp_JobProfile_GetConfigurationScript 238

 --Note on initial deployment: We want to call the data prep procedure to populate the account state table ahead of time, 
 -- so that we don't send a seed file to LiveVox, because that will be too much volume and in theory they already have the data. So only data for going forward as our data changes
 exec SEIDR.usp_JobProfile_Help 238
 exec SEIDR.usp_JobProfile_help 783
 exec SEIDR.usp_JobProfile_Help 77
 */

 --SELECT * FROM SEIDR.JobExecution WHERE JobProfileID = 783

 exec SEIDR.usp_JobProfile_i  'LiveVox Automation', 0, null, null, 'EXPORT', null, 'LIVEVOX AUTOMATION', 
	@SafetyMode = 0, @JobProfileID = 77,
	@SuccessNotificationMail = 'antonio.aguilar@navigant.com'

exec SEIDR.usp_JobProfile_ExportSettings_iu	
	@JObProfileID = 77, @StepNumber = 1, 
	@JobName = 'LiveVoxAutomatedExportJob', 
	@ArchiveLocation = '\\ncimtxfls02.nci.local\vendor_export_archive_prd\Production_File_Store\LiveVox\Export\_AUTOMATION',
	@MetrixDatabaseLookup = 'LIVEVOX', @CanRetry = 0,
	@FailureNotificationMail = 'ryan.langer@navigant.com,elias.zeidy@navigant.com,kris.salas@navigant.com,antonio.aguilar@navigant.com'

exec SEIDR.usp_JobProfile_ExportSettings_iu	
	@JobProfileID = 77, @StepNumber = 1,  
	@Description = 'Metrix Export Status update job for File generation Failure',
	@JobName = 'MetrixExportStatusUpdateJob', --@ArchiveLocation = 'C:\Temp\LiveVox\Automation',
	@MetrixDatabaseLookup = 'LIVEVOX',
	@TriggerExecutionStatusCode = 'F', @TriggerExecutionNameSpace = 'SEIDR',
	@FailureNotificationMail = 'ryan.langer@navigant.com,elias.zeidy@navigant.com'


	--FTP Configuration for Sending file
	exec [SEIDR].[usp_JobProfile_FTPJob_iu]
	@JobProfileID = 77,
	@StepNumber = 2,
	@Description = null,	
	@FTPAccount = 'METRIX Export - LiveVox',
	@FTPOperation = 'SEND',
	@LocalPath = NULL,
	@RemotePath = '/ftpIn/',
	@RemoteTargetPath = NULL,
	@Overwrite = 1,
	@Delete = 0,
	@DateFlag = 1,
	@CanRetry = 1,
	@RetryLimit = 500,
	@RetryDelay = 15,
	@TriggerExecutionStatus = NULL,
	@TriggerExecutionNameSpace = NULL,
	@ThreadID = NULL,
	@FailureNotificationMail = 'ryan.langer@navigant.com,elias.zeidy@navigant.com,kris.salas@navigant.com,antonio.aguilar@navigant.com',
	@SequenceSchedule = NULL,
	@SafetyMode = 1
	
	exec SEIDR.usp_JobProfile_ExportSettings_iu	
		@JobProfileID = 77, @StepNumber = 2,  @Description = 'Metrix Export Status update job for FTP Failure',
		@JobName = 'MetrixExportStatusUpdateJob', --@ArchiveLocation = 'C:\Temp\LiveVox\Automation',
		@MetrixDatabaseLookup = 'LIVEVOX',
		@TriggerExecutionStatusCode = 'F', @TriggerExecutionNameSpace = 'SEIDR',
		@FailureNotificationMail = 'ryan.langer@navigant.com,elias.zeidy@navigant.com'

	exec SEIDR.usp_JobProfile_ExportSettings_iu	
		@JobProfileID = 77, @StepNumber = 3, @Description = 'Metrix Export Status update job for Success',
		@JobName = 'MetrixExportStatusUpdateJob', --@ArchiveLocation = 'C:\Temp\LiveVox\Automation',
		@MetrixDatabaseLookup = 'LIVEVOX',
		@FailureNotificationMail = 'ryan.langer@navigant.com,elias.zeidy@navigant.com'


--PST
SELECT * FROM SEIDR.vw_Schedule WHERE IntervalType = 'dd' AND Hour BETWEEN 0 AND 5 AND ISNULL(Minute, 0) = 0

exec SEIDR.usp_JobProfile_SetSchedule 77, 
	@Schedule ='Daily @ 2:00AM', --This is PST, so 4 AM CST
	@ScheduleFromDate = '2020-02-14' -- allow 2/13 to be a manual execution, check on output first.


--exec SEIDR.usp_JobExecution_Schedule_Ondemand 77, @StopAfterStepNumber = 1