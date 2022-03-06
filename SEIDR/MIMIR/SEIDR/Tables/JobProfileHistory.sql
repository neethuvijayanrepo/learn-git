CREATE TABLE [SEIDR].[JobProfileHistory] (
    [JobProfileHistoryID]           INT           IDENTITY (1, 1) NOT NULL,
    [JobProfileID]                  INT           NOT NULL,
    [OrganizationID]                INT           NULL,
    [ProjectID]                     INT           NULL,
    [LoadProfileID]                 INT           NULL,
    [UserKey1]                      VARCHAR (50)  NULL,
    [UserKey2]                      VARCHAR (50)  NULL,
    [RegistrationFolder]            VARCHAR (500) NULL,
    [FileFilter]                    VARCHAR (600) NULL,
    [FileDateMask]                  VARCHAR (128) NULL,
    [RegistrationDestinationFolder] VARCHAR (500) NULL,
    [ScheduleID]                    INT           NULL,
    [ScheduleFromDate]              DATETIME      NULL,
    [ScheduleThroughDate]           DATETIME      NULL,
    [Editor]                        VARCHAR (128) NOT NULL,
    [DC]                            SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [AgeMinutes]                    INT           NOT NULL,
    [AgeHours]                      AS            ([AgeMinutes]/(60)),
    [AgeDays]                       AS            ([AgeMinutes]/(1440)),
    [TriggeringUser]                VARCHAR (128) CONSTRAINT [df_JobProfileHistory_Triggeruser] DEFAULT (suser_name()) NOT NULL,
    [Active]                        BIT           NOT NULL,
    [ChangeSummary]                 VARCHAR (500) NULL,
    [FileExclusionFilter]           VARCHAR (600) NULL,
    CONSTRAINT [PK_JobProfileHistory_JobProfileHistoryID] PRIMARY KEY CLUSTERED ([JobProfileHistoryID] ASC),
    CONSTRAINT [CK_TriggerOnly] CHECK (Trigger_Nestlevel()>(0)),
    FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID]),
    FOREIGN KEY ([ScheduleID]) REFERENCES [SEIDR].[Schedule] ([ScheduleID])
);





