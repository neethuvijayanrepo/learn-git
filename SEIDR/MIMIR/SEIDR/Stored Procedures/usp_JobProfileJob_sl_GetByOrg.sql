/*SP FOR DISPLAYING GRID VALUES*/

CREATE PROCEDURE [SEIDR].[usp_JobProfileJob_sl_GetByOrg]
	@OrganizationID int,
	@JobProfileID int=NULL,
	@IncludeInactive bit
AS
BEGIN
	SELECT 
	jp.OrganizationID,
	JobProfile_JobID, 
	jp.Description [Profile],
	StepNumber,
	jpj.Description [StepDescriptions], 
	j.JobID, 
	j.JobName, 
	ConfigurationTable,
	jpj.TriggerExecutionStatusCode,
	jpj.TriggerExecutionNameSpace, 
	CanRetry,
	RetryLimit,
	ISNULL(RetryDelay, 10) [RetryDelay], 
	jpj.RequiredThreadID,
	FailureNotificationMail,
	jpj.Active

	FROM SEIDR.JobProfile_Job jpj
	JOIN SEIDR.Job j ON jpj.JobID = j.JobID
	JOIN SEIDR.JobProfile jp ON jpj.JobProfileID = jp.JobProfileID

	WHERE 1 = 1
	AND (jp.OrganizationID=@OrganizationID)
	AND (@JobProfileID IS NULL OR Jpj.JobProfileID = @JobProfileID)
	AND (jpj.Active = 1 OR @IncludeInactive = 1) 
	ORDER BY StepNumber, JobProfile_JobID

END