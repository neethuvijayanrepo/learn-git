
--SELECT * FROM UTIL.vw_Check_Constraints WHERE PARENT_OBJECT_ID = OBJECT_ID('SEIDR.Job')
ALTER TABLE [SEIDR].[Job] DROP CONSTRAINT [CK_Validate_Only]
GO

UPDATE SEIDR.Job
SET JobNameSpace = 'METRIX_EXPORT'
WHERE JobName = 'LiveVoxFileGenerationJob'
GO

ALTER TABLE [SEIDR].[Job] WITH NOCHECK ADD CONSTRAINT [CK_Validate_Only] CHECK (isnull(object_name(@@procid),'')='usp_Job_Validate' OR is_srvrolemember('sysadmin')=(1))
GO
