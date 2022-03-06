UPDATE jpj
SET CanRetry = 0 -- SELECT *
FROM SEIDR.JobProfile_Job jpj
JOIN SEIDR.FileSystemJob fs
	ON jpj.JobProfile_JobID = fs.JobProfile_JobID
WHERE fs.Operation IN ('COPY_METRIX', 'MOVE_METRIX')
AND CanRetry = 1