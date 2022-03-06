/*SP FOR DISPLAYING GRID VALUES*/

CREATE PROCEDURE [SEIDR].[usp_JobExecution_sl_GetByOrg] 
(
	@OrganizationID int,
	@userkey1 varchar(50)=NULL,
	@UserKey2 varchar(50)=NULL,
	@ExecutionStatus varchar(20)=NULL,
	@FromProcessingDate date=NULL,
    @ThroughProcessingDate date=NULL,
	@Complete int=1,
	@Missing int=1,
	@InComplete int=1,
	@HasError int=1 
)
AS

BEGIN

SELECT 
OrganizationID,
userkey1,
UserKey2,
Description,
ProcessingDate,
ProcessingDayOfWeek,
JobExecutionID,
FilePath,
ExecutionStatusDescription,
StepNumber,
CurrentStep,
IsError ,
IsComplete,
JobPriority,
IsWorking,
ExpectedDeliverySchedule,
Schedule,
ScheduleFromDate,
ScheduleThroughDate
FROM SEIDR.vw_JobProfile_ProcessingDate
WHERE 
     (OrganizationID = @OrganizationID)
AND  (((@userkey1 IS NULL) 
		OR (userkey1 = @userkey1)) 
		AND ((@UserKey2 IS NULL)
	    OR (UserKey2 = @UserKey2)))
AND  ((@ExecutionStatus IS NULL) 
		OR (ExecutionStatus = @ExecutionStatus)) 
AND  ((@FromProcessingDate is null and  @ThroughProcessingDate is null and  Today=1)
		OR(@FromProcessingDate is not null and  @ThroughProcessingDate is  not null and (ProcessingDate between @FromProcessingDate and @ThroughProcessingDate))
		OR(@FromProcessingDate is null and  @ThroughProcessingDate is not null and ProcessingDate <= @ThroughProcessingDate) 
	    OR(@FromProcessingDate is not null and  @ThroughProcessingDate is null and ProcessingDate >= @FromProcessingDate))
AND  ((@Missing = 1 and @HasError = 1 and IsError in (-1,1))
		OR(@Missing = 1 and IsError= -1 )
		OR(@HasError = 1 and IsError= 1)
		OR(@Complete=1  and @InComplete = 1 and IsComplete in (0,1)) 
		OR(@Complete=1  and IsComplete= 1) 
		OR(@InComplete = 1 and IsComplete= 0)) order by ProcessingDate
END