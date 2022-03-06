CREATE TABLE [CONFIG].[SSIS_Package_History] (
    [HistoryID]   INT           IDENTITY (1, 1) NOT NULL,
    [PackageID]   INT           NOT NULL,
    [Category]    VARCHAR (128) NOT NULL,
    [Name]        VARCHAR (128) NOT NULL,
    [ServerName]  VARCHAR (130) NULL,
    [PackagePath] VARCHAR (500) NOT NULL,
    [Editor]      VARCHAR (170) NOT NULL,
    [ArchiveTime] SMALLDATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([HistoryID] ASC)
);

