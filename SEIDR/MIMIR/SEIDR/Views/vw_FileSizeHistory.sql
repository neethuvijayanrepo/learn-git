
CREATE VIEW SEIDR.vw_FileSizeHistory
AS
SELECT FileSizeHistoryID, 
	fh.FileSizeCheckJobID, 
	JobExecutionID, 
	FilePath, 
	UTIL.ufn_PathItem_GetName(FilePath) [FileName], 
	FileDate, 
	DayOfWeek, 
	PassValidation, 
	AllowContinue,
	CONVERT(bit, CASE WHEN PassValidation <> AllowContinue then 1 else 0 end) [ManualOverride],
	FileSize, 
	CASE WHEN j.DoPercentCheck = 0 then j.StandardDeviationMultiplier end [StandardDeviationMultiplier],
	j.PercentThreshold,
	IsEmptyFile = CONVERT(bit, IIF(fh.FileSize <= j.EmptyFileSize, 1, 0)),
	DeviationAtEvaluation [AllowedDeviation], 
	ABS(FileSize - AverageAtEvaluation) [DeviationFromAverage],  
	AverageAtEvaluation,
	CONVERT(int, 100 * CASE WHEN FileSize < AverageAtEvaluation then 1.00 - (1.0 * FileSize / AverageAtEvaluation) end) [PercentageBelowAverageAtEvaluation],
	CONVERT(bit, CASE WHEN AllowContinue = 1 AND Active = 1 AND FileSize > j.EmptyFileSize then 1
			ELSE 0 
			end) [IncludeInFutureAverage],	
	CONVERT(int, 100 * CASE WHEN AverageAtEvaluation > DeviationAtEvaluation then 1.00 - (1.0 * (AverageAtEvaluation - DeviationAtEvaluation) / AverageAtEvaluation) end) [FileBoundaryPercentage],
	AverageAtEvaluation - DeviationAtEvaluation [SmallFileBoundary],
	CONVERT(int, 100 * CASE WHEN AverageAtEvaluation > DeviationAtEvaluation then (1.0 * (AverageAtEvaluation - DeviationAtEvaluation) / AverageAtEvaluation) end) [SmallFileBoundaryPercentageOfAverage],
	CheckSmallFileBoundary = 
	CONVERT(bit, 
		CASE WHEN fh.FileSize <= j.EmptyFileSize
			then 1 - CASE fh.DayOfWeek
				WHEN 1 then j.IgnoreEmptyFileSunday
				WHEN 2 then j.IgnoreEmptyFileMonday
				WHEN 3 then j.IgnoreEmptyFileTuesday
				WHEN 4 then j.IgnoreEmptyFileWednesday
				WHEN 5 then j.IgnoreEmptyFileThursday
				WHEN 6 then j.IgnoreEmptyFileFriday
				ELSE j.IgnoreEmptyFileSaturday
				END
			else 1 - CASE fh.DayOfWeek
				WHEN 1 then j.IgnoreSunday
				WHEN 2 then j.IgnoreMonday
				WHEN 3 then j.IgnoreTuesday
				WHEN 4 then j.IgnoreWednesday
				WHEN 5 then j.IgnoreThursday
				WHEN 6 then j.IgnoreFriday
				ELSE j.IgnoreSaturday
				END
			END),
	AverageAtEvaluation + DeviationAtEvaluation [LargeFileBoundary],	
	CONVERT(int, 100 * CASE WHEN AverageAtEvaluation > 0 then (1.0 * (AverageAtEvaluation + DeviationAtEvaluation) / AverageAtEvaluation) end) [LargeFileBoundaryPercentageOfAverage],
	CheckLargeFileBoundary = 
	CONVERT(bit, 
		CASE WHEN j.CheckLargeFiles = 0 then 0
			WHEN fh.FileSize <= j.EmptyFileSize then 0
			else 1 - CASE fh.DayOfWeek
				WHEN 1 then j.IgnoreSunday
				WHEN 2 then j.IgnoreMonday
				WHEN 3 then j.IgnoreTuesday
				WHEN 4 then j.IgnoreWednesday
				WHEN 5 then j.IgnoreThursday
				WHEN 6 then j.IgnoreFriday
				ELSE j.IgnoreSaturday
				END
			END)
FROM SEIDR.FileSizeHistory fh
JOIN SEIDR.FileSizeCheckJob j
	ON fh.FileSizeCheckJobID = j.FileSizeCheckJobID
WHERE Active = 1