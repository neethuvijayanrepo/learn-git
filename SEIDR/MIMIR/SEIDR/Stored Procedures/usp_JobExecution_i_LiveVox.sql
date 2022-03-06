CREATE PROCEDURE [SEIDR].[usp_JobExecution_i_LiveVox]
	@CampaignID	int,
	@JobProfileID int,
	@CampaignName varchar(250),
	@FilePath varchar(250) = NULL output ,
	@JobExecutionID bigint = NULL output 

AS
	SELECT @FilePath=c.ArchiveLocation
	FROM SEIDR.JobProfile_Job jpj 
	JOIN METRIX.ExportSettings c 
		ON jpj.JobProfile_JobID = c.JobProfile_JobID 
	WHERE jpj.JobProfileID = @JobProfileID 
	AND jpj.StepNumber = 1
	AND ArchiveLocation is not null

	IF @@ROWCOUNT = 0
	BEGIN
		RAISERROR('ArchiveLocation not specified.', 16, 1)
		RETURN
	END

	
	SET @FilePath = CASE WHEN RIGHT(@FilePath, 1) IN ('\') THEN @FilePath
					ELSE @FilePath+'\' END


	INSERT INTO SEIDR.JobExecution(JobProfileID, UserKey, UserKey1, UserKey2,
				StepNumber, ExecutionStatusCode,
				OrganizationID,ProjectID,LoadProfileID)
	SELECT @JobProfileID, UserKey, UserKey1, UserKey2, 
				1, 'PD',
				OrganizationID,ProjectID,LoadProfileID
	FROM SEIDR.JobProfile
	WHERE JobProfileID = @JobProfileID
	AND Active = 1

	IF @@ERROR <> 0
	BEGIN
		SET @FilePath = null
		SELECT null, null
		RETURN;
	END

	SET @JobExecutionID = SCOPE_IDENTITY()

	SET @FilePath = @FilePath+ 'C' + Convert(varchar,@CampaignID) + @CampaignName + 'JE' + Convert(varchar,@JobExecutionID) + '.csv'
	
	UPDATE SEIDR.JobExecution WITH (ROWLOCK)
	SET FilePath = @FilePath
	WHERE JobExecutionID = @JobExecutionID


	SELECT @JobExecutionID,@FilePath
