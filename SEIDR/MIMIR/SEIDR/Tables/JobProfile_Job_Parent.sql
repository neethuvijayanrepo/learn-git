CREATE TABLE [SEIDR].[JobProfile_Job_Parent] (
    [JobProfile_Job_ParentID]    INT      IDENTITY (1, 1) NOT NULL,
    [JobProfile_JobID]           INT      NOT NULL,
    [Parent_JobProfile_JobID]    INT      NOT NULL,
    [SequenceDayMatchDifference] SMALLINT DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Bridge] PRIMARY KEY CLUSTERED ([JobProfile_Job_ParentID] ASC),
    CHECK ([Parent_JobProfile_JobID]<>[JobProfile_JobID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),
    FOREIGN KEY ([Parent_JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID]),    
);

