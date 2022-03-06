CREATE PROCEDURE [SEIDR].[usp_JobExecution_Schedule_OnDemand] 
	@JobProfileID INT,
	@ProcessingDate DATETIME = NULL,
	@ExecutionStatusCode VARCHAR(2) =NULL,
	@FilePath varchar(500) = null,
	@StartingStep tinyint = 1,
	@ForceCreate bit = 0,
	@StopAfterStepNumber TINYINT = NULL,
	@Branch varchar(30) = null
AS
BEGIN
	SET XACT_ABORT ON
	DECLARE @ReturnCode INT = 1
	DECLARE @NameSpace VARCHAR(128) = 'SEIDR'
	IF @Branch is null
		SET @Branch = 'MAIN'
	IF @FilePath LIKE '%#%'
	BEGIN
		RAISERROR('Expanding ShortHandPath of @FilePath (%s)', 0, 0, @FilePath)
		SET @FilePath = CONFIG.ufn_ShortHandPath_Profile(@FilePath, @JobProfileID)
		RAISERROR('Expanded ShortHandPath of @FilePath: "%s"', 0, 0, @FilePath)
	END
	BEGIN TRY
	BEGIN TRANSACTION

		--Checking JobProfileID
		IF @JobProfileID IS NULL
		BEGIN
			RAISERROR('JobProfileID is not provided.', 16, 1)  
			GOTO PROC_EXIT 
		END

		--Checking for Processing Date
		IF @ProcessingDate IS NULL
		BEGIN
			SET @ProcessingDate = CONVERT(date, GETDATE())
		END

		--Checking the ExecutionStatusCode
		IF  @ExecutionStatusCode IS NULL OR @filePath is not null
		BEGIN
			SET @ExecutionStatusCode = 'M'
		END
		ELSE IF @ExecutionStatusCode NOT IN ('M','S')
		BEGIN
			RAISERROR('ExecutionStatusCode is not valid for this procedure.', 16, 1)  			
		END

		IF EXISTS (SELECT NULL FROM [SEIDR].[JobProfile_Job] WHERE JobProfileID=@JobProfileID)
		BEGIN
			DECLARE @RC int, @JobExecutionID bigint
			INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2, StepNumber, ExecutionStatusCode, ProcessingDate, OrganizationID,ProjectID,LoadProfileID, FilePath, StopAfterStepNumber, Branch)  
				SELECT JobProfileID, UserKey, UserKey1, UserKey2, @StartingStep, @ExecutionStatusCode , @ProcessingDate ,OrganizationID,ProjectID,LoadProfileID, @FilePath, @StopAfterStepNumber, @Branch
				FROM SEIDR.JobProfile  
				WHERE JobProfileID = @JobProfileID
				AND (
					NOT EXISTS(SELECT null
						FROM SEIDR.JobExecution
						WHERE JobProfileID = @JobProfileID
						AND ProcessingDate = @ProcessingDate
						AND Active = 1)  
					OR @ForceCreate = 1
					)
			SELECT @JobExecutionID = SCOPE_IDENTITY(), @RC = @@ROWCOUNT
			IF @RC = 0
			BEGIN
				RAISERROR('Unable to create JobExecution because there is already an active execution exists in this date. Set @ForceCreate = 1 to create one anyway.', 16, 1)  				
			END

			exec SEIDR.usp_JobExecution_Note_i	@JobExecutionID = @JobExecutionID, @NoteText = 'Requested Manual Job Execution', @Technical = 0
		
			SELECT * 
			FROM SEIDR.JobExecution 
			WHERE JobProfileID = @JobProfileID
			AND ProcessingDate = @ProcessingDate
			AND Active = 1
		END
		ELSE
		BEGIN
			RAISERROR('There is nothing for the JobProfile to do.', 16, 1)  		
		END

		COMMIT
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
		;throw
	END CATCH
	
	SET @ReturnCode=0

	PROC_EXIT:
	/*
	--Move to end of try/catch
	IF @@TRANCOUNT>0
	BEGIN
		IF @ReturnCode = 0
			COMMIT TRANSACTION
		ELSE
			ROLLBACK
	END*/
END