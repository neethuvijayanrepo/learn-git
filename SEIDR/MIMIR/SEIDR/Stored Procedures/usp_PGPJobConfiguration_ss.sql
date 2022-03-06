CREATE PROCEDURE [SEIDR].[usp_PGPJobConfiguration_ss]
(
	@JobProfile_JobID INT
)
AS  
BEGIN    
	SELECT   
		p.PGPJobID,
		p.JobProfile_JobID,
		p.PGPOperationID,
		p.SourcePath,
		p.OutputPath,
		p.PublicKeyFile,
		p.PrivateKeyFile,
		p.KeyIdentity,
		p.PassPhrase,
		p.[Description],
		o.PGPOperationName
	FROM 
		[SEIDR].[PGPJob] p 
		INNER JOIN [SEIDR].[PGPOperation] o ON o.PGPOperationID = p.PGPOperationID
	WHERE 
		p.JobProfile_JobID = @JobProfile_JobID  
		AND p.Active = 1  
END