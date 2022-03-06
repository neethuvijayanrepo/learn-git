
CREATE VIEW UTIL.vw_Triggers
AS
SELECT *
      ,CASE OBJECTPROPERTY([trg].[OBJECT_id] ,'ExecIsFirstInsertTrigger')
            WHEN 0 THEN ''
            ELSE 'X'
       END AS 'Insert First'
      ,CASE OBJECTPROPERTY([trg].[OBJECT_id] ,'ExecIsLastInsertTrigger')
            WHEN 0 THEN ''
            ELSE 'X'
       END AS 'Insert Last'
      ,CASE OBJECTPROPERTY([trg].[OBJECT_id] ,'ExecIsFirstUpdateTrigger')
            WHEN 0 THEN ''
            ELSE 'X'
       END AS 'Update First'
      ,CASE OBJECTPROPERTY([trg].[OBJECT_id] ,'ExecIsLastUpdateTrigger')
            WHEN 0 THEN ''
            ELSE 'X' END AS 'Update Last'
      ,CASE OBJECTPROPERTY([trg].[OBJECT_ID] ,'ExecIsFirstDeleteTrigger')
            WHEN 0 THEN ''
            ELSE 'X'
       END AS 'Delete First'
      ,CASE OBJECTPROPERTY([trg].[OBJECT_ID] ,'ExecIsLastDeleteTrigger')
            WHEN 0 THEN ''
            ELSE 'X'
       END AS 'Delete Last'
	FROM sys.triggers trg