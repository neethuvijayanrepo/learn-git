CREATE TABLE [SEIDR].[ScheduleRuleCluster] (
    [ScheduleRuleClusterID] INT           IDENTITY (1, 1) NOT NULL,
    [Description]           VARCHAR (250) NULL,
    [DC]                    SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [Creator]               VARCHAR (128) DEFAULT (suser_name()) NOT NULL,
    [DD]                    SMALLDATETIME NULL,
    [Active]                AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    PRIMARY KEY CLUSTERED ([ScheduleRuleClusterID] ASC),
    CONSTRAINT [uq_ScheduleRuleCluster_Description] UNIQUE NONCLUSTERED ([Description] ASC)
);



