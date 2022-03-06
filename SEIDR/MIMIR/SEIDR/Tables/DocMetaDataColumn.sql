CREATE TABLE [SEIDR].[DocMetaDataColumn] (
    [MetaDataColumnID] INT           IDENTITY (1, 1) NOT NULL,
    [MetaDataID]       INT           NOT NULL,
    [ColumnName]       VARCHAR (128) NOT NULL,
    [Position]         INT           NOT NULL,
    [Max_Length]       SMALLINT      NULL,
    [SortASC]          BIT           CONSTRAINT [DF_DocMetaDataColumn_SortASC] DEFAULT ((1)) NOT NULL,
    [SortPriority]     SMALLINT      NULL,
    CONSTRAINT [PK__FileVali__C890E3B36B595737] PRIMARY KEY CLUSTERED ([MetaDataColumnID] ASC),
    CONSTRAINT [CK__FileValid__Max_L__16CE6296] CHECK ([Max_Length] IS NULL OR [Max_Length]>(0)),
    CONSTRAINT [CK__FileValid__Posit__15DA3E5D] CHECK ([Position]>=(0)),
    CONSTRAINT [FK__FileValid__MetaD__14E61A24] FOREIGN KEY ([MetaDataID]) REFERENCES [SEIDR].[DocMetaData] ([MetaDataID]),
    CONSTRAINT [UQ__FileVali__C7331527E8D31338] UNIQUE NONCLUSTERED ([MetaDataID] ASC, [Position] ASC)
);



GO
CREATE UNIQUE NONCLUSTERED INDEX [idx_DocMetaDataColumn_MetaDataID_SortPriority]
    ON [SEIDR].[DocMetaDataColumn]([MetaDataID] ASC, [SortPriority] ASC) WHERE ([SortPriority] IS NOT NULL);

