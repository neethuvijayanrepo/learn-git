CREATE TABLE [SEIDR].[FTPOperation] (
    [FTPOperationID] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Operation]      TINYINT        NOT NULL,
    [OperationName]  NVARCHAR(50)     NOT NULL,
    [Description]    NVARCHAR (255) NULL,
    [DD]             SMALLDATETIME  NULL,
    CONSTRAINT [PK_FTPOperation] PRIMARY KEY CLUSTERED ([FTPOperationID] ASC) WITH (FILLFACTOR = 85)
);

