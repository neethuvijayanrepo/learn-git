
CREATE PROCEDURE [SEIDR].[usp_FileSearch]
	@FilePath varchar(500),
	@PartialPath bit = 1,
	--@IncludeLog bit = 0,
	@OrganizationID int = null,
	@ProjectID smallint = null,
	@UserKey1 varchar(50) = null
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	IF @PartialPath = 1
		SET @FilePath = '%' + @FilePath + '%'

	SELECT JobProfileID, OrganizationID, ProjectID, UserKey1, UserKey2, JobExecutionID, ProcessingDate, StepNumber, FilePath, ExecutionStatus, JobProfile_JobID, 
		LU [TimeStamp], 'Current' as [State], je.FileHash
	FROM SEIDR.jobExecution je	
	WHERE Active = 1 	
	AND (
		FilePath LIKE @FilePath	
		--OR  @IncludeLog = 1
		--	AND EXISTS(SELECT null FROM SEIDR.Log WHERE JobExecutionID = je.JobExecutionID AND JobProfile_JobID = je.JobProfile_JobID AND LogMessage LIKE @FilePath)
		)
	AND (@OrganizationID is null OR OrganizationID = @OrganizationID)
	AND (@ProjectID is null or ProjectID = @ProjectID)
	AND (@UserKey1 is null or UserKey1 = @UserKey1)	
	UNION ALL
	SELECT je.JobProfileID, OrganizationID, ProjectID, UserKey1, UserKey2, h.JobExecutionID, h.ProcessingDate, h.StepNumber, h.FilePath, h.ExecutionStatus, h.JobProfile_JobID, 
		h.DC, 'History', h.FileHash
	FROM SEIDR.vw_jobExecutionHistory h
	JOIN SEIDR.JobExecution je
		ON h.JobExecutionID = je.JobExecutionID
		AND (h.ExecutionStatusCode <> je.ExecutionStatusCode or h.ExecutionStatusNameSpace <> je.ExecutionStatusNameSpace) --Completion is inserted to history. Show as current.
	WHERE (
			h.FilePath LIKE @FilePath 			
			--OR @IncludeLog = 1
			--AND EXISTS(SELECT null FROM SEIDR.Log WHERE JobExecutionID = h.JobExecutionID AND JobProfile_JobID = h.JobProfile_JobID AND LogMessage LIKE @FilePath)
			)
	AND (@OrganizationID is null OR OrganizationID = @OrganizationID)
	AND (@ProjectID is null or ProjectID = @ProjectID)
	AND (@UserKey1 is null or UserKey1 = @UserKey1)
	ORDER BY ProcessingDate desc, JobExecutionID, StepNumber, [TimeStamp]

	IF @FilePath LIKE '%<[-+0123456789]%[YMD]>%'
	BEGIN
		SELECT f.JobProfileID, f.JobProfile, jp.OrganizationID, jp.ProjectID, 
			jp.UserKey1, jp.UserKey2, 
			f.JobProfile_JobID, 
			f.StepNumber, 
			f.StepDescription, 
			f.FilePath, f.[Source]
		FROM SEIDR.vw_FilePath f 
		JOIN SEIDR.JobProfile jp
			ON f.JobProfileID = jp.JobProfileID
		WHERE FilePath LIKE @FilePath
		AND (@UserKey1 is null or jp.UserKey1 = @UserKey1)
		AND (@ProjectID Is null or jp.ProjectID = @ProjectID)
		AND (@OrganizationID is null or jp.OrganizationID = @OrganizationID)
		ORDER BY f.JobProfileID, f.StepNumber
	END
	ELSE
	BEGIN
		SELECT * 
		FROM SEIDR.vw_JobExecution_FilePath
		WHERE UnmaskedFilePath LIKE @FilePath
		AND (@UserKey1 is null or UserKey1 = @UserKey1)
		AND (@ProjectID Is null or ProjectID = @ProjectID)
		AND (@OrganizationID is null or OrganizationID = @OrganizationID)
		ORDER BY JobExecutionID, StepNumber, [ConfigurationSource]
	END
END