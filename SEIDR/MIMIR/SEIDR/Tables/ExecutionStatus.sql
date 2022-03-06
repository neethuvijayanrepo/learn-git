CREATE TABLE [SEIDR].[ExecutionStatus] (
    [ExecutionStatusCode] VARCHAR (2)   NOT NULL,
    [NameSpace]           VARCHAR (128) DEFAULT ('SEIDR') NOT NULL,
    [IsComplete]          BIT           DEFAULT ((0)) NOT NULL,
    [IsError]             BIT           DEFAULT ((0)) NOT NULL,
    [Description]         VARCHAR (60)  NULL,
	ExecutionStatus AS [NameSpace] + '.' + ExecutionStatusCode PERSISTED NOT NULL,
    PRIMARY KEY CLUSTERED ([NameSpace] ASC, [ExecutionStatusCode] ASC)
);

