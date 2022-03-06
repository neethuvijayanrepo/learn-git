CREATE TABLE [SEIDR].[FileValidationJob] (
    [FileValidationJobID]        INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]           INT           NOT NULL,
    [SkipLines]                  INT           DEFAULT ((0)) NOT NULL,
    [HasHeader]                  BIT           DEFAULT ((1)) NOT NULL,
    [DoMetaDataConfiguration]    BIT           DEFAULT ((1)) NOT NULL,
    [CurrentMetaDataVersion]     INT           NULL,
    [TextQualifier]              VARCHAR (5)   DEFAULT ('"') NOT NULL,
    [Delimiter]                  VARCHAR (1)   NULL,
    [SizeThreshold]              INT           NULL,
    [SizeThresholdDayRange]      TINYINT       NULL,
    [NotificationList]           VARCHAR (300) NULL,
    [HasTrailer]                 BIT           DEFAULT ((0)) NOT NULL,
    [RemoveTextQual]             BIT           DEFAULT ((0)) NOT NULL,
    [SizeThresholdWarningMode]   BIT           CONSTRAINT [DF_FileValidationJob_SizeThresholdWarningMode] DEFAULT ((0)) NOT NULL,
    [TextQualifyColumnNumber]    INT           NULL,
    [MinimumColumnCountForMerge] INT           DEFAULT ((0)) NOT NULL,
    [KeepOriginal]               BIT           DEFAULT ((1)) NOT NULL,
    [OverrideExtension]          VARCHAR (10)  NULL,
    [LineEnd_CR]                 BIT           DEFAULT ((1)) NOT NULL,
    [LineEnd_LF]                 BIT           DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([FileValidationJobID] ASC),
    CHECK ([SizeThreshold] IS NULL OR [SizeThreshold]>=(0) AND [SizeThreshold]<=(100)),
    CHECK ([SkipLines]>=(0)),
    CONSTRAINT [CK_LineEnd] CHECK ([LineEnd_CR]=(1) OR [LineEnd_LF]=(1)),
    UNIQUE NONCLUSTERED ([JobProfile_JobID] ASC)
);






