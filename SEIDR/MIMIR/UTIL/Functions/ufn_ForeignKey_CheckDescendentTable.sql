CREATE FUNCTION UTIL.ufn_ForeignKey_CheckDescendentTable
(	
	@PossibleAncestorObjectID int,
	@PossibleDescendentObjectID int
)
RETURNS BIT
AS
BEGIN
	DECLARE @Ref Table(ID int identity(1,1) primary key, 
						KeyID int, 
						PARENT_OBJECT_ID int,
						LevelID int not null default(0)
					)
	DECLARE @RC int, @Level int = 1
	INSERT INTO @Ref(KeyID, PARENT_OBJECT_ID)
	SELECT fk.OBJECT_ID, fk.PARENT_OBJECT_ID
	FROM SYS.FOREIGN_KEYS fk
	WHERE REFERENCED_OBJECT_ID = @PossibleAncestorObjectID

	IF @@ROWCOUNT = 0
		RETURN 0

	WHILE 1=1
	BEGIN
		INSERT INTO @Ref(KeyID, PARENT_OBJECT_ID)
		SELECT fk.OBJECT_ID, fk.PARENT_OBJECT_ID
		FROM SYS.FOREIGN_KEYS fk
		JOIN @Ref r
			ON fk.REFERENCED_OBJECT_ID = r.PARENT_OBJECT_ID
		WHERE r.LevelID = @Level
		AND NOT EXISTS(SELECT null 
						from @REF 
						WHERE KeyID = fk.OBJECT_ID)

		SELECT @RC = @@ROWCOUNT		
		IF EXISTS(SELECT null
					FROM @Ref
					WHERE PARENT_OBJECT_ID = @PossibleDescendentObjectID)
		BEGIN
			RETURN 1
		END
		ELSE IF @RC = 0
			BREAK

		SET @Level += 1

	END
	RETURN 0
END
-- ufn_ForeignKey_CheckDescendentTable_Exclusion