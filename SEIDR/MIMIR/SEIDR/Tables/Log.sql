CREATE TABLE [SEIDR].[Log] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [ThreadID]         SMALLINT       NOT NULL,
    [ThreadType]       VARCHAR (50)   NOT NULL,
    [ThreadName]       VARCHAR (100)  NOT NULL,
    [LogMessage]       VARCHAR (2000) NOT NULL,
    [MessageType]      VARCHAR (5)    NOT NULL,
    [JobProfileID]     INT            NULL,
    [JobExecutionID]   BIGINT         NULL,
    [JobProfile_JobID] INT            NULL,
    [LogTime]          DATETIME       DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([JobExecutionID]) REFERENCES [SEIDR].[JobExecution] ([JobExecutionID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
    FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID])
);





GO
CREATE NONCLUSTERED INDEX [ix_Log_JobExecutionID_includes]
    ON [SEIDR].[Log]([JobExecutionID] ASC)
    INCLUDE([Id], [ThreadID], [LogMessage], [MessageType], [JobProfile_JobID], [LogTime]) WITH (FILLFACTOR = 100);


GO
CREATE NONCLUSTERED INDEX [IDX_Log_149]
    ON [SEIDR].[Log]([JobExecutionID] ASC)
    INCLUDE([Id], [ThreadID], [ThreadType], [ThreadName], [LogMessage], [MessageType], [JobProfileID], [JobProfile_JobID], [LogTime]);

