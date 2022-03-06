CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_SetSequenceSchedule]
	@JobProfile_JobID int = null,
	@JobProfileID int = null,
	@StepNumber smallint = null,	
	@TriggerExecutionStatus varchar(50) = null,
	@TriggerExecutionNameSpace varchar(128) = null,
	@SequenceSchedule varchar(300) = null,
	@SequenceScheduleID int = null,
	@Offset int = 0,
	@UserName varchar(260) = null
AS
	IF @JobProfile_JobID is null
	BEGIN
		SELECT @JobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @JobProfileID 
		AND StepNumber = @StepNumber
		AND (@TriggerExecutionStatus is null AND TriggerExecutionStatusCode is null OR TriggerExecutionStatusCode = @TriggerExecutionStatus)
		AND (@TriggerExecutionNameSpace is null AND TriggerExecutionNameSpace is null OR TriggerExecutionNameSpace = @TriggerExecutionNameSpace)		
		AND Active = 1
	END
	ELSE IF @JobProfileID is not null
	BEGIN
		IF EXISTS(SELECT null 
					FROM SEIDR.JobProfile_Job 
					WHERE JobProfile_JobID = @JobProfile_JobID 
					AND (
						jobProfileID <> @JobProfileID
						OR @StepNumber is not null and StepNumber <> @StepNumber
						OR Active = 0
						OR @TriggerExecutionStatus is not null AND ISNULL(TriggerExecutionStatusCode, '') <> @TriggerExecutionStatus
						OR @TriggerExecutionNameSpace is not null and ISNULL(TriggerExecutionNameSpace, '') <> @TriggerExecutionNameSpace
						)
				)
		BEGIN
			RAISERROR('@JobProfileID/@Stepnumber/Trigger Status do not match', 16, 1)
			RETURN
		END

		SELECT @StepNumber = StepNumber
		FROM SEIDR.JobProfile_Job
		WHERE JobProfile_JobID = @JobProfile_JobID
	END	
	
	
	IF @JobProfile_JobID is null
	BEGIN
		RAISERROR('Unable to identify @JobProfile_JobID', 16, 1)
		RETURN
	END
	IF @SequenceScheduleID IS NOT NULL AND NOT EXISTS(SELECT null FROM SEIDR.Schedule WHERE ScheduleID = @SequenceScheduleID AND Active = 1 AND ForSequenceControl = 1)
	BEGIN
		RAISERROR('@SequenceScheduleID %d is not marked for sequence control usage.', 16, 1, @SequenceScheduleID)
		SET @SequenceScheduleID = null
	END
	--DECLARE @SequenceScheduleID int = null
	IF @SequenceScheduleID IS NULL 
	AND @SequenceSchedule IS NULL
	BEGIN
		RAISERROR('Must provide @SequenceSchedule or @SequenceScheduleID', 16, 1)
		RETURN
	END

	IF @SequenceScheduleID IS NULL
	BEGIN
		SELECT @SequenceScheduleID = ScheduleID
		FROM SEIDR.Schedule
		WHERE Description = @SequenceSchedule
		AND Active = 1 
		AND ForSequenceControl = 1
		IF @@ROWCOUNT = 0
		BEGIN
			SELECT *, SEIDR.ufn_Schedule_GetDescription(ScheduleID, @Offset) [ComputedDescription], @Offset [ScheduleComputedDescription_HourOffset] 
			FROM SEIDR.Schedule 
			WHERE ForSequenceControl = 1

			RAISERROR('Invalid Schedule for Sequence Control: %s', 16, 1, @SequenceSchedule)
			RETURN 80
		END
		RAISERROR('Schedule identified from description "%s": %d', 0, 0, @SequenceSchedule, @SequenceScheduleID)
	END 
	ELSE
		SELECT @SequenceSchedule = Description
		FROM SEIDR.Schedule WITH (NOLOCK)
		WHERE ScheduleID = @SequenceScheduleID

	SELECT SEIDR.ufn_Schedule_GetDescription(@SequenceScheduleID, @Offset) [ComputedDescription], @Offset [ScheduleComputedDescription_HourOffset]

	IF @JobProfileID IS NULL
		SELECT @JobProfileID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfile_jobID = @JobProfile_JobID
	
	UPDATE SEIDR.JobProfile_Job
	SET SequenceScheduleID = @SequenceScheduleID
	WHERE JobProfile_JobID = @JobProfile_JobID

	DECLARE @AutoMessage varchar(2000) = 'Set Sequence for Step Number ' + CONVERT(varchar(20), @StepNumber) + ' to "' + @SequenceSchedule + '"'
	exec SEIDR.usp_JobProfileNote_i @JobProfileID, @AutoMessage, 1, @UserName
RETURN 0
