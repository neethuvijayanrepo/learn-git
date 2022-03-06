CREATE PROCEDURE SEIDR.usp_ExecutionStatus_sl_Description
AS
BEGIN
	SELECT ExecutionStatusCode,NameSpace,Description 
	FROM SEIDR.ExecutionStatus WITH (NOLOCK) 
END