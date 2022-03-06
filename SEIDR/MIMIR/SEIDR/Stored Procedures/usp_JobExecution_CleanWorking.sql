
CREATE PROCEDURE [SEIDR].[usp_JobExecution_CleanWorking]
as
BEGIN
	UPDATE SEIDR.JobExecution
	SET IsWorking = 0,
		InWorkQueue = 0
	WHERE (IsWorking = 1 or InWorkQueue = 1)


	DECLARE @CTXT varbinary(255) = CAST('SERVICE' as varbinary(255))
	SET CONTEXT_INFO @CTXT

	
	UPDATE jp
	SET FileFilter = f.FileFilter --SELECT f.*, jp.* 
	FROM SEIDR.JobProfile jp
	CROSS APPLY(SELECT TOP 1 FileFilter 
				FROM SEIDR.JobProfileHistory h
				WHERE JobProfileID = jp.JobProfileiD
				AND FileFilter <> 'INVALID'
				ORDER BY JobProfileHistoryID DESC) f
	WHERE jp.FileFilter = 'INVALID'

	SET CONTEXT_INFO 0
END

