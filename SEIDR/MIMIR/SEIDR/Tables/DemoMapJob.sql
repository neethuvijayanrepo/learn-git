CREATE TABLE [SEIDR].[DemoMapJob] (
    [DemoMapID]             INT  IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]      INT                  NOT NULL,	
	[SkipLines]             INT DEFAULT ((0))    NOT NULL,
    [Delimiter] CHAR(3) NOT NULL DEFAULT '|', 
	[OutputDelimiter] CHAR(3) NOT NULL DEFAULT '|',
	[DoAPB]                 BIT DEFAULT ((0))    NOT NULL, --Use Project CRCM setting instead? Default it in configuration proc for now.	
    [Enable_OOO] BIT NOT NULL DEFAULT 0, 
	[OutputFolder]          VARCHAR (256)        NULL,
	[FilePageSize]          INT                  NULL ,
	[FileMapID]             INT                  NOT NULL, --PackageID	
	[FileMapDatabaseID]     INT			         NOT NULL FOREIGN KEY REFERENCES SEIDR.DatabaseLookup(DatabaseLookupID),
	[PayerLookupDatabaseID] INT                  NOT NULL FOREIGN KEY REFERENCES SEIDR.DatabaseLookup(DatabaseLookupID), --Potential Rename to staging?	
	_PatientBalanceUnavailable    bit NOT NULL DEFAULT(0),
	_InsuranceBalanceUnavailable  bit NOT NULL DEFAULT(0),
	_InsuranceDetailUnavailable	  bit NOT NULL DEFAULT(0),
	_PartialDemographicLoad		  bit NOT NULL DEFAULT(0),
    [OOO_InsuranceBalanceValidation] BIT NOT NULL DEFAULT 1, 
	HasHeaderRow bit not null default(1),
    CONSTRAINT [PK_DemoMap] PRIMARY KEY CLUSTERED ([DemoMapID] ASC) WITH (FILLFACTOR = 85),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);



