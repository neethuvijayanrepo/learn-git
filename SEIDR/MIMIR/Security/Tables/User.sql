CREATE TABLE [SECURITY].[User] (
    [UserID]               SMALLINT      IDENTITY (1, 1) NOT NULL,
    [UserName]             VARCHAR (260) NOT NULL,
    [LastName]             VARCHAR (130) NOT NULL,
    [FirstName]            VARCHAR (130) NOT NULL,
    [NickName]             VARCHAR (130) NULL,
    [EmailAddress]         VARCHAR (260) NULL,
    [DomainName]           AS            ([SECURITY].[ufn_GetDomainName]([UserName])),
    [TimeZoneOffset]       SMALLINT      DEFAULT ((0)) NOT NULL,
    [DC]                   DATETIME      DEFAULT (getdate()) NOT NULL,
    [DD]                   SMALLDATETIME NULL,
    [CB]                   VARCHAR (260) DEFAULT (suser_name()) NOT NULL,
    [CB_UserID]            SMALLINT      NULL,
    [LU]                   DATETIME      DEFAULT (getdate()) NOT NULL,
    [UID]                  SMALLINT      NULL,
    [DisplayName]          AS            (case when [UserName]=[LastName] AND [UserName]=[FirstName] then [UserName] else (coalesce([NickName],[FirstName])+' ')+[LastName] end) PERSISTED NOT NULL,
    [DisplayNameLastFirst] AS            (case when [UserName]=[LastName] AND [UserName]=[FirstName] then [UserName] else ([LastName]+', ')+coalesce([NickName],[FirstName]) end) PERSISTED NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC),
    UNIQUE NONCLUSTERED ([UserName] ASC)
);

