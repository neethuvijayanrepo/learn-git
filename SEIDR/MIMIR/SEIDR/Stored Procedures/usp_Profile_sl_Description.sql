

CREATE PROCEDURE [SEIDR].[usp_Profile_sl_Description]

AS
BEGIN
	SELECT JobProfileID, Description 
	FROM SEIDR.JobProfile WITH (NOLOCK)	
END