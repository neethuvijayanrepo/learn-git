using SEIDR.DataBase;
using SEIDR.JobBase;
using System;
using SEIDR.Doc;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SEIDR.FileSystem.FileSort
{
    [IJobMetaData(nameof(FileSortJob), nameof(FileSystem.FileSort), 
        "File sort Operation", ConfigurationTable: "SEIDR.DocMetaData", NeedsFilePath: true,
        AllowRetry:false)]
   public class FileSortJob : IJob
    {
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            DatabaseManager dm = jobExecutor.Manager;  //new DatabaseManager(jobExecutor.connection);
            DocMetaDataSet DocMetaDS = new DocMetaDataSet(dm, execution.JobProfile_JobID, execution.ProcessingDate);
            List<DocMetaDataColumn> metaDataColumns = DocMetaDS.MetaDataColumns;
            if (string.IsNullOrWhiteSpace(execution.FilePath) || !new FileInfo(execution.FilePath).Exists)
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NS",
                    IsError = true
                };
                return false;
            }

            Doc.DocMetaData metaData = new Doc.DocMetaData(execution.FilePath)
                .SetEmptyIsNull(true)
                .SetFileAccess(FileAccess.ReadWrite)
                .SetMultiLineEndDelimiters("\r", "\n", "\r\n");

            if (DocMetaDS.MetaData != null)
            {
                metaData.SetHasHeader(DocMetaDS.MetaData.HasHeader);
                metaData.SetSkipLines(DocMetaDS.MetaData.SkipLines);
                metaData.SetDelimiter((char)DocMetaDS.MetaData.Delimiter);
                metaData.Columns.TextQualifier = DocMetaDS.MetaData.TextQualifier;
            }

            using (var dr = new DocReader(metaData))
            {                
                if (DocMetaDS.MetaDataColumns != null)
                {
                    string msg = string.Empty;
                    ValidationError errCode = DocMetaDS.CompareColumnData(dr.Columns, out msg);
                    if (errCode == ValidationError.NC)
                    {
                        metaDataColumns = AddDocMetaDataCoumns(metaData, execution, dm, jobExecutor, out status);
                        if (status != null)
                            return false;
                    }
                    else if (errCode != ValidationError.None)
                    {
                        status = new ExecutionStatus
                        {
                            ExecutionStatusCode = errCode.ToString(),
                            Description = errCode.GetDescription(),
                            IsError = true
                        };
                        jobExecutor.LogError(errCode.ToString() + ": " + msg);
                        return false;
                    }
                }
                else
                {
                    metaDataColumns = AddDocMetaDataCoumns( metaData, execution, dm, jobExecutor, out status);
                    if (status != null)
                        return false;
                }

                List<DocMetaDataColumn> sortMetaColumns = metaDataColumns.Where(column => column.SortPriority.HasValue).OrderBy(col => col.SortPriority).ToList();

                //if SortPriority not configured then select following columns and sort priority would be Facility%, Account%, Encounter%
                if (sortMetaColumns.Count == 0)
                {
                    sortMetaColumns = metaDataColumns.Where(column => column.ColumnName.StartsWith("Facility", StringComparison.OrdinalIgnoreCase) ||
                                                       column.ColumnName.StartsWith("Account", StringComparison.OrdinalIgnoreCase) ||
                                                       column.ColumnName.StartsWith("Encounter", StringComparison.OrdinalIgnoreCase)
                                                       ).OrderByDescending(column => column.ColumnName).ToList();
                    if (sortMetaColumns.Count > 0)
                        if (!sortMetaColumns[0].ColumnName.StartsWith("Facility", StringComparison.OrdinalIgnoreCase) && sortMetaColumns.Count == 2)
                            sortMetaColumns = sortMetaColumns.OrderBy(column => column.ColumnName).ToList();
                        else if (sortMetaColumns.Count == 3)
                        {
                            DocMetaDataColumn tempSwitchColumn = sortMetaColumns[2];
                            sortMetaColumns[2] = sortMetaColumns[1];
                            sortMetaColumns[1] = tempSwitchColumn;
                        }
                }

                DocRecordColumnCollection columns = dr.Columns;
                List<SortColumn> sortColumnList = new List<SortColumn>();
                bool isColumn0Present = false;
                foreach (var item in sortMetaColumns)
                {
                    sortColumnList.Add(new SortColumn(item.Position, item.SortASC));
                    if (item.Position == 0)
                        isColumn0Present = true;
                }

                if (!isColumn0Present)
                    sortColumnList.Add(new SortColumn(0, metaDataColumns[0].SortASC));
                
                SortColumn[] sortColumns = sortColumnList.ToArray();
                DocSorter docSort = new DocSorter(dr, sortColumns);
                docSort.WriteToFile(execution.FilePath + ".Sorted");
                execution.FilePath = execution.FilePath + ".Sorted";
            }
            
            return true;
        }

        private List<DocMetaDataColumn> AddDocMetaDataCoumns(Doc.DocMetaData metaData, JobExecution execution, DatabaseManager dm, IJobExecutor jobExecutor, out ExecutionStatus status)
        {
            List<DocMetaDataColumn>  metaDataColumns = new List<DocMetaDataColumn>();
            metaData.Columns.ForEach(col =>
            {
                metaDataColumns.Add(new DocMetaDataColumn
                {
                    ColumnName = col.ColumnName.Trim(),
                    Position = col.Position,
                    SortASC = true

                });
            });

            var ColumnMetaData = new System.Data.DataTable("udt_DocMetaDataColumn");
            ColumnMetaData.AddColumns<DocMetaDataColumn>();
            metaDataColumns.ForEach(c => ColumnMetaData.AddRow(c));
            using (var helper = dm.GetBasicHelper())
            {
                helper.ExpectedReturnValue = 0;
                helper[nameof(execution.JobProfile_JobID)] = execution.JobProfile_JobID;
                helper[nameof(ColumnMetaData)] = ColumnMetaData;
                helper[nameof(DocMetaData.Delimiter)] = metaData.Delimiter;
                helper[nameof(DocMetaData.HasHeader)] = metaData.HasHeader;
                helper[nameof(DocMetaData.SkipLines)] = metaData.SkipLines;
                helper[nameof(DocMetaData.HasTrailer)] = false;
                helper[nameof(DocMetaData.TextQualifier)] = "\"";
                helper[nameof(execution.ProcessingDate)] = execution.ProcessingDate;
                helper.QualifiedProcedure = "SEIDR.usp_DocMetaData_i";
                dm.ExecuteNonQuery(helper);
                if (helper.ReturnValue != helper.ExpectedReturnValue)
                {
                    status = new ExecutionStatus
                    {
                        ExecutionStatusCode = ValidationError.MD.ToString(),
                        Description = ValidationError.MD.GetDescription(),
                        IsError = true
                    };
                    metaDataColumns = null;
                }
                jobExecutor.LogInfo("New DocMetaDataColumns are added for JobProfile_JobID" + execution.JobProfile_JobID.ToString());
             
            }
            status = null;
            return metaDataColumns;
        }

    }
}
