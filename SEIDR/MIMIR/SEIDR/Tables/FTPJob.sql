CREATE TABLE [SEIDR].[FTPJob] (
    [FTPJobID]         INT             IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID] INT             NOT NULL,
    [FTPAccountID]     INT             NOT NULL,
    [FTPOperationID]   TINYINT         NOT NULL,
    [LocalPath]        NVARCHAR (4000) NULL,
    [RemotePath]       VARCHAR (1000)  NOT NULL,
    [RemoteTargetPath] NVARCHAR (1000) NULL,
    [Exclude]          NVARCHAR (4000) NULL,
    [Include]          NVARCHAR (4000) NULL,
    [Overwrite]        BIT             DEFAULT ((0)) NOT NULL,
    [Delete]           BIT             CONSTRAINT [DF_FTP_Delete] DEFAULT ((0)) NOT NULL,
    [DateFlag]         BIT             DEFAULT ((1)) NOT NULL,
    [DD]               SMALLDATETIME   NULL,
    [Active]           AS              (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED NOT NULL,
    CONSTRAINT [PK_FTP] PRIMARY KEY CLUSTERED ([FTPJobID] ASC) WITH (FILLFACTOR = 85),
    FOREIGN KEY ([FTPAccountID]) REFERENCES [SEIDR].[FTPAccount] ([FTPAccountID]),
    FOREIGN KEY ([FTPOperationID]) REFERENCES [SEIDR].[FTPOperation] ([FTPOperationID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);





