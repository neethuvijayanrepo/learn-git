CREATE TYPE [SEIDR].[udt_DocMetaDataColumn] AS TABLE (
    [ColumnName]   VARCHAR (128) NOT NULL,
    [Position]     INT           NOT NULL,
    [Max_Length]   SMALLINT      NULL,
    [SortASC]      BIT           DEFAULT ((1)) NULL,
    [SortPriority] SMALLINT      NULL,
    PRIMARY KEY CLUSTERED ([Position] ASC));


