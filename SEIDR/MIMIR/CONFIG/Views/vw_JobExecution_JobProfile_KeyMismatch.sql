CREATE VIEW CONFIG.vw_JobExecution_JobProfile_KeyMismatch
AS
SELECT je.JobExecutionID, je.ProcessingDate, 
		je.OrganizationID,
		o.Description Organization, 
		jp.OrganizationID ProfileOrganizationID,
		po.Description ProfileOrganization,
		CASE WHEN je.OrganizationID <> jp.OrganizationID then cast(1 as bit) else CAST(0 as bit) end [OrganizationMismatch],

		je.ProjectID, p.Description Project, 
		p.CRCM, p.Modular, p.Active ProjectActive, 
		 
		jp.ProjectID ProfileProjectID, 
		pp.Description ProfileProject, pp.CRCM ProfileCRCM, pp.Modular ProfileModular, pp.Active ProfileProjectActive, 

		CASE WHEN ISNULL(je.ProjectID, -1) <> ISNULL(jp.ProjectID, -1) then cast(1 as bit) else CAST(0 as bit) end [ProjectMismatch],

		je.UserKey1 + IIF(je.UserKey2 is null, '', '|' + je.UserKey2) [FullUserKey],
		jp.UserKey1 + IIF(jp.UserKey2 is null, '', '|' + jp.UserKey2) [ProfileFullUserKey],
		CASE WHEN je.UserKey1 <> jp.UserKey1 OR ISNULL(Je.UserKey2, '') <> ISNULL(jp.UserKey2, '')
			then cast(1 as bit) else CAST(0 as bit) end  [UserKeyMismatch]
FROM SEIDR.JobExecution je
	JOIN REFERENCE.Organization o
		ON je.OrganizationID = o.OrganizationID
	LEFT JOIN REFERENCE.Project p
		ON je.ProjectID = p.ProjectID
	JOIN SEIDR.JobProfile jp
		ON je.JobProfileID = jp.JobProfileiD
	JOIN REFERENCE.Organization po
		ON jp.OrganizationID = po.OrganizationID
	LEFT JOIN REFERENCE.Project pp
		ON jp.ProjectID = pp.ProjectID
	WHERE je.Active = 1
	AND (je.OrganizationID <> jp.OrganizationID OR ISNULL(je.ProjectID, -1) <> ISNULL(jp.ProjectID, -1)
			OR je.UserKey1 <> jp.UserKey1 OR ISNULL(Je.UserKey2, '') <> ISNULL(jp.UserKey2, '')
			)