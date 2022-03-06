
CREATE PROCEDURE [SEIDR].[usp_JobProfile_SetParentStep]
	@JobProfileID int = null,
	@StepNumber smallint = null,
	@JobProfile_JobID int = null,
	@ParentJobProfileID int = null,
	@ParentStepNumber smallint = null,
	@ParentJobProfile_JobID int = null,
	--@ScheduleID int
	@SequenceDayMatchDifference int
AS
BEGIN
	IF @JobProfile_JobID IS NULL
	BEGIN
		IF @JobProfileID IS NULL OR @StepNumber IS NULL
		BEGIN
			RAISERROR('Must provide both @JobProfileID and @StepNumber if not providing @JobProfile_JobID', 16, 1)
			RETURN
		END
		SELECT @JobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @JobProfileID
		AND StepNumber = @StepNumber 
		AND Active = 1
		IF @@ROWCOUNT <> 1
		BEGIN
			RAISERROR('Could not uniquely identify step.', 16, 1)
			RETURN
		END
	END

	IF @ParentJobProfile_JobID IS NULL
	BEGIN
		IF @ParentJobProfileID IS NULL OR @ParentStepNumber IS NULL
		BEGIN
			RAISERROR('Must provide both @ParentJobProfileID and @ParentStepNumber if not providing @ParentJobProfile_JobID', 16, 1)
			RETURN
		END
		SELECT @ParentJobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job
		WHERE JobProfileID = @ParentJobProfileID
		AND StepNumber = @ParentStepNumber 
		AND Active = 1
		IF @@ROWCOUNT <> 1
		BEGIN
			RAISERROR('Could not uniquely identify parent step.', 16, 1)
			RETURN
		END
	END
	IF @SequenceDayMatchDifference is null
	BEGIN
		RAISERROR('REMOVING ANY PARENT RELATION.', 1, 1)
		DELETE SEIDR.JobProfile_Job_Parent
		WHERE JobProfile_JobID = @JobProfile_JobID
		AND Parent_JobProfile_JobID = @ParentJobProfile_JobID
		
		RETURN 0
	END/*
	ELSE IF NOT EXISTS(SELECT null FROM SEIDR.Schedule WHERE Active = 1 AND ScheduleID = @ScheduleID AND ForSequenceControl = 1)
	BEGIN
		RAISERROR('Provided ScheduleID not linked to an active schedule for Sequence control usage.' ,16 , 1)
		RETURN
	END*/
	IF EXISTS(SELECT null 
				FROM SEIDR.JobProfile_Job j1
				JOIN SEIDR.JobProfile_Job j2
					ON j1.JobProfileID = j2.JobProfileID
				WHERE j1.JobProfile_JobID = @JobProfile_JobID
				AND j2.JobProfile_JobID = @ParentJObProfile_JobID)
	BEGIN
		RAISERROR('Cannot make a step for the same JobProfileID a sequence schedule parent.', 16, 1)
		RETURN
	END
	IF EXISTS(SELECT null 
				FROM SEIDR.JobProfile_Job_Parent
				WHERE JobProfile_JobID = @JobProfile_JobID
				AND Parent_JobProfile_JobID = @ParentJobprofile_JobID)
	BEGIN
		RAISERROR('Parent relation is already set.', 1, 1)
		RETURN
	END
	IF EXISTS(SELECT null
				FROM SEIDR.JobProfile_Job_Parent
				WHERE Parent_JobProfile_JobID = @JobProfile_JobID
				AND JobProfile_JobID = @ParentJobProfile_JobID)
	BEGIN
		RAISERROR('Opposite relation is currently configured. Cannot configure.', 16, 1)
		RETURN
	END

	INSERT INTO SEIDR.JobProfile_Job_Parent(JobProfile_JobID, Parent_JobProfile_JobID, SequenceDayMatchDifference)
	VALUES(@JobProfile_JobID, @ParentJobProfile_JobID, @SequenceDayMatchDifference)

END