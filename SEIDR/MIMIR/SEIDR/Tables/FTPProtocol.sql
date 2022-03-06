CREATE TABLE [SEIDR].[FTPProtocol] (
    [FTPProtocolID] INT           IDENTITY (1, 1) NOT NULL,
    [Protocol]      NVARCHAR (20) NULL,
    [DD]            SMALLDATETIME NULL,
    CONSTRAINT [PK_FTPProtocol] PRIMARY KEY CLUSTERED ([FTPProtocolID] ASC) WITH (FILLFACTOR = 85)
);

