 CREATE VIEW SEIDR.vw_DocMetaData
 as
 SELECT d.MetaDataID, d.JobProfile_JobID, d.[Version], d.FromDate, d.ThroughDate, 
 d.Delimiter, d.TextQualifier, d.HasHeader, d.SkipLines, d.HasTrailer, d.DuplicateHandling, d.IsCurrent,
	c.MetaDataColumnID, c.ColumnName, c.Position, c.Max_Length, c.SortASC, c.SortPriority [SortOrder]
 FROM SEIDR.DocMetaData d
 JOIN SEIDR.DocMetaDataColumn c
	ON d.MetaDataID = c.MetaDataID
 WHERE d.Active = 1