CREATE TABLE [REFERENCE].[Project] (
    [ProjectID]                      SMALLINT      NOT NULL,
    [Description]                    VARCHAR (150) NOT NULL,
    [CRCM]                           BIT           NOT NULL,
    [Modular]                        AS            (CONVERT([bit],(1)-[CRCM])) PERSISTED NOT NULL,
    [FromDate]                       DATE          DEFAULT (getdate()) NOT NULL,
    [ThroughDate]                    DATE          NULL,
    [DD]                             SMALLDATETIME NULL,
    [Active]                         AS            (CONVERT([bit],case when [DD] IS NOT NULL then (0) when [ThroughDate]<getdate() then (0) else (1) end)),
    [OrganizationID]                 INT           NOT NULL,
    [FTP_RootFolderOverride]         VARCHAR (500) NULL,
    [Source_RootFolderOverride]      VARCHAR (500) NULL,
    [Metrix_RootFolderName_Override] VARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([ProjectID] ASC),
    CONSTRAINT [CK_FromDate_throughDate] CHECK ([ThroughDate] IS NULL OR [ThroughDate]>[FromDate]),
    CONSTRAINT [FK__Project__Organiz__162F4418] FOREIGN KEY ([OrganizationID]) REFERENCES [REFERENCE].[Organization] ([OrganizationID]) ON UPDATE CASCADE
);





