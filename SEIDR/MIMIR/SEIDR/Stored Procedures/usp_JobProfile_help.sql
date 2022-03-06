CREATE PROCEDURE [SEIDR].[usp_JobProfile_help]
	@JobProfileID int,
	@JobProfile_JobID int = null,
	@IncludeInactive bit = 0 --ToDo: Parameters for @IncludeConfigurations (existing cursor logic/jobprofile_Job logic), @IncludeDocumentation (JobProfileNote), @IncludeContactDocumentation (Contact links + Notes on those contacts)
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	IF @JobProfile_JobID is null
		SELECT * 
		FROM SEIDR.vw_JobProfile 
		WHERE jobProfileID = @JobProfileID

	IF @IncludeInactive = 0
	BEGIN
		SELECT JobProfile_JobID, jpj.Branch, StepNumber, jpj.Description [StepDescriptions], 
			j.JobID, j.JobName,  ConfigurationTable, jpj.TriggerBranch, jpj.TriggerExecutionStatusCode, jpj.TriggerExecutionNameSpace, 
			CanRetry, RetryLimit, ISNULL(RetryDelay, 10) [RetryDelay],
			RequiredThreadID, FailureNotificationMail, jpj.RetryCountBeforeFailureNotification,
			s.Description [SequenceSchedule],
			[ParentStepCount] = (SELECT COUNT(*) FROM SEIDR.JobProfile_Job_Parent WHERE JobProfile_JobID = jpj.JobProfile_JobID)
		FROM SEIDR.JobProfile_Job jpj
		JOIN SEIDR.Job j
			ON jpj.JobID = j.JobID
		LEFT JOIN SEIDR.Schedule s
			on jpj.SequenceScheduleID = s.ScheduleID
		WHERE jpj.Active = 1
		AND Jpj.JobProfileID = @JobProfileID
		AND (@JobProfile_JobID is null or JobProfile_JobID = @JobProfile_JobID)
		ORDER BY jpj.Branch, StepNumber, jpj.TriggerBranch, jpj.TriggerExecutionNameSpace, jpj.TriggerExecutionStatusCode
	END
	ELSE
	BEGIN		
		SELECT JobProfile_JobID, jpj.Branch, StepNumber, jpj.Active, jpj.Description [StepDescriptions], 
			j.JobID, j.JobName,  ConfigurationTable, jpj.TriggerBranch, jpj.TriggerExecutionStatusCode, jpj.TriggerExecutionNameSpace, 
			CanRetry, RetryLimit, ISNULL(RetryDelay, 10) [RetryDelay],
			RequiredThreadID, FailureNotificationMail, jpj.RetryCountBeforeFailureNotification, 
			jpj.DD, 
			s.Description [SequenceSchedule],
			[ParentStepCount] = (SELECT COUNT(*) FROM SEIDR.JobProfile_Job_Parent WHERE JobProfile_JobID = jpj.JobProfile_JobID)
		FROM SEIDR.JobProfile_Job jpj
		JOIN SEIDR.Job j
			ON jpj.JobID = j.JobID
		LEFT JOIN SEIDR.Schedule s
			on jpj.SequenceScheduleID = s.ScheduleID
		WHERE Jpj.JobProfileID = @JobProfileID
		AND (@JobProfile_JobID is null or JobProfile_JobID = @JobProfile_JobID)
		ORDER BY jpj.Branch, StepNumber, jpj.TriggerBranch, jpj.TriggerExecutionNameSpace, jpj.TriggerExecutionStatusCode
	END

	IF EXISTS(SELECT null FROM SEIDR.vw_JobProfile_Parent WHERE JobProfileID = @JobProfileID AND (@JobProfile_JobID IS NULL or JobProfile_JobID = @JobProfile_JobID))
	BEGIN
		SELECT JobProfile_Job_ParentID, StepNumber, Step, 
			ParentJobProfileID, pjp.Description [ParentJobProfile], pjp.Userkey1 + ISNULL('|' + pjp.UserKey2, '') [ParentUserKey],
			ParentStepNumber, ParentStep, 
			SequenceDayMatchDifference
		FROM SEIDR.vw_JobProfile_Parent p
		JOIN SEIDR.JobProfile pjp
			ON p.ParentJobProfileID = pjp.JobProfileID
		WHERE p.JobProfileID = @JobProfileID 
		AND (@JobProfile_JobID IS NULL or JobProfile_JobID = @JobProfile_JobID)
	END


	DECLARE step_Cursor CURSOR LOCAL FAST_FORWARD
	FOR SELECT distinct ConfigurationTable, j.JobID
	FROM SEIDR.JobProfile_Job jpj
	JOIN SEIDR.Job j
		ON jpj.JobID = j.JobID
	WHERE (jpj.Active = 1 OR @IncludeInactive = 1)
	AND Jpj.JobProfileID = @JobProfileID
	AND (@JobProfile_JobID is null or JobProfile_JobID = @JobProfile_JobID)
	--ORDER BY StepNumber ASC, TriggerExecutionNameSpace ASC, TriggerExecutionStatusCode ASC
	

	DECLARE @stepID int
	DECLARE @Config varchar(256), @JobID int

	OPEN step_Cursor

	FETCH NEXT FROM step_Cursor
	INTO @Config, @JobID

	WHILE @@FETCH_STATUS = 0
	BEGIN		
		DECLARE @Active nvarchar(100) = ''
		IF @IncludeInactive = 0
		BEGIN
			IF COL_LENGTH(@Config, 'Active') IS NOT NULL
				SET @Active = 'AND x.Active = 1'
			ELSE IF COL_LENGTH(@Config, 'IsValid') IS NOT NULL
				SET @Active = 'AND x.IsValid = 1'
			SET @Active += ' AND jpj.Active = 1'
		END
		DECLARE @ColList varchar(2000) = ''
		
		SELECT @ColList += ', x.[' + name + ']'
		FROM sys.columns 
		WHERE OBJECT_ID = OBJECT_ID(@Config)
		AND Name NOT IN ('DC', 'LU', 'JobProfile_JobID', 'RV', 'CB', 'DD', 'Active', 'IsValid')

		DECLARE @JPJ nvarchar(30) = N''
		IF EXISTS(SELECT null
					FROM SEIDR.JobProfile_Job
					WHERE JobProfileID = @JobProfileID 
					AND JobID = @JobID
					AND (Active = 1 OR @IncludeInactive = 1)
					GROUP BY StepNumber
					HAVING COUNT(*) > 1)
		BEGIN
			SET @JPJ = N'jpj.JobProfile_JobID, '
		END

		DECLARE @SQL nvarchar(4000) = N'SELECT ' + @JPJ + N'jpj.StepNumber, jpj.Description [Step Description],' + CASE WHEN @IncludeInactive = 1 then N' jpj.Active,' else N'' end + N' jpj.JobID' + @ColList + N'
		,jpj.CanRetry, jpj.RetryLimit, ISNULL(jpj.RetryDelay, 10) [RetryDelay], jpj.RequiredThreadID, jpj.FailureNotificationMail
		FROM SEIDR.JobProfile_Job jpj WITH (NOLOCK)
		JOIN ' + @Config + ' x WITH (NOLOCK)
			ON jpj.JobProfile_JobID = x.JobProfile_JobID ' + @Active + N'
		WHERE jpj.JobProfileID = ' + CONVERT(varchar(20), @JobProfileID) 
		IF @JobProfile_JobID IS NOT NULL
			SET @SQL += N'	AND jpj.JobProfile_JobID = ' + CONVERT(varchar(20), @JobProfile_JobID)
		
		SET @SQL += N'
		ORDER BY jpj.StepNumber ASC, jpj.JobProfile_JobID ASC'

		exec(@SQL)

			
		FETCH NEXT FROM step_Cursor
		INTO @Config, @JobID
	END
	
	CLOSE step_Cursor
	DEALLOCATE step_cursor

RETURN 0
