CREATE TABLE [SEIDR].[QueueRejection] (
    [QueueRejectionID]  INT           IDENTITY (1, 1) NOT NULL,
    [JobProfileID]      INT           NOT NULL,
    [InputFolder]       VARCHAR (500) NOT NULL,
    [DestinationFolder] VARCHAR (500) NOT NULL,
    [FileName]          VARCHAR (255) NOT NULL,
    [FIlePath]          VARCHAR (500) NOT NULL,
    [ProcessingDate]    DATETIME      NOT NULL,
    [Rejected]          BIT           NOT NULL,
    [Duplicate]         BIT           NOT NULL,
    [DC]                DATETIME      CONSTRAINT [DF_QueueRejection_DC] DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([QueueRejectionID] ASC),
    FOREIGN KEY ([JobProfileID]) REFERENCES [SEIDR].[JobProfile] ([JobProfileID])
);





