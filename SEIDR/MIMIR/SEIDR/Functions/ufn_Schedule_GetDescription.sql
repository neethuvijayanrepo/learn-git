CREATE FUNCTION [SEIDR].[ufn_Schedule_GetDescription](@ScheduleID int, @TimeOffset int)
RETURNS VARCHAR(2000)
AS
BEGIN
	DECLARE @Desc varchar(2000) = ''
	DECLARE @ScheduleRuleClusterID int
	DECLARE @ScheduleRuleID int
	
	--DECLARE @ScheduleID int = 769
	DECLARE @ClusterList table (Rn1 int primary key, ScheduleRuleClusterID int not null)	
	;WITH CTE AS (SELECT s.ScheduleRuleClusterID, 
			ROW_NUMBER() OVER(ORDER BY PartOfDateType, PartOfDate, IntervalType, IntervalValue) rn1,
			ROW_NUMBER() OVER(PARTITION BY s.ScheduleRuleClusterID ORDER BY  PartOfDateType, PartOfDate, IntervalType, IntervalValue) rn2
			FROM SEIDR.Schedule_ScheduleRuleCluster s
			JOIN SEIDR.ScheduleRuleCluster_ScheduleRule src WITH (NOLOCK)
				ON s.ScheduleRuleClusterID = src.ScheduleRuleClusterID
			JOIN SEIDR.ScheduleRule r WITH (NOLOCK)
				ON src.ScheduleRuleID = r.ScheduleRuleID
			WHERE ScheduleID = @ScheduleID
	) 
	INSERT INTO @ClusterList
	SELECT rn1, ScheduleRuleClusterID
	FROM CTE 
	WHERE Rn2 = 1
	/*
	SELECT * 
	FROM @ClusterList c
	JOIN SEIDR.ScheduleRuleCluster_ScheduleRule src
		ON c.ScheduleRuleClusterID = src.ScheduleRuleClusterID
	JOIN SEIDR.ScheduleRule r
		ON src.ScheduleRuleId = r.ScheduleRuleID
	ORDER BY c.Rn1*/
	
	DECLARE Cluster_Cursor cursor LOCAL FAST_FORWARD
	FOR SELECT ScheduleRuleClusterID
	FROM @ClusterList
	ORDER BY rn1
	
	OPEN CLUSTER_CURSOR
	FETCH NEXT FROM Cluster_Cursor 
	INTO @ScheduleRuleClusterID
	
	DECLARE @RC int = @@FETCH_STATUS

	WHILE @RC = 0
	BEGIN
		DECLARE Rule_Cursor CURSOR LOCAL FAST_FORWARD
		FOR SELECT src.ScheduleRuleID
		FROM SEIDR.ScheduleRuleCluster_ScheduleRule src WITH (NOLOCK)
		JOIN SEIDR.ScheduleRule r WITH (NOLOCK)
			ON src.ScheduleRuleID = r.ScheduleRuleID
		WHERE ScheduleRuleClusterID = @ScheduleRuleClusterID		
		ORDER BY PartOfDateType, PartOfDate, IntervalType, IntervalValue, Hour, Minute
		
		OPEN Rule_Cursor

		FETCH NEXT FROM RULE_CURSOR INTO @ScheduleRuleID
		DECLARE @FS2 int = @@FETCH_STATUS
		WHILE @FS2 = 0
		BEGIN
			SELECT @Desc += SEIDR.ufn_ScheduleRule_GetDescription(@ScheduleRuleID, @TimeOffset)

			
			FETCH NEXT FROM RULE_CURSOR INTO @ScheduleRuleID
			SELECT @FS2 = @@FETCH_STATUS
			IF @FS2 = 0
				SELECT @Desc += ' AND '
		END
		CLOSE RULE_CURSOR
		DEALLOCATE RULE_CURSOR
		
		FETCH NEXT FROM Cluster_Cursor 
		INTO @ScheduleRuleClusterID
		SELECT @RC = @@FETCH_STATUS
		
		IF @RC = 0
			SELECT @Desc += ' 
OR '
	END
	CLOSE CLUSTER_CURSOR
	DEALLOCATE CLUSTER_CURSOR


	RETURN @Desc
END