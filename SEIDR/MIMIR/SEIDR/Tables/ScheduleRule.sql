CREATE TABLE [SEIDR].[ScheduleRule] (
    [ScheduleRuleID] INT           IDENTITY (1, 1) NOT NULL,
    [Description]    VARCHAR (250) NULL,
    [PartOfDateType] VARCHAR (5)   NULL,
    [PartOfDate]     INT           NULL,
    [IntervalType]   VARCHAR (4)   NULL,
    [IntervalValue]  INT           NULL,
    [Hour]           TINYINT       NULL,
    [Minute]         TINYINT       NULL,
    [DC]             SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [Creator]        VARCHAR (128) DEFAULT (suser_name()) NOT NULL,
    [DD]             SMALLDATETIME NULL,
    [Active]         AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    PRIMARY KEY CLUSTERED ([ScheduleRuleID] ASC),
    CHECK ([Hour]>=(0) AND [Hour]<=(23)),
    CHECK ([IntervalType] IS NULL OR [IntervalValue]>=(0)),
    CHECK ([Minute]>=(0) AND [Minute]<=(59)),
    CHECK ([PartOfDateType] IS NOT NULL OR [IntervalType] IS NOT NULL),
    CHECK ([PartOfDateType] IS NULL OR [PartOfDate] IS NOT NULL),
    CONSTRAINT [FK_ScheduleRule_ScheduleRulePartOfDateType] FOREIGN KEY ([PartOfDateType]) REFERENCES [SEIDR].[ScheduleDatePart] ([PartOfDateType]),
    CONSTRAINT [uq_ScheduleRule_Description] UNIQUE NONCLUSTERED ([Description] ASC)
);












GO
CREATE TRIGGER SEIDR.trg_ScheduleRule_u
   ON  SEIDR.ScheduleRule
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	
	IF 1 < (SELECT COUNT(*)
				FROM INSERTED i
				JOIN SEIDR.ScheduleRuleCluster_ScheduleRule srcsr
					ON i.ScheduleRuleID = srcsr.ScheduleRuleID)
	BEGIN
		ROLLBACK
		RAISERROR('ScheduleRule is in use by multiple ScheduleRuleClusters. Cannot alter.', 16, 1);
		RETURN
	END				

	INSERT INTO [SEIDR].[ScheduleRuleHistory]
			   ([Description]
			   ,[PartOfDateType]
			   ,[PartOfDate]
			   ,[IntervalType]
			   ,[IntervalValue]
			   ,[Hour]
			   ,[Minute]
			   ,[Modifier]
			   ,[DD]
			   ,[ArchiveTime]
			   ,[ScheduleRuleID])
		 SELECT
			   d.Description
			   ,d.PartOfDateType
			   ,d.PartOfDate
			   ,d.IntervalType
			   ,d.IntervalValue
			   ,d.[Hour]
			   ,d.[Minute]
			   ,SUSER_NAME() as Modifier
			   ,d.DD
			   ,GETDATE()
			   ,d.ScheduleRuleID
		FROM DELETED d
		JOIN INSERTED i
			ON d.ScheduleRuleID = i.ScheduleRuleID
		WHERE ISNULL(i.Description, '') <> d.Description 
		OR ISNULL(i.PartOfDateType, '') <> ISNULL(d.PartOfDateType, '')
		OR ISNULL(i.PartOfDate, '') <> ISNULL(d.PartOfDate, '')
		OR ISNULL(i.Hour, 0) <> ISNULL(d.Hour, 0)
		OR ISNULL(i.Minute, 0) <> ISNULL(d.Minute, 0)
		OR i.Active <> d.Active
END