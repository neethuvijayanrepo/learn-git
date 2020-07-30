CREATE TABLE [Security].[LoginAttemptFailure](
	[LoginAttemptFailureID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[UserPassword] [varchar](50) NOT NULL,
	[LoginAttemptDateTime] [datetime] NOT NULL,
	[IPAddress] [char](16) NOT NULL,
	[DC] [datetime] NOT NULL,
 CONSTRAINT [PK_LoginAttemptFailure] PRIMARY KEY NONCLUSTERED 
(
	[LoginAttemptFailureID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [Security].[LoginAttemptFailure] ADD  CONSTRAINT [DF_LoginAttemptFailure_DC]  DEFAULT (getdate()) FOR [DC]
GO
