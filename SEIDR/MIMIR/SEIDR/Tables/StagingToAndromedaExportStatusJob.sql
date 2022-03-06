CREATE TABLE [SEIDR].[StagingToAndromedaExportStatusJob]
(	
	JobProfile_JobID int not null PRIMARY KEY FOREIGN KEY REFERENCES SEIDR.JobProfile_Job(JobProfile_JobID),	
	CheckProject bit not null default(1),
	CheckOrganization bit not null default(1),
	IgnoreProcessingDate bit not null default(0),
	RequireCurrentProcessingDate bit not null default(0),
	MonitoredOnly bit not null default(1),
	IgnoreUnusedProfiles bit not null default(1),
	LoadBatchTypeList varchar(300) not null,
	DatabaseLookupID int not null FOREIGN KEY REFERENCES SEIDR.DatabaseLookup(DatabaseLookupID),
		
	DC datetime not null default(GETDATE()),
	CHECK(CheckProject =1  OR CheckOrganization = 1)
)
