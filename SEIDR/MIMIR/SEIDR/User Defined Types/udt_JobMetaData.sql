CREATE TYPE [SEIDR].[udt_JobMetaData] AS TABLE (
    [JobName]            VARCHAR (128) NOT NULL,
    [Description]        VARCHAR (256) NULL,
    [JobNameSpace]       VARCHAR (128) NOT NULL,
    [ThreadName]         VARCHAR (128) NULL,
    [SingleThreaded]     BIT           NOT NULL,
    [NotificationTime]   SMALLINT      NOT NULL,
    [ConfigurationTable] VARCHAR (255) NULL,
    [AllowRetry]         BIT           NOT NULL,
    [DefaultRetryTime]   INT           NULL,
    [NeedsFilePath]      BIT           NOT NULL);





