CREATE FUNCTION UTIL.ufn_GetForeignKey_JoinCondition
(
	@ConstraintID int,
	@AliasReferencing varchar(128),
	@AliasReferenced varchar(128)
)
RETURNS VARCHAR(max)
AS
BEGIN
	DECLARE @ret varchar(max) = ' ON '

	SELECT @Ret += @AliasReferencing + '.[' + COL_NAME(PARENT_OBJECT_ID, PARENT_COLUMN_ID) + '] = ' 
					+ @AliasReferenced + '.[' + COL_NAME(REFERENCED_OBJECT_ID, REFERENCED_COLUMN_ID) + '] AND '
	FROM sys.foreign_key_Columns 
	WHERE CONSTRAINT_OBJECT_ID = @ConstraintID

	RETURN SUBSTRING(@Ret, 1, LEN(@RET) - 4)
END
-- ufn_ForeignKey_GetCircularLevel