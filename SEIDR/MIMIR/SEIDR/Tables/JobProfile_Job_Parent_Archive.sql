CREATE TABLE [SEIDR].[JobProfile_Job_Parent_Archive] (
    [ArchiveID]                  INT           IDENTITY (1, 1) NOT NULL,
    [JobProfile_Job_ParentID]    INT           NOT NULL,
    [JobProfile_JobID]           INT           NOT NULL,
    [Parent_JobProfile_JobID]    INT           NOT NULL,
    [SequenceDayMatchDifference] INT           NOT NULL,
    [DC]                         DATETIME      DEFAULT (getdate()) NOT NULL,
    [DeletingUser]               VARCHAR (128) DEFAULT (suser_name()) NOT NULL,
    PRIMARY KEY CLUSTERED ([ArchiveID] ASC)
);

