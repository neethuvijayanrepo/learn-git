CREATE TABLE [SEIDR].[FileSystemOperation] (
    [FileSystemOperationCode] VARCHAR (30)  NOT NULL,
    [Description]             VARCHAR (200) NOT NULL,
    [RequireSource]           BIT           DEFAULT ((0)) NOT NULL,
    [RequireOutputPath]       BIT           DEFAULT ((0)) NOT NULL,
    [RequireFilter]           BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([FileSystemOperationCode] ASC)
);


