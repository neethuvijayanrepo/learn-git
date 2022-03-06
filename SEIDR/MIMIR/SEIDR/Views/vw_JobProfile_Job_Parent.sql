CREATE VIEW [SEIDR].[vw_JobProfile_Job_Parent]
AS
SELECT 
	je.JobProfile_JobID, 
	je.JobExecutionID, 
	je.StepNumber, 
	je.ProcessingDate, 
	p.SequenceDayMatchDifference,
	--p.SequenceScheduleID, 
	p.Parent_JobProfile_JobID,
	h.JobProfile_JobID History_JobProfile_JobID, 
	h.JobExecutionID History_JobExecutionID, 
	h.StepNumber History_StepNumber, 
	h.ProcessingDate History_ProcessingDate
FROM SEIDR.JobExecution je WITH(NOLOCK)
	JOIN SEIDR.JobProfile_Job_Parent p WITH(NOLOCK)
	ON je.JobProfile_JobID = p.JobProfile_JobID
	LEFT JOIN SEIDR.vw_JobExecutionHistory h WITH(NOLOCK)
	ON p.Parent_JobProfile_JobID = h.JobProfile_JobID
	AND DATEDIFF(day, h.ProcessingDate, je.ProcessingDate) = p.SequenceDayMatchDifference
	--AND SEIDR.ufn_CheckSchedule(p.SequenceScheduleID, je.ProcessingDate, h.ProcessingDate) <> 0
	AND h.Success = 1