
CREATE TABLE [Security].[UserAccountSession](
	[SG] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UserID] [smallint] NOT NULL,
	[LoginDateTime] [datetime] NOT NULL,
	[IPAddress] [char](16) NOT NULL,
	[LastActionDateTime] [datetime] NOT NULL,
	[LogoutDateTime] [datetime] NULL,
	[WasForcedLogout] [bit] NULL,
	[RV] [int] NOT NULL,
	[LU] [datetime] NOT NULL,
	[DC] [datetime] NOT NULL,
	[DD] [datetime] NULL,
 CONSTRAINT [PK_UserAccountSession] PRIMARY KEY NONCLUSTERED 
(
	[SG] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY],
 CONSTRAINT [UQC_Session_SG_LoginDateTime] UNIQUE CLUSTERED 
(
	[LoginDateTime] ASC,
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [Security].[UserAccountSession] ADD  CONSTRAINT [DF_UserAccountSession_SG]  DEFAULT (newid()) FOR [SG]
GO

ALTER TABLE [Security].[UserAccountSession] ADD  CONSTRAINT [DF_UserAccountSession_RV]  DEFAULT ((0)) FOR [RV]
GO

ALTER TABLE [Security].[UserAccountSession] ADD  CONSTRAINT [DF_UserAccountSession_LU]  DEFAULT (getdate()) FOR [LU]
GO

ALTER TABLE [Security].[UserAccountSession] ADD  CONSTRAINT [DF_UserAccountSession_DC]  DEFAULT (getdate()) FOR [DC]
GO

ALTER TABLE [Security].[UserAccountSession] ADD  CONSTRAINT [FK_UserAccountSession_UserAccount] FOREIGN KEY([UserID])
REFERENCES [Security].[User] ([UserID])
GO

ALTER TABLE [Security].[UserAccountSession] CHECK CONSTRAINT [FK_UserAccountSession_UserAccount]
GO
