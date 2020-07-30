CREATE TABLE [Security].[User](
	      [UserID]               SMALLINT IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	      [UserName]			 VARCHAR(100) NOT NULL,
		  [UserPassword]		 VARBINARY(128) NOT NULL,
	      [FirstName]			 VARCHAR(30) NOT NULL,
		  [MiddleName]			 VARCHAR(30) NULL,
	      [LastName]			 VARCHAR(30) NOT NULL,
	      [Email]				 VARCHAR(100) NOT NULL,
	      [DisplayName]			 AS RTRIM(LTRIM(CONCAT([FirstName] + ' ', COALESCE([MiddleName] + ' ', ''),[LastName]))) PERSISTED NOT NULL,
		  [AccountLockedOut]	 BIT NOT NULL,
	      [DC]					 SMALLDATETIME CONSTRAINT [DF_User_DC] DEFAULT (GETDATE()) NOT NULL,
		  [UIDC]				 SMALLINT NOT NULL, 
	      [LU]					 SMALLDATETIME CONSTRAINT [DF_User_LU] DEFAULT (GETDATE()) NOT NULL,
		  [UILU]			     SMALLINT NOT NULL, 
		  [Status]				 INT NOT NULL DEFAULT (1),

 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserID] ASC),
 );