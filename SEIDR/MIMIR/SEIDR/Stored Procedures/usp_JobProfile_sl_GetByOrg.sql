
/*SP FOR DISPLAYING GRID VALUES*/

CREATE PROCEDURE [SEIDR].[usp_JobProfile_sl_GetByOrg] 
@OrganizationID int=NULL,
@ProjectID smallint=NULL,
@JobProfileID int=NULL,
@ProcessingDate date=NULL,
@ExecutionStatus varchar(20)=NULL
AS

	IF(@OrganizationID IS NULL 
	AND @ProjectID IS NULL 
	AND @JobProfileID IS NULL 
	AND @ProcessingDate IS NULL 
	AND @ExecutionStatus IS NULL)
	BEGIN

		SELECT 
		OrganizationID,
		ProjectID,
		Description,
		userKey1,
		ProcessingDate,
		ProcessingDayOfWeek,
		JobExecutionID,
		FilePath,
		ExecutionStatusDescription,
		StepNumber,
		CurrentStep,
		CurrentJobName,
		IsError ,
		IsComplete,
		JobPriority,
		ProcessingDateExpanded,
		ExecutionStatus,
		IsWorking,
		ExpectedDeliverySchedule,
		Schedule,
		ScheduleFromDate,
		ScheduleThroughDate,
		RegistrationFolder,
		RegistrationDestinationFolder,
		FileFilter,
		FileDateMask,
		Creator
		FROM SEIDR.vw_JobProfile_ProcessingDate
		WHERE Today=1
	END
	ELSE
	BEGIN
		SELECT 
		OrganizationID,
		ProjectID,
		Description,
		userKey1,
		ProcessingDate,
		ProcessingDayOfWeek,
		JobExecutionID,
		FilePath,
		ExecutionStatusDescription,
		StepNumber,
		CurrentStep,
		CurrentJobName,
		IsError ,
		IsComplete,
		JobPriority,
		ProcessingDateExpanded,
		ExecutionStatus,
		IsWorking,
		ExpectedDeliverySchedule,
		Schedule,
		ScheduleFromDate,
		ScheduleThroughDate,
		RegistrationFolder,
		RegistrationDestinationFolder,
		FileFilter,
		FileDateMask,
		Creator

		FROM SEIDR.vw_JobProfile_ProcessingDate 

		WHERE OrganizationID=@OrganizationID
		AND ProjectID=@ProjectID
		AND ((@JobProfileID IS NULL) OR (JobProfileID = @JobProfileID)) 
		AND((@ProcessingDate IS NULL) or (ProcessingDate = @ProcessingDate))
		AND ((@ExecutionStatus IS NULL) OR (ExecutionStatus = @ExecutionStatus)) ORDER BY ProcessingDate DESC
	END