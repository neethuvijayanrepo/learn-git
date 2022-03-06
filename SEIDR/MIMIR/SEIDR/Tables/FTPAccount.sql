CREATE TABLE [SEIDR].[FTPAccount] (
    [FTPAccountID]   INT            IDENTITY (1, 1) NOT NULL,
    [FTPProtocolID]  INT            NOT NULL,
    [Server]         NVARCHAR (255) NOT NULL,
    [UserName]       NVARCHAR (50)  NULL,
    [Password]       VARCHAR (255)  NULL,
    [Port]           INT            NULL,
    [Passive]        BIT            CONSTRAINT [DF_FTPAccount_Passive] DEFAULT ((0)) NOT NULL,
    [Fingerprint]    NVARCHAR (100) NULL,
    [Description]    VARCHAR (100)  NOT NULL,
    [PpkFileName]    NVARCHAR (100) NULL,
    [DD]             SMALLDATETIME  NULL,
    [ProjectID]      SMALLINT       NULL,
    [OrganizationID] INT            NULL,
    [DC]             SMALLDATETIME  DEFAULT (getdate()) NOT NULL,
	[TransferResumeSupport] BIT DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_FTPAccount] PRIMARY KEY CLUSTERED ([FTPAccountID] ASC) WITH (FILLFACTOR = 85),
    CONSTRAINT [CK_FTP_Account_Project_Organization] CHECK ([REFERENCE].[ufn_Check_Project_Organization]([ProjectID],[OrganizationID])=(1)),
    FOREIGN KEY ([FTPProtocolID]) REFERENCES [SEIDR].[FTPProtocol] ([FTPProtocolID]),
    CONSTRAINT [FK_FTPAccount_Organization] FOREIGN KEY ([OrganizationID]) REFERENCES [REFERENCE].[Organization] ([OrganizationID]) ON UPDATE CASCADE,
    CONSTRAINT [FK_FTPAccount_Project] FOREIGN KEY ([ProjectID]) REFERENCES [REFERENCE].[Project] ([ProjectID]),
    UNIQUE NONCLUSTERED ([Description] ASC)
);













