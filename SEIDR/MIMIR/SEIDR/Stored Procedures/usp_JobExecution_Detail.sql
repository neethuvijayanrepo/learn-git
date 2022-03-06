CREATE PROCEDURE SEIDR.usp_JobExecution_Detail(
@JobExecutionID int
)
AS
BEGIN
	SELECT 
	je.JobProfileID,
	jp.Description AS JobProfile,
	je.OrganizationID,
	je.UserKey1,
	je.UserKey2,
	je.ProjectID,
	p.Description AS Project,
	je.JobExecutionID,
	je.ProcessingDate,
	je.TotalExecutionTimeSeconds,
	je.LU,
	je.FilePath,
	Je.ExecutionStatus,
	es.Description AS ExecutionStatusDescription,
	je.RetryCount,
	es.IsComplete,
	es.IsError,
	je.StepNumber,
	je.JobProfile_JobID,
	(select MAX(StepNumber) FROM SEIDR.JobProfile_Job WHERE JobProfileID=je.JobProfileID) AS LastStepNumber,
	jpj.Description AS JobProfile_Job,
	j.JobName AS JobDescription,
	IIF(vwj.InSequence = 0, '- OUT OF SEQUENCE', '') as [Insequence],

	CASE 
		WHEN es.IsComplete = 1 AND es.IsError = 1 then 'COMPLETED VIA ERROR' 
		WHEN es.IsComplete = 1 then 'COMPLETE'  
		WHEN es.IsError = 1 AND je.JobProfile_JobID is null then 'ERROR' 
		WHEN es.IsError = 1 then 'ERROR HANDLING STEP' 
		WHEN je.RetryCount > 0 then 'ERROR - RETRYING' 
		ELSE 'INCOMPLETE' end 
	AS Progress

	FROM SEIDR.JobExecution je LEFT JOIN SEIDR.ExecutionStatus es 
	ON ((je.ExecutionStatusNameSpace=es.NameSpace)AND(je.ExecutionStatusCode=es.ExecutionStatusCode))
	LEFT JOIN SEIDR.JobProfile jp ON je.JobProfileID=jp.JobProfileID
	LEFT JOIN SEIDR.JobProfile_Job jpj ON jpj.JobProfile_JobID=je.JobProfile_JobID
	LEFT JOIN SEIDR.Job j on j.JobID=jpj.JobID
	LEFT JOIN REFERENCE.Project p ON p.ProjectID=je.ProjectID
	LEFT JOIN SEIDR.vw_JobExecution vwj ON vwj.JobExecutionID=je.JobExecutionID
	WHERE je.JobExecutionID=@JobExecutionID
	 
END