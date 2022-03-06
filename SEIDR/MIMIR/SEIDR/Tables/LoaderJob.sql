CREATE TABLE [SEIDR].[LoaderJob] (
    [JobProfile_JobID]   INT            NOT NULL,
    [ServerInstanceName] VARCHAR (128)  NULL,
    [AndromedaServer]    VARCHAR (128)  NULL,
    [OutputFolder]       VARCHAR (4000) NULL,
    [PackageID]          INT            NULL,
    [CB]                 VARCHAR (128)  CONSTRAINT [DF_LoaderJob_CB] DEFAULT (suser_name()) NOT NULL,
    [DC]                 SMALLDATETIME  CONSTRAINT [DF_LoaderJob_DC] DEFAULT (getdate()) NOT NULL,
    [LU]                 SMALLDATETIME  CONSTRAINT [DF_LoaderJob_LU] DEFAULT (getdate()) NOT NULL,
    [DD]                 SMALLDATETIME  NULL,
    [IsValid]            AS             (CONVERT([bit],case when [PackageID] IS NOT NULL AND [DD] IS NULL then (1) else (0) end)),
    [FacilityID]         SMALLINT       NULL,
    [OutputFileName]     VARCHAR (100)  NULL,
    [DatabaseName]       VARCHAR (128)  NULL,
    [Misc] VARCHAR(200) NULL, 
    [Misc2] VARCHAR(200) NULL, 
    [Misc3] VARCHAR(200) NULL, 
    [DatabaseConnectionManager] VARCHAR(200) NULL,
	[DatabaseConnection_DatabaseLookupID] [int] NULL, 
    [SecondaryFilePath] VARCHAR(4000) NULL, 
    [TertiaryFilePath] VARCHAR(4000) NULL, 
    CONSTRAINT [PK_LoaderJob] PRIMARY KEY CLUSTERED ([JobProfile_JobID] ASC),
    CONSTRAINT [FK_LoaderJob_JobProfile_Job] FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
	CONSTRAINT [FK_LoaderJob_DatabaseLookupID] FOREIGN KEY([DatabaseConnection_DatabaseLookupID]) REFERENCES [SEIDR].[DatabaseLookup] ([DatabaseLookupID])
);



