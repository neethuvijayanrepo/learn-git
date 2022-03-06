CREATE TABLE [SEIDR].[DocMetaData] (
    [MetaDataID]        INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]  INT           NOT NULL,
    [Version]           INT           NOT NULL,
    [FromDate]          SMALLDATETIME CONSTRAINT [DF__FileValid__FromD__0E391C95] DEFAULT (getdate()) NOT NULL,
    [ThroughDate]       SMALLDATETIME NULL,
    [Delimiter]         CHAR (1)      NULL,
    [TextQualifier]     VARCHAR (5)   CONSTRAINT [DF__FileValid__TextQ__0F2D40CE] DEFAULT ('"') NOT NULL,
    [HasHeader]         BIT           CONSTRAINT [DF__FileValid__HasHe__10216507] DEFAULT ((1)) NOT NULL,
    [SkipLines]         INT           CONSTRAINT [DF__FileValid__SkipL__11158940] DEFAULT ((0)) NOT NULL,
    [HasTrailer]        BIT           CONSTRAINT [DF__FileValid__HasTr__1209AD79] DEFAULT ((0)) NOT NULL,
    [DuplicateHandling] VARCHAR (50)  NULL,
    [IsCurrent]         AS            (CONVERT([bit],case when [ThroughDate] IS NULL then (1) else (0) end)) PERSISTED,
    [DD]                SMALLDATETIME NULL,
    [Active]            AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)) PERSISTED,
    CONSTRAINT [PK__FileVali__429BA0ADC4D271ED] PRIMARY KEY CLUSTERED ([MetaDataID] ASC),
    CONSTRAINT [CK_DocMetaData_FromDate_BeforeThroughDate] CHECK ([FromDate]<=[ThroughDate] OR [ThroughDate] IS NULL),
    CONSTRAINT [UQ__FileVali__BD9355DDC43C29FA] UNIQUE NONCLUSTERED ([JobProfile_JobID] ASC, [Version] ASC)
);













GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_FileValidationDocMetaData_FromDate_ThroughDate]
    ON [SEIDR].[DocMetaData]([JobProfile_JobID] ASC, [FromDate] ASC, [ThroughDate] ASC) WHERE ([DD] IS NULL);

