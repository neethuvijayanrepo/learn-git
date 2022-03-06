CREATE TABLE [SEIDR].[JobReport]
(
	[JobReportID] INT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
    [ReportName] VARCHAR(130) NOT NULL, 
    [SQLProcedure] VARCHAR(258) NOT NULL, 
    [Recipient] VARCHAR(500) NULL, 
    [Mode] TINYINT NULL, 
    [LastExecution] DATE NULL, 
    [ArchiveFolder] VARCHAR(300) NULL, 
    [DatabaseLookupID] INT NULL FOREIGN KEY REFERENCES SEIDR.DatabaseLookup(DatabaseLookupID)
)
