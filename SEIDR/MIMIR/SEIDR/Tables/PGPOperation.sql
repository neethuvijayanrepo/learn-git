
	CREATE TABLE [SEIDR].[PGPOperation] (
    [PGPOperationID]          TINYINT       IDENTITY (1, 1) NOT NULL,
    [PGPOperationName]        VARCHAR (20)  NOT NULL,
    [PGPOperationDescription] VARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([PGPOperationID] ASC) WITH (FILLFACTOR = 80)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_PGPOperation_PGPOperationName]
    ON [SEIDR].[PGPOperation]([PGPOperationName] ASC) WITH (FILLFACTOR = 85);

