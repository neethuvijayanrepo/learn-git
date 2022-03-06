UPDATE SEIDR.FileSystemOperation
SET RequireSource = 0, RequireOutputPath = 1
WHERE FileSystemOperationCode IN ('CREATE_DUMMY', 'CREATEDIR')

  INSERT INTO [SEIDR].[FileSystemOperation](FileSystemOperationCode, Description, RequireSource, RequireOutputPath)
  SELECT *
  FROM(
  VALUES
  ('CREATEDIR', 'Create Directory', 0, 1),
  ('CREATE_DUMMY', 'Create Dummy File', 0, 1),
  ('CREATE_DUMMY_TAG', 'Create Dummy File with Tag', 0, 1),
  ('GRAB', 'Grab File, move to Destination. Identical to "MOVE".', 0, 1),
  ('MOVE', 'Move File to Destination', 0, 1),
  ('COPY', 'Copy File to Destination', 0, 1),
  ('TAG', 'Copy File to Destination, append FileName.', 0, 1),
  ('TAG_DEST', 'Copy file to Destination, append destination FileName', 0, 1),
  ('GRAB_ALL', 'Grab all files in Source Directory that match filter, Move to Destination', 1, 1),
  ('CHECK', 'Check if the file exists. Identical to "EXIST"', 1, 0),
  ('EXIST', 'Check if the file exists.', 1, 0),
  ('DELETE', 'Delete the file specified. If "Source" is not specified, uses the File specified by the JobExecution', 0, 0),
  ('MOVEDIR', 'Moves the entire directory to a new path. Does not register moved files.', 1, 1),
  ('COPYDIR', 'Copies the directory''s content to a  new path. Does not register copied files.', 1, 1),
  ('ZIP', 'Create .Zip file at Destination.', 0, 1),
  ('UNZIP', 'Take a .Zip file from Source and Extract file into  OutputPath.', 0, 1),
  ('COPY_METRIX', 'Copy Source File to Metrix Input folder for the linked LoadProfileID.', 0, 0),
  ('MOVE_METRIX', 'Move Source File to Metrix Input folder for the linked LoadProfileID.', 0, 0),
  ('COPY_ALL', 'Copy all source files that match filter to the destination folder.', 1, 1),
  ('MOVE_ALL', 'Move all files in Source directory that match filter to the Destination. Identical to "GRAB_ALL"', 1, 1),
  ('CHECK_FILTER', 'Compare file path against filter. If filter matches, return "FM", else "FD".
  Allow moving forward a certain way only for certain filters, or by excluding certain filters.', 1, 0),
  ('SIZE_CHECK', 'Perform basic size check with hard coded percentage. For more control, use FileValidationJob.', 1, 0),
  ('COPY_ANY', 'Copy any Source files  that match filter to the destination folder. Does not fail if no files are found.', 1,1),
  ('MOVE_ANY', 'Move any Source files  that match filter to the destination folder. Does not fail if no files are found.', 1,1),
  ('CLEAN_COPY', 'Perform a simple clean on the file, and then Copy to Destination', 0, 1),
  ('CLEAN_COPY_METRIX', 'Perform a simple clean on the file, and then Copy to Metrix Input folder for the linked LoadProfileID', 0,0)
  )C(FileSystemOperationCode, Description, RequireSource, RequireDestination)
  WHERE NOT EXISTS(SELECT null FROM SEIDR.FileSystemOperation WHERE FileSystemOperationCode = c.FileSystemOperationCode)

  UPDATE SEIDR.FileSystemOperation
  SET RequireFilter = 1
  WHERE FileSystemOperationCode LIKE '%_ANY'
  OR FileSystemOperationCode LIKE '%_ALL'
  OR FileSystemOperationCode IN('CHECK_FILTER', 'ZIP')
  AND RequireFilter = 0
  

  

  
INSERT INTO SEIDR.Priority(PriorityCode, PriorityValue)
SELECT *
FROM (
VALUES	
	('TRIVIAL', 1),
	('LOW', 3),
	('NORMAL', 6),
	('HIGH', 8),	
	('URGENT', 10),
	('CRITICAL', 12),
	('BLOCKER', 15)
)c(PriorityCode, Value)
WHERE NOT EXISTS(SELECT null FROM SEIDR.Priority WHERE PriorityCode = c.PriorityCode)