CREATE PROCEDURE [SEIDR].[usp_JobProfile_Job_d]
	@JobProfile_JobID int = null,
	@JobProfileID int = null,
	@StepNumber tinyint = null,
	@ShiftAmount smallint = null,
	@ShiftStop tinyint = null,
	@Delete bit = 1,
	@UserName varchar(260) = null,
	@SafetyMode bit = 1
AS
BEGIN
	SET XACT_ABORT ON
	IF @ShiftAmount is null
		SET @ShiftAmount = 0	

	IF @JobProfile_JobID is null
	BEGIN
		IF @JobProfileID is null or @StepNumber is null
		BEGIN
			RAISERROR('Need to provide either @JobProfile_JobID or @JobProfileID and @StepNumber', 16, 1)
			RETURN
		END
		DECLARE @RC int
		SELECT @JobProfile_JobID = JobProfile_JobID
		FROM SEIDR.JobProfile_Job WITH (NOLOCK)
		WHERE JobProfileID = @JobProfileID
		AND StepNumber = @StepNumber
		AND Active = @Delete
		
		SELECT @RC = @@ROWCOUNT
		IF @RC = 0
		BEGIN
			RAISERROR('No active step found for @StepNumber = %d.', 16, 1, @StepNumber);
			RETURN
		END
		IF @RC > 1
		BEGIN
			IF @Delete = 1
			BEGIN
				SELECT * 
				FROM SEIDR.vw_JobProfile_Job
				WHERE JobProfileID = @JobProfileID
				AND StepNumber = @StepNumber
			END
			ELSE
				SELECT * 
				FROM SEIDR.JobProfile_Job jpj
				WHERE JobProfileID = @JobProfileID
				AND StepNumber = @StepNumber
				AND Active = 0
			RAISERROR('Multiple JobProfile_Job records for step. Please specify @JobProfile_JobID.' , 16, 1)
			RETURN
			
		END
	END
	ELSE 
	BEGIN
		IF NOT EXISTS(SELECT null FROM SEIDR.JobProfile_Job WHERE JobProfile_JobID = @JobProfile_JobID AND Active = @Delete)
		BEGIN
			RAISERROR('JobProfile_JobID is not in correct Active state for operation.', 16, 1)
			RETURN
		END
		SELECT @JobProfileID = JobProfileID, @StepNumber = StepNumber
		FROM SEIDR.JobProfile_Job
		WHERE JObProfile_JobID = @JobProfile_JobID
	END

	DECLARE @AutoMessage varchar(2000)
	IF @UserName is null
		SET @UserName = SUSER_NAME()

	IF @Delete = 1
	BEGIN
		--Cleanup and archive JobProfile_Job_Parent
		IF EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent jpjp WHERE jpjp.Parent_JobProfile_JobID = @JobProfile_JobID)
		BEGIN
			IF @SafetyMode = 1
			BEGIN
				RAISERROR('JobProfile_JobID %d is being used as a parent step and cannot be deleted with @SafetyMode = 1', 16, 1)
				RETURN
			END
			DELETE SEIDR.JobProfile_Job_Parent 
			OUTPUT DELETED.JobProfile_Job_ParentID, DELETED.Parent_JobProfile_JobID, DELETED.JobProfile_JobID, DELETED.SequenceDayMatchDifference, @UserName
			INTO SEIDR.JobProfile_Job_Parent_Archive(JobProfile_Job_ParentID, Parent_JobProfile_JobID, JobProfile_JobID, SequenceDayMatchDifference, DeletingUser)
			WHERE Parent_JobProfile_JobID = @JobProfile_JobID
		END 
		/* --Should not hurt to have the inactive JobProfile_JobID still pointing to other steps in the parent table.
		IF EXISTS(SELECT null FROM SEIDR.JobProfile_Job_Parent WHERE JobProfile_JobID = @JobProfile_JobID)
			DELETE SEIDR.JobProfile_Job_Parent 			
			OUTPUT DELETED.JobProfile_Job_ParentID, DELETED.Parent_JobProfile_JobID, DELETED.JobProfile_JobID, DELETED.SequenceDayMatchDifference, @UserName
			INTO SEIDR.JobProfile_Job_Parent_Archive(JobProfile_Job_ParentID, Parent_JobProfile_JobID, JobProfile_JobID, SequenceDayMatchDifference, DeletingUser)
			WHERE JobProfile_JobID = @JobProfile_JobID
		*/
		
		BEGIN TRY
			BEGIN TRAN

			UPDATE SEIDR.JobProfile_Job
			SET DD = GETDATE(), 
				DeletedBy = @UserName
			WHERE JobProfile_JobID = @JobProfile_JobID
		
			IF @ShiftAmount <> 0
			BEGIN
				IF @ShiftStop is null
				BEGIN
					IF @ShiftAmount > 0
						SET @ShiftStop = 1
					ELSE
					BEGIN
						SELECT @ShiftStop = MAX(StepNumber)
						FROM SEIDR.JobProfile_Job
						WHERE JobProfileID = @JobProfileID
						AND Active = 1
						IF @ShiftStop < @StepNumber
							SET @ShiftStop = @StepNumber --NoShift
					END
				END

				IF @ShiftStop < @StepNumber
				BEGIN
					UPDATE SEIDR.JobProfile_Job
					SET StepNumber += @ShiftAmount
					WHERE JobProfileID = @JobProfileID
					AND Active = 1
					AND StepNumber BETWEEN @ShiftStop AND @StepNumber
				END
				ELSE IF @ShiftStop > @StepNumber
				BEGIN
					UPDATE SEIDR.JobProfile_Job
					SET StepNumber += @ShiftAmount
					WHERE JobProfileID = @JobProfileID
					AND Active = 1
					AND StepNumber BETWEEN @StepNumber and @ShiftStop
				END

				--SELECT * 
				--FROM SEIDR.vw_JobProfile_Job
				--WHERE JobProfileID = @JobProfileID

			END
			
			exec SEIDR.usp_JobProfile_Help @JobProfileID = @JobProfileID,  @IncludeInactive = 1

			SET @AutoMessage = 'Deleted Step Number ' + CONVERT(varchar(20), @StepNumber)
			IF @ShiftStop is not null and @ShiftStop != @StepNumber
				SET @AutoMessage += ', Shifted steps until ' + CONVERT(varchar(20), @ShiftStop)

			exec SEIDR.usp_JobProfileNote_i @JobProfileID, @AutoMessage, 1, @UserName

		END TRY
		BEGIN CATCH
			ROLLBACK
			;throw
		END CATCH

	END -- IF @Delete = 1
	ELSE IF @DELETE = 0
	BEGIN
		BEGIN TRY
		BEGIN TRAN
		UPDATE SEIDR.JobProfile_Job
		SET DD = null, 
			DeletedBy = null, 
			LastUpdate = GETDATE(), 
			UpdatedBy = @UserName
		WHERE JobProfile_JobID = @JobProfile_JobID

		SET @AutoMessage = 'Reactivated step Number ' + CONVERT(varchar(20), @StepNumber)
		exec SEIDR.usp_JobProfileNote_i @JobProfileID, @AutoMessage, 1, @UserName
		
		END TRY
		BEGIN CATCH			
			ROLLBACK
			;throw
		END CATCH

	END

	
		IF @@TRANCOUNT > 0
			COMMIT
END