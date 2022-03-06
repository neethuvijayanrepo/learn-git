CREATE TABLE [UTIL].[DDL_ChangeLog] (
    [LogId]        INT           IDENTITY (1, 1) NOT NULL,
    [DatabaseName] VARCHAR (256) NOT NULL,
    [EventType]    VARCHAR (50)  NOT NULL,
    [ObjectName]   VARCHAR (256) NULL,
    [ObjectSchema] VARCHAR (128) NULL,
    [ObjectType]   VARCHAR (25)  NOT NULL,
    [SqlCommand]   VARCHAR (MAX) NOT NULL,
    [EventDate]    DATETIME      CONSTRAINT [DF_EventsLog_EventDate] DEFAULT (getdate()) NOT NULL,
    [LoginName]    VARCHAR (256) NOT NULL
);

