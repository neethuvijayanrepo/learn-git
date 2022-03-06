CREATE FUNCTION UTIL.ufn_ForeignKey_GetCircularLevel
(@ConstraintID int
)
RETURNS INT
AS
BEGIN
	DECLARE @StartObj int
	DECLARE @Level int = -1
	SELECT @StartObj = PARENT_OBJECT_ID
	FROM sys.foreign_keys
	WHERE OBJECT_ID = @ConstraintID

	IF @StartObj = (SELECT REFERENCED_OBJECT_ID FROM sys.foreign_keys WHERE OBJECT_ID = @ConstraintID)
		RETURN 0

	DECLARE @Ref table(ID int identity(1,1) primary key, KeyID int, Processed bit, LevelID int)
	INSERT INTO @Ref(KeyID, Processed, LevelID)
	SELECT fk.OBJECT_ID, 0, 1
	FROM sys.FOREIGN_KEY_COLUMNS fkc
	JOIN sys.FOREIGN_KEYS fk
		ON fkc.REFERENCED_OBJECT_ID = fk.PARENT_OBJECT_ID
	WHERE fkc.CONSTRAINT_OBJECT_ID = @ConstraintID
	IF @@ROWCOUNT = 0
		RETURN @Level

	WHILE EXISTS(SELECT null FROM @Ref WHERE PRocessed = 0)
	BEGIN
		UPDATE TOP (1) r
		SET Processed = 1,
			@ConstraintID = KeyID,
			@Level = LevelID
		FROM @Ref r
		WHERE Processed = 0
		
		IF @@ROWCOUNT = 0
		BEGIN
			SET @Level = -1
			BREAK
		END

		
		IF EXISTS(SELECT null 
					FROM sys.FOREIGN_KEYS 
					WHERE OBJECT_ID = @ConstraintID 
					AND (REFERENCED_OBJECT_ID = @StartObj
						OR Parent_Object_ID = @StartObj)
					)
		BEGIN
			BREAK
		END
		SET @Level = -1

		INSERT INTO @Ref(KeyID, LevelID, Processed)
		SELECT fk.OBJECT_ID, @Level + 1, 0
		FROM sys.FOREIGN_KEY_COLUMNS fkc
		JOIN sys.FOREIGN_KEYS fk
			ON fkc.REFERENCED_OBJECT_ID = fk.PARENT_OBJECT_ID
		WHERE fkc.CONSTRAINT_OBJECT_ID = @ConstraintID
		AND  NOT EXISTS(SELECT null FROM @Ref WHERE KeyID = fk.OBJECT_ID)
	END
	RETURN @Level
END
-- ufn_ForeignKey_CheckDescendentTable