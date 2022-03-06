CREATE VIEW REFERENCE.vw_JobExecution_Parent
AS
SELECT 
	p.JobProfileID, p.StepNumber, p.Step, CASE WHEN je.JobProfile_JobID = p.JobProfile_JobID then CAST(1 as bit) else cast(0 as bit) end CurrentStep,
	ISNULL(je.JobExecutionID, h.JobExecutionID) [JobExecutionID], ISNULL(je.ProcessingDate, h.ProcessingDate) [ProcessingDate],
	
	p.ParentJobProfileID, p.ParentStepNumber, p.ParentStep,
	
	SequenceDayMatchDifference,
	DATEADD(day, SequenceDaymatchDifference, ISNULL(je.ProcessingDate, h.ProcessingDate)) [ParentProcessingDate],	
	pje.JobExecutionID [ParentJobExecutionID], ISNULL(pjeh.Success, 0) [ParentComplete] --*/
FROM SEIDR.vw_JobProfile_Parent p
LEFT JOIN SEIDR.JobExecution je
	ON p.JobProfile_JobID = je.JobProfile_JobID
LEFT JOIN SEIDR.JobExecution_ExecutionStatus h
	ON p.JobProfile_JobID = h.JobProfile_JobID
	AND h.IsLatestForExecutionStep = 1
	AND je.JobExecutionID is null
LEFT JOIN SEIDR.JobExecution pje
	ON p.ParentJobProfileID = pje.JobProfileID
	AND DATEDIFF(day, ISNULL(je.ProcessingDate, h.ProcessingDate), pje.ProcessingDate) = p.SequenceDaymatchDifference
LEFT JOIN SEIDR.JobExecution_ExecutionStatus pjeh
	ON pje.JobExecutionID = pjeh.JobExecutionID
	AND pjeh.JobProfile_JobID = p.ParentJobProfile_JobID
	AND pjeh.IsLatestForExecutionStep = 1