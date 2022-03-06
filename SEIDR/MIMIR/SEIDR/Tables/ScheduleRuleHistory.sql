CREATE TABLE [SEIDR].[ScheduleRuleHistory] (
    [Description]           VARCHAR (250)  NULL,
    [PartOfDateType]        VARCHAR (4)    NULL,
    [PartOfDate]            INT            NULL,
    [IntervalType]          VARCHAR (4)    NULL,
    [IntervalValue]         INT            NULL,
    [Hour]                  TINYINT        NULL,
    [Minute]                TINYINT        NULL,
    [Modifier]              NVARCHAR (128) NULL,
    [DD]                    SMALLDATETIME  NULL,
    [ArchiveTime]           DATETIME       NOT NULL,
    [ScheduleRuleHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [ScheduleRuleID]        INT            NULL,
    PRIMARY KEY CLUSTERED ([ScheduleRuleHistoryID] ASC)
);

