CREATE TABLE [SEIDR].[Schedule] (
    [ScheduleID]         INT           IDENTITY (1, 1) NOT NULL,
    [Description]        VARCHAR (250) NOT NULL,
    [Creator]            VARCHAR (128) DEFAULT (suser_name()) NOT NULL,
    [DC]                 DATETIME      DEFAULT (getdate()) NOT NULL,
    [DD]                 DATETIME      NULL,
    [Active]             AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)),
    [ForSequenceControl] BIT           CONSTRAINT [DF__Schedule__ForSequenceControl] DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ScheduleID] ASC),
    CONSTRAINT [UQ_Schedule_Description] UNIQUE NONCLUSTERED ([Description] ASC)
);









