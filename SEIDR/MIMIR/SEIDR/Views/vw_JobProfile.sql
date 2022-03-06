CREATE VIEW [SEIDR].[vw_JobProfile]
	AS 
	
	SELECT JobProfileID, jp.Description, 
		jp.OrganizationID, o.Description [Organization], 
		jp.ProjectID, p.[Description] as [Project], p.CRCM, p.Modular, p.Active [ProjectActive],
		jp.LoadProfileID, jp.UserKey1, jp.UserKey2, jp.UserKey1 as [UserKey], uk.Description as [UserKeyDescription],
		jp.RegistrationValid, 
				jp.RegistrationFolder, CONFIG.ufn_GetShortHandPath(jp.RegistrationFolder, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) [ShortHandRegistrationFolder], 
				jp.FileDateMask, jp.FileFilter, jp.FileExclusionFilter,
				jp.RegistrationDestinationFolder, CONFIG.ufn_GetShortHandPath(jp.RegistrationDestinationFolder, jp.OrganizationID, jp.ProjectID, jp.UserKey1, jp.LoadProfileID) [ShortHandRegistrationDestinationFolder],
		jp.ScheduleValid,
		s.ScheduleID, s.Description [Schedule], jp.ScheduleFromDate, jp.ScheduleThroughDate, 
		jp.ScheduleNoHistory, CONVERT(bit, 1- jp.ScheduleNoHistory) [CreateHistoricalExecutions],
		jp.DeliveryScheduleID, ds.Description [DeliverySchedule],
		jp.RequiredThreadID,
		jpj.ConfiguredSteps,		
		spo.SpawnJobParentCount, 
		je.*,
		jp.SuccessNotificationMail, jp.Track, jp.StopAfterStepNumber, jp.JobPriority
	FROM SEIDR.JobProfile jp
	JOIN REFERENCE.UserKey uk WITH (NOLOCK)
		ON jp.UserKey1 = uk.UserKey
	LEFT JOIN REFERENCE.Project p WITH (NOLOCK)
		ON jp.ProjectID = p.ProjectID
	JOIN REFERENCE.Organization o WITH (NOLOCK)
		ON jp.OrganizationID = o.OrganizationID
	LEFT JOIN SEIDR.Schedule ds
		ON jp.DeliveryScheduleID = ds.ScheduleID
		AND ds.Active = 1
	LEFT JOIN SEIDR.Schedule s
		ON jp.ScheduleID = s.ScheduleID
		--AND jp.ScheduleValid = 1 
		AND s.Active = 1	
	CROSS APPLY ( SELECT COUNT(*) [ConfiguredSteps]
					FROM SEIDR.JobProfile_Job
					WHERE JobProfileID = jp.JobProfileID
					AND Active = 1) jpj
	CROSS APPLY(SELECT COUNT(*) [SpawnJobParentCount]
					FROM SEIDR.SpawnJob sp
					JOIN SEIDR.JobProfile_Job jpj
						ON sp.JobProfile_JobID = jpj.JobProfile_JobID
						AND jpj.Active = 1
					WHERE sp.JobProfileID = jp.JobProfileID)spo
	CROSS APPLY(SELECT MIN(ProcessingDate) [EarliestProcessingDate], MAX(ProcessingDate) [LatestProcessingDate], 
				COUNT(CASE WHEN Active = 1 then 1 end) [ActiveJobExecutionCount],
				COUNT(CASE WHEN Active = 1 and jes.IsComplete = 1 then 1 end) [CompleteJobExecutionCount]
				FROM SEIDR.JobExecution je
				JOIN SEIDR.ExecutionStatus jes
					ON je.ExecutionStatusCode = jes.ExecutionStatusCode
					AND je.ExecutionStatusNameSpace = jes.[NameSpace]
				WHERE JobProfileID = jp.JobProfileID)je
	WHERE jp.Active = 1
