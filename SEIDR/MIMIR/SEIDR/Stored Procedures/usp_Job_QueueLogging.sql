-- =============================================
-- Author:		<SUMAN M>
-- Create date: <04/09/2018>
-- Description:	<SEIDR.usp_Job_QueueLogging>
-- =============================================
CREATE PROCEDURE [SEIDR].[usp_Job_QueueLogging] 
	@JobProfileID int,
	@RegistrationPath varchar(500),
	@FilePath varchar(500),   
	@FileDate date,  
	@FileName varchar(255),
	@IsRejected bit,
	@IsDuplicate bit     
AS
BEGIN
	INSERT INTO [SEIDR].[QueueRejection] (
		JobProfileID
		,InputFolder
		,DestinationFolder
		,[FileName]
		,FIlePath
		,ProcessingDate
		,Rejected
		,Duplicate)
	SELECT
		@JobProfileID
		,RegistrationFolder
		,@RegistrationPath
		,@FileName
		,@FilePath
		,@FileDate
		,@IsRejected
		,@IsDuplicate
	FROM SEIDR.JobProfile  
	WHERE JobProfileID = @JobProfileID
END