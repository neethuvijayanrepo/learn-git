CREATE TABLE [SEIDR].[Schedule_ScheduleRuleCluster] (
    [ScheduleID]            INT NOT NULL,
    [ScheduleRuleClusterID] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([ScheduleID] ASC, [ScheduleRuleClusterID] ASC),
    FOREIGN KEY ([ScheduleID]) REFERENCES [SEIDR].[Schedule] ([ScheduleID])  ON DELETE CASCADE,
    FOREIGN KEY ([ScheduleRuleClusterID]) REFERENCES [SEIDR].[ScheduleRuleCluster] ([ScheduleRuleClusterID]) 
);

