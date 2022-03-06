CREATE PROCEDURE UTIL.usp_IdentityGap_Range_sl
	@ObjectName varchar(260) = null,
	@ObjectID int = null,
	@MinRange int = 1
AS
BEGIN
IF @ObjectID is not null
	SELECT @ObjectName = QUOTENAME(OBJECT_SCHEMA_NAME(@ObjectID)) + '.' + QUOTENAME(OBJECT_NAME(@ObjectID))
ELSE
	SELECT @ObjectID = OBJECT_ID(@OBJECTNAME)

IF @ObjectID is null or @ObjectName is null
BEGIN
	RAISERROR('Invalid Object parameters (Name: %s, ID: %d)', 16, 1, @ObjectName, @ObjectID)
	RETURN
END
DECLARE @IDCol nvarchar(130)
IF @MinRange < 1
BEGIN
	RAISERROR('Invalid @MinRange: %d .... Setting to 1.', 16, 1, @MinRange)
	SET @MinRange = 1
END

SELECT @IDCol = name
FROM sys.identity_columns 
WHERE OBJECT_ID = @ObjectID

DECLARE @SQL nvarchar(4000) = N'
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT [' + @IDCol + N'], NextID, NextID - [' + @IDCol + N'] - 1 [GapRecordCount], [' + @IDCol + N'] + 1 [GapStart], NextID - 1 [GapEnd]
FROM ' + @ObjectName + N' je
CROSS APPLY(SELECT MIN(['+ @IDCol + N']) NextID
			FROM ' + @ObjectName + N'
			WHERE [' + @IDCol + N'] > je.[' + @IDCol + N']) r2
WHERE 1=1
AND NOT EXISTS(SELECT null FROM ' + @ObjectName + N' WHERE [' + @IDCol + N'] = je.[' + @IDCol + N'] + 1)
AND NextID > [' + @IDCol + '] + ' + CONVERT(nvarchar(30), @MinRange) + N'
ORDER BY GapRecordCount desc'
print @SQL
exec (@SQL)

END