CREATE TABLE [SEIDR].[Job] (
    [JobID]              INT           IDENTITY (1, 1) NOT NULL,
    [JobName]            VARCHAR (128) NOT NULL,
    [Description]        VARCHAR (256) NULL,
    [JobNameSpace]       VARCHAR (128) NOT NULL,
    [ThreadName]         VARCHAR (128) NULL,
    [SingleThreaded]     BIT           NOT NULL,
    [DC]                 DATETIME      DEFAULT (getdate()) NOT NULL,
    [Loaded]             BIT           DEFAULT ((1)) NOT NULL,
    [LastLoad]           DATETIME      DEFAULT (getdate()) NOT NULL,
    [ConfigurationTable] VARCHAR (255) NULL,
    [AllowRetry]         BIT           DEFAULT ((1)) NOT NULL,
    [DefaultRetryTime]   INT           DEFAULT ((5)) NULL, -- Needs to fit the check constraint to be valid...
    [NeedsFilePath]      BIT           DEFAULT ((0)) NOT NULL,
    [NotificationTime]   SMALLINT      DEFAULT ((10)) NOT NULL,
    PRIMARY KEY CLUSTERED ([JobID] ASC),
    CONSTRAINT [CHK_JOB_DefaultRetryTime] CHECK (DefaultRetryTime is null OR [DefaultRetryTime]>=(5)),
    CONSTRAINT [CK_Validate_Only] CHECK (isnull(object_name(@@procid),'')='usp_Job_Validate' OR IS_SRVROLEMEMBER('sysadmin') = 1) -- Server role for publishing purposes.
);





