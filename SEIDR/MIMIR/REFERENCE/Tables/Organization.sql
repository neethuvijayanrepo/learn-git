CREATE TABLE [REFERENCE].[Organization] (
    [OrganizationID]          INT           NOT NULL,
    [Description]             VARCHAR (100) NOT NULL,
    [DC]                      SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [ParentOrganizationID]    INT           NULL,
    [DefaultPriorityCode]     VARCHAR (10)  DEFAULT ('NORMAL') NOT NULL,
    [FTP_RootFolder]          VARCHAR (500) NULL,
    [Source_RootFolder]       VARCHAR (500) NULL,
    [Metrix_RootFolderName]   VARCHAR (100) NULL,
    [OrganizationThroughDate] DATE          NULL,
    PRIMARY KEY CLUSTERED ([OrganizationID] ASC),
    CONSTRAINT [CK_Organization_ParentOrganization] CHECK ([ParentOrganizationID] IS NOT NULL AND [ParentOrganizationID]<>[OrganizationID] OR [OrganizationID]=(0) AND [ParentOrganizationID] IS NULL),
    FOREIGN KEY ([DefaultPriorityCode]) REFERENCES [SEIDR].[Priority] ([PriorityCode]),
    CONSTRAINT [FK__Organizat__Paren__1AF3F935] FOREIGN KEY ([ParentOrganizationID]) REFERENCES [REFERENCE].[Organization] ([OrganizationID])
);







