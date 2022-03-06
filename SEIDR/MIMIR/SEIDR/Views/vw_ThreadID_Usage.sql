
CREATE VIEW SEIDR.vw_ThreadID_Usage
AS

SELECT COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID) ThreadID, 
	COUNT(jpj.RequiredThreadID) [StepLevel],
	COUNT(CASE WHEN jpj.RequiredThreadID is null and jp.RequiredThreadID IS NOT NULL then 1 end) [ProfileLevel], 
	COUNT(*) [TotalThreadUsage]
FROM SEIDR.JobProfile jp
JOIN SEIDR.JobProfile_Job jpj
	ON jp.JobProfileID = jpj.JobProfileID
WHERE jp.Active = 1
AND jpj.Active = 1
GROUP BY COALESCE(jpj.RequiredThreadID, jp.RequiredThreadID)