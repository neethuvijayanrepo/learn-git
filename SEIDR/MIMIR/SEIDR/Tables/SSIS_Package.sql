CREATE TABLE [SEIDR].[SSIS_Package] (
    [PackageID]   INT           IDENTITY (1, 1) NOT NULL,
    [Category]    VARCHAR (128) NOT NULL,
    [Name]        VARCHAR (128) NOT NULL,
    [ServerName]  VARCHAR (130) NULL,
    [PackagePath] VARCHAR (500) NOT NULL,
    [CB]          VARCHAR (128) CONSTRAINT [DF_SSIS_Package_CB] DEFAULT (suser_name()) NOT NULL,
    [DC]          SMALLDATETIME CONSTRAINT [DF_SSIS_Package_DC] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SSIS_Package] PRIMARY KEY CLUSTERED ([PackageID] ASC)
);



