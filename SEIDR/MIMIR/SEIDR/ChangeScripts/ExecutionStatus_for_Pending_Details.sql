
IF NOT EXISTS (SELECT 1 FROM [SEIDR].[ExecutionStatus] WHERE [ExecutionStatusCode] = 'PD' AND [NameSpace] = 'SEIDR')
BEGIN
	INSERT INTO [SEIDR].[ExecutionStatus] ([ExecutionStatusCode],[NameSpace],[IsComplete],[IsError],[Description])  
	VALUES ('PD', 'SEIDR', 0, 1, 'Pending Details')
END
