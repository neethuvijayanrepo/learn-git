CREATE TABLE [SEIDR].[JobProfile] (
    [JobProfileID]                  INT           IDENTITY (1, 1) NOT NULL,
    [Description]                   VARCHAR (256) NOT NULL,
    [RegistrationFolder]            VARCHAR (250) NULL,
    [RegistrationDestinationFolder] VARCHAR (250) NULL,
    [FileDateMask]                  VARCHAR (128) NULL,
    [FileFilter]                    VARCHAR (600) NULL,
    [RequiredThreadID]              TINYINT       NULL,
    [ScheduleID]                    INT           NULL,
    [UserKey]                       INT           NULL,
    [UserKey1]                      VARCHAR (50)  NOT NULL,
    [UserKey2]                      VARCHAR (50)  NULL,
    [Creator]                       VARCHAR (128) DEFAULT (suser_name()) NOT NULL,
    [DC]                            DATETIME      DEFAULT (getdate()) NOT NULL,
    [DD]                            DATETIME      NULL,
    [Active]                        AS            (CONVERT([bit],case when [DD] IS NULL then (1) else (0) end)),
    [SuccessNotificationMail]       VARCHAR (500) NULL,
    [JobPriority]                   VARCHAR (10)  DEFAULT ('NORMAL') NOT NULL,
    [ScheduleFromDate]              DATETIME      NULL,
    [ScheduleThroughDate]           DATETIME      NULL,
    [ScheduleValid]                 AS            (CONVERT([bit],case when [ScheduleID] IS NULL then (0) when [ScheduleFromDate] IS NULL OR [ScheduleFromDate]>getdate() then (0) when [ScheduleThroughDate] IS NULL then (1) when [ScheduleThroughDate]<=[ScheduleFromDate] then (0) else (1) end)),
    [ScheduleNoHistory]             BIT           DEFAULT ((0)) NOT NULL,
    [RegistrationValid]             AS            (CONVERT([bit],case when nullif(ltrim(rtrim([RegistrationFolder])),'') IS NULL then (0) when nullif(ltrim(rtrim([FileFilter])),'') IS NULL then (0) when nullif(ltrim(rtrim([FileDateMask])),'') IS NULL then (0) when [FileFilter]='INVALID' OR [FileFilter] like '%INACTIVE' then (0) when [RegistrationDestinationFolder] IS NOT NULL AND NOT [RegistrationDestinationFolder] like '\\%' AND NOT [RegistrationDestinationFolder] like '[a-zA-Z]:\%' then (0) else (1) end)) PERSISTED NOT NULL,
    [OrganizationID]                INT           NOT NULL,
    [ProjectID]                     SMALLINT      NULL,
    [LoadProfileID]                 INT           NULL,
    [DeliveryScheduleID]            INT           NULL,
    [Track]                         BIT           DEFAULT ((1)) NOT NULL,
    [Editor]                        VARCHAR (128) CONSTRAINT [df_JobProfile_Editor] DEFAULT (suser_name()) NOT NULL,
    [StopAfterStepNumber]           TINYINT       NULL,
    [FileExclusionFilter]           VARCHAR (600) NULL,
    PRIMARY KEY CLUSTERED ([JobProfileID] ASC),
    CONSTRAINT [CK_JobProfile_UserKey_Difference] CHECK ([UserKey2] IS NULL OR [UserKey1]<>[UserKey2]),
    CONSTRAINT [CK_Project_Organization] CHECK ([REFERENCE].[ufn_Check_Project_Organization]([ProjectID],[OrganizationID])=(1)),
    FOREIGN KEY ([DeliveryScheduleID]) REFERENCES [SEIDR].[Schedule] ([ScheduleID]),
    FOREIGN KEY ([JobPriority]) REFERENCES [SEIDR].[Priority] ([PriorityCode]),
    FOREIGN KEY ([ScheduleID]) REFERENCES [SEIDR].[Schedule] ([ScheduleID]),
    CONSTRAINT [FK_JobProfile_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [REFERENCE].[Organization] ([OrganizationID]) ON UPDATE CASCADE,
    CONSTRAINT [FK_JobProfile_Project] FOREIGN KEY ([ProjectID]) REFERENCES [REFERENCE].[Project] ([ProjectID]),
    CONSTRAINT [FK_JobProfile_UserKey] FOREIGN KEY ([UserKey1]) REFERENCES [REFERENCE].[UserKey] ([UserKey])
);

























GO
CREATE UNIQUE INDEX UQ_REGISTRATION_FOLDER_FILTER 
ON SEIDR.JobProfile(RegistrationFolder, FileFilter)
WHERE DD IS NULL AND RegistrationFolder is not null and FileFilter is not null
GO
CREATE STATISTICS [st_JobProfile_JobPriority]
    ON [SEIDR].[JobProfile]([JobPriority]);


GO

CREATE TRIGGER [SEIDR].[trg_JobProfile_u]
ON [SEIDR].[JobProfile]
AFTER UPDATE
AS
BEGIN
	
	--ETL Team permission for direct updating removed. Procedure usage should be okay
	IF @@NESTLEVEL = 0 --Okay to do from a procedure.  
	--CAST(CONTEXT_INFO() as varchar(30)) <> 'SERVICE'
	BEGIN
		IF UPDATE(RegistrationFolder)
		OR UPDATE(RegistrationDestinationFolder) 
		OR UPDATE(ScheduleID)
		OR UPDATE(ScheduleFromDate)
		OR UPDATE(ScheduleThroughDate)
		OR UPDATE(FileFilter)
		BEGIN
			IF 1 < (SELECT COUNT(distinct JobProfileID) FROM INSERTED)
			BEGIN
				RAISERROR('Cannot Update Multiple Profiles at the same time.', 16, 1)
				ROLLBACK
				RETURN
			END			
		END
	END

	IF TRIGGER_NESTLEVEL(@@PROCID) > 1
		RETURN
	SET NOCOUNT ON
	SET XACT_ABORT ON

	DECLARE @updateTime datetime = GETDATE()
	

	CREATE TABLE #Profile_Age(JobProfileiD int primary key, LU datetime, ChangeSummary varchar(500))
	INSERT INTO #Profile_Age(JobProfileID, LU, ChangeSummary)
	SELECT i.JobProfileID, i.DC, 
		''+ CASE WHEN  ISNULL(i.OrganizationID, 0) <> ISNULL(d.OrganizationID, 0) 
				then 'Changed OrganizationID to ' + ISNULL(CONVERT(varchar, i.OrganizationID), '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.ProjectID, 0) <> ISNULL(d.ProjectID, 0) 
				then 'Changed ProjectID to ' + ISNULL(CONVERT(varchar, i.ProjectID), '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.UserKey1, '') <> ISNULL(d.UserKey1, '') 
				then 'Changed UserKey1 to ' + ISNULL(i.UserKey1, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.UserKey2, '') <> ISNULL(d.UserKey2, '') 
				then 'Changed UserKey2 to ' + ISNULL(i.UserKey2, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.RegistrationFolder, '') <> ISNULL(d.RegistrationFolder, '') 
				then 'Changed RegistrationFolder to ' + ISNULL(i.RegistrationFolder, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.FileFilter, '') <> ISNULL(d.FileFilter, '') 
				then 'Changed FileFilter to ' + ISNULL(i.FileFilter, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.FileDateMask, '') <> ISNULL(d.FileDateMask, '') 
				then 'Changed FileDateMask to ' + ISNULL(i.FileDateMask, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.RegistrationDestinationFolder, '') <> ISNULL(d.RegistrationDestinationFolder, '') 
				then 'Changed RegistrationDestinationFolder to ' + ISNULL(i.RegistrationDestinationFolder, '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.ScheduleID, 0) <> ISNULL(d.ScheduleID, 0) 
				then 'Changed ScheduleID to ' + ISNULL(CONVERT(varchar, i.ScheduleID), '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.ScheduleFromDate, 0) <> ISNULL(d.ScheduleFromDate, 0) 
				then 'Changed ScheduleFromDate to ' + ISNULL(CONVERT(varchar, i.ScheduleFromDate), '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN  ISNULL(i.ScheduleThroughDate, 0) <> ISNULL(d.ScheduleThroughDate, 0) 
				then 'Changed ScheduleThroughDate to ' + ISNULL(CONVERT(varchar, i.ScheduleThroughDate), '(NULL)') + CHAR(13) + CHAR(10)  
				else '' end
		+ CASE WHEN ISNULL(i.FileExclusionFilter, '') <> ISNULL(d.FileExclusionFilter, '')
				then 'Changed FileExclusionFilter to ' + ISNULL(i.FileExclusionFilter, '(NULL)') + CHAR(13) + CHAR(10)
				else '' end
		+ CASE WHEN  i.Active = 0 and d.Active = 1 then 'Deactivated'
				WHEN i.Active = 1 and d.Active = 0 then 'Reactivated'
				else '' end
	FROM INSERTED i
	JOIN DELETED d
		ON i.JObProfileID = d.JobProfileID
	WHERE ISNULL(i.OrganizationID, 0) <> ISNULL(d.OrganizationID, 0)
	OR ISNULL(i.ProjectID, 0) <> ISNULL(d.ProjectID, 0)
	OR ISNULL(i.UserKey1, '') <> ISNULL(d.UserKey1, '')
	OR ISNULL(i.RegistrationFolder, '') <> ISNULL(d.RegistrationFolder, '')
	OR ISNULL(i.FileFilter, '') <> ISNULL(d.FileFilter, '')
	OR ISNULL(i.RegistrationDestinationFolder, '') <> ISNULL(d.RegistrationDestinationFolder, '')
	OR ISNULL(i.ScheduleID, 0) <> ISNULL(d.ScheduleID, 0)
	OR ISNULL(i.ScheduleFromDate, 0) <> ISNULL(d.ScheduleFromDate, 0)
	OR ISNULL(i.ScheduleThroughDate, 0) <> ISNULL(d.ScheduleThroughDate, 0)	
	OR ISNULL(i.FileExclusionFilter, '') <> ISNULL(d.FileExclusionFilter, '')
	OR i.Active <> d.Active
	IF @@ROWCOUNT > 0
	BEGIN
		UPDATE t
		SET LU = COALESCE(maxDC, t.LU)
		FROM #Profile_age t
		CROSS APPLY(SELECT MAX(DC)maxDC
					FROM SEIDR.JobProfileHistory
					WHERE JobProfileID = t.JobProfileID)m

		INSERT INTO SEIDR.JobProfileHistory(JobProfileID, OrganizationID, ProjectID, LoadProfileID,
		UserKey1, userKey2, RegistrationFolder, FileFilter, FileDateMask, RegistrationDestinationFolder,
		ScheduleID, ScheduleFromDate, ScheduleThroughDate, Editor, AgeMinutes, Active, ChangeSummary, FileExclusionFilter)
		SELECT d.JobProfileID, OrganizationID, ProjectID, LoadProfileID,
		UserKey1, UserKey2, RegistrationFolder, FileFilter, FileDateMask, RegistrationDestinationFolder,
		ScheduleID, ScheduleFromDate, ScheduleThroughDate, d.Editor, DATEDIFF(minute, t.LU, @updateTime), d.Active, t.ChangeSummary, FileExclusionFilter
		FROM DELETED d
		JOIN #Profile_age t
			ON d.JobProfileID = t.JobProfileID
	END
	
	DROP TABLE #Profile_Age

	UPDATE jp
	SET Editor = SUSER_NAME(), 
		Creator = d.Creator --Prevent change.
	FROM SEIDR.JobProfile jp
	JOIN DELETED d
		ON jp.JobProfileID = d.JobProfileID
END