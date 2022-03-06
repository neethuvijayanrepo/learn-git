CREATE TABLE [SEIDR].[ScheduleDatePart] (
    [PartOfDateType] VARCHAR (5)   NOT NULL,
    [Description]    VARCHAR (105) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([PartOfDateType] ASC)
);

