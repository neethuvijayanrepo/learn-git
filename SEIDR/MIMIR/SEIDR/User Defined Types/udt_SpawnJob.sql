CREATE TYPE [SEIDR].[udt_SpawnJob] AS TABLE 
(
	[SpawnJobID]		INT NOT NULL,
	[JobProfile_JobID]	INT NOT NULL,
	[JobProfileID]		INT NOT NULL,
	[SourceFile]			NVARCHAR(500) NOT NULL,
	[FileSize]			BIGINT NOT NULL,
	[FileHash]			VARCHAR(88) NOT NULL,
    PRIMARY KEY CLUSTERED ([SpawnJobID] ASC)
);