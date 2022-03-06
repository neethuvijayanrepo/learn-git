IF NOT EXISTS (SELECT 1 FROM [SEIDR].[ExecutionStatus] WHERE [ExecutionStatusCode] = 'C' AND [NameSpace] = 'METRIX_EXPORT')
BEGIN
	INSERT INTO [SEIDR].[ExecutionStatus] ([ExecutionStatusCode],[NameSpace],[IsComplete],[IsError],[Description])  
	VALUES ('C', 'METRIX_EXPORT', 1, 0, 'Metrix Export Status Updated')
END
