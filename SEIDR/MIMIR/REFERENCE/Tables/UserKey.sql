CREATE TABLE [REFERENCE].[UserKey] (
    [UserKey]                                 VARCHAR (50)  NOT NULL,
    [Description]                             VARCHAR (130) NOT NULL,
    [DC]                                      SMALLDATETIME DEFAULT (getdate()) NOT NULL,
    [OverrideOrganizationDefaultPriorityCode] VARCHAR (10)  NULL,
	Inbound bit not null default(1),
	Outbound bit not null default(0),
	VendorSpecific bit not null default(0),
    [LU] SMALLDATETIME NOT NULL default(GETDATE()), 
    PRIMARY KEY CLUSTERED ([UserKey] ASC),    
    FOREIGN KEY ([OverrideOrganizationDefaultPriorityCode]) REFERENCES [SEIDR].[Priority] ([PriorityCode])
);

