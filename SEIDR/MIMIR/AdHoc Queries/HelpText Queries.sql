exec sp_helptext 'SEIDR.trg_JobExecution_iu';
GO
exec sp_helptext 'SEIDR.vw_JobExecutionHistory';
GO

SELECT * FROM SEIDR.vw_JobExecutionHistory h
JOIN SEIDR.JobExecution je
	on h.JobExecutionID = je.JobExecutionID
WHERE je.JobProfileID = 12
ORDER BY h.JobExecutionID, JobExecution_ExecutionStatusID