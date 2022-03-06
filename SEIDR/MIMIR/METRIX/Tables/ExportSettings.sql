CREATE TABLE [METRIX].[ExportSettings] (
    [ExportSettingsId]       INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]       INT           NOT NULL,
    [ArchiveLocation]        VARCHAR (500) NULL,
    [MetrixDatabaseLookupID] INT           NULL,
    [VendorID] SMALLINT NULL FOREIGN KEY REFERENCES METRIX.Vendor(VendorID), 
	[ExportTypeID] TINYINT null FOREIGN KEY REFERENCES METRIX.ExportType(ExportTypeID),
    [ImportTypeID] TINYINT null FOREIGN KEY REFERENCES METRIX.ImportType(ImportTypeID), 
    CONSTRAINT [PK_ExportSettings] PRIMARY KEY CLUSTERED ([ExportSettingsId] ASC),
    CONSTRAINT [FK_ExportSettings_DatabaseLookup] FOREIGN KEY ([MetrixDatabaseLookupID]) REFERENCES [SEIDR].[DatabaseLookup] ([DatabaseLookupID]),
    CONSTRAINT [FK_ExportSettings_JobProfile_Job] FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);

