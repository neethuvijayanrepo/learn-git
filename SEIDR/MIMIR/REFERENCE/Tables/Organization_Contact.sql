CREATE TABLE REFERENCE.[Organization_Contact]
(
	[Organization_ContactID] INT NOT NULL identity(1,1) PRIMARY KEY,
	OrganizationID int not null foreign key references REFERENCE.Organization(OrganizationID) ON UPDATE CASCADE,
	ProjectID smallint null foreign key references REFERENCE.Project(ProjectID),
	UserKey varchar(50) not null foreign key references REFERENCE.UserKey(UserKey),
	ContactID int not null foreign key REFERENCES [REFERENCE].[Contact](ContactID),
	FromDate date NOT NULL,
	ThroughDate date,
	DC datetime not null default(GETDATE()),
	Active as(CASE WHEN ThroughDate is null OR ThroughDate > GETDATE() then 1 else 0 end),	
)

