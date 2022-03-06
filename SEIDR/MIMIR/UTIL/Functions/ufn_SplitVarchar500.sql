CREATE FUNCTION [UTIL].[ufn_SplitVarchar500]
(
	@Split varchar(5),
	@Input varchar(max)
)
RETURNS @Result TABLE(ID int identity(1,1) primary key, Value varchar(500))
AS
BEGIN
	--DECLARE @Result UTIL.udt_Varchar500
	
	DECLARE @Position int = 1
	DECLARE @toPosition int = CHARINDEX(@Split, @Input, @Position)
	WHILE @toPosition > 0
	BEGIN
		INSERT INTO @Result([Value])
		SELECT SUBSTRING(@Input, @Position, @ToPosition - @Position)

		SELECT @Position = @ToPosition + LEN(@Split),
				@ToPosition = CHARINDEX(@Split, @Input, @Position)
	END
	IF @Position <= LEN(@Input)
	BEGIN
		INSERT INTO @Result([Value]) -- If @Split is not found, or if we just do not end on a @Split.
		SELECT SUBSTRING(@Input, @Position, LEN(@Input))
	END
	
	RETURN --@Result
END
