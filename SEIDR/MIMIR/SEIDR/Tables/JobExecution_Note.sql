CREATE TABLE [SEIDR].[JobExecution_Note] (
    [NoteID]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [JobExecutionID]   BIGINT         NOT NULL,
    [StepNumber]       SMALLINT       NOT NULL,
    [JobProfile_JobID] INT            NULL,
    [NoteText]         VARCHAR (2000) NOT NULL,
    [DC]               DATETIME       DEFAULT (getdate()) NOT NULL,
    [UserName]         VARCHAR (128)  DEFAULT (suser_name()) NOT NULL,
    [NoteSequence]     SMALLINT       DEFAULT ((1)) NOT NULL,
    [StepNoteSequence] SMALLINT       DEFAULT ((1)) NOT NULL,
    [IsLatest]         BIT            DEFAULT ((1)) NOT NULL,
    [StepIsLatest]     BIT            DEFAULT ((1)) NOT NULL,
    [Technical]        BIT            DEFAULT ((0)) NOT NULL,
    [Auto]             BIT            DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([NoteID] ASC),
    CHECK ([IsLatest]=(0) OR [StepIsLatest]=(1)),
    CHECK ([NoteSequence]>=(1)),
    CHECK ([StepNoteSequence]>=(1)),
    FOREIGN KEY ([JobExecutionID]) REFERENCES [SEIDR].[JobExecution] ([JobExecutionID]),
    FOREIGN KEY ([JobProfile_JobID]) REFERENCES [SEIDR].[JobProfile_Job] ([JobProfile_JobID])
);


GO

CREATE TRIGGER SEIDR.trg_JobExecution_Note_i
ON SEIDR.JobExecution_Note after INSERT
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE n
	SET NoteSequence = e.Seq + 1,
		StepNoteSequence = se.StepSeq + 1
	FROM SEIDR.JobExecution_Note n
	JOIN INSERTED i	ON n.NoteID = i.NoteID
	CROSS APPLY(SELECT MAX(NoteSequence) Seq
				FROM SEIDR.JobExecution_Note
				WHERE JobExecutionID = n.JobExecutionID
				AND NoteID <> n.NoteID)e
	CROSS APPLY(SELECT COALESCE(MAX(StepNoteSequence), 0) StepSeq
				FROM SEIDR.JobExecution_Note
				WHERE JobExecutionID = n.JobExecutionID
				AND StepNumber = n.StepNumber
				AND NoteID <> n.NoteID)se
	WHERE e.Seq is not null

	UPDATE n
	SET IsLatest = 0,
		StepIsLatest = CASE WHEN n.StepNumber = i.StepNumber then 0 else n.StepIsLatest end
	FROM SEIDR.JobExecution_Note n
	JOIN INSERTED i
		ON n.JobExecutionID = i.JobExecutionID		
	WHERE n.NoteID < i.NoteID AND n.StepIsLatest = 1
END
GO