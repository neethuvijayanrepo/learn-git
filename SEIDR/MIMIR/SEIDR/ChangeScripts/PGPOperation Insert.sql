SET IDENTITY_INSERT [SEIDR].[PGPOperation] ON;

IF NOT EXISTS (SELECT 1 FROM [SEIDR].[PGPOperation] WHERE [PGPOperationID] = 0)
BEGIN
	INSERT INTO [SEIDR].[PGPOperation] ([PGPOperationID], [PGPOperationName], [PGPOperationDescription])
	VALUES (0, 'GenerateKey', 'Perform key files generation')
END

IF NOT EXISTS (SELECT 1 FROM [SEIDR].[PGPOperation] WHERE [PGPOperationID] = 1)
BEGIN
	INSERT INTO [SEIDR].[PGPOperation] ([PGPOperationID], [PGPOperationName], [PGPOperationDescription])
	VALUES (1, 'Encrypt', 'Perform file encrypt')
END

IF NOT EXISTS (SELECT 1 FROM [SEIDR].[PGPOperation] WHERE [PGPOperationID] = 2)
BEGIN
	INSERT INTO [SEIDR].[PGPOperation] ([PGPOperationID], [PGPOperationName], [PGPOperationDescription])
	VALUES (2, 'Decrypt', 'Perform file decrypt')
END

IF NOT EXISTS (SELECT 1 FROM [SEIDR].[PGPOperation] WHERE [PGPOperationID] = 3)
BEGIN
	INSERT INTO [SEIDR].[PGPOperation] ([PGPOperationID], [PGPOperationName], [PGPOperationDescription])
	VALUES (3, 'SignAndEncrypt', 'Perform file sign and encrypt')
END

IF NOT EXISTS (SELECT 1 FROM [SEIDR].[PGPOperation] WHERE [PGPOperationID] = 4)
BEGIN
	INSERT INTO [SEIDR].[PGPOperation] ([PGPOperationID], [PGPOperationName], [PGPOperationDescription])
	VALUES (4, 'Sign', 'Perform file sign')
END
SET IDENTITY_INSERT [SEIDR].[PGPOperation] OFF

GO