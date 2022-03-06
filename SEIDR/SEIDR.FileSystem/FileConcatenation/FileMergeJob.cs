using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.FileConcatenation
{
    [IJobMetaData(nameof(FileMergeJob), nameof(FileSystem), 
        "Merge two files together.", NeedsFilePath:true,
        AllowRetry:false,
        ConfigurationTable:"SEIDR.FileMergeJob")]
    public class FileMergeJob : ContextJobBase<FileSystemContext>
    {
        public Func<Doc.DocRecord, Doc.DocRecord, bool> ProcessKeys(FileMergeJobSettings settings)
        {
            StringComparison c = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (settings.RightKey1 == null)
                settings.RightKey1 = settings.LeftKey1;
            if (settings.LeftKey2 == null)
            {
                if (settings.Trim)
                    return (r1, r2) => string.Equals(r1[settings.LeftKey1].Trim(), r2[settings.RightKey1].Trim(), c);
                else
	                return (r1, r2) => string.Equals(r1[settings.LeftKey1], r2[settings.RightKey1], c);
            }
            if (settings.RightKey2 == null)
                settings.RightKey2 = settings.LeftKey2;
            if (settings.LeftKey3 == null)
            {
                if(settings.Trim)
                    return (r1, r2) => string.Equals(r1[settings.LeftKey1].Trim(), r2[settings.RightKey1].Trim(), c) 
                                    && string.Equals(r1[settings.LeftKey2].Trim(), r2[settings.RightKey2].Trim(), c);
                return (r1, r2) => string.Equals(r1[settings.LeftKey1], r2[settings.RightKey1], c) 
                                   && string.Equals(r1[settings.LeftKey2], r2[settings.RightKey2], c);
            }

            if (settings.RightKey3 == null)
                settings.RightKey3 = settings.LeftKey3;
            if(settings.Trim)
                return (r1, r2) => string.Equals(r1[settings.LeftKey1].Trim(), r2[settings.RightKey1].Trim(), c)
                                   && string.Equals(r1[settings.LeftKey2].Trim(), r2[settings.RightKey2].Trim(), c)
                                   && string.Equals(r1[settings.LeftKey3].Trim(), r2[settings.RightKey3].Trim(), c);
            return (r1, r2) => string.Equals(r1[settings.LeftKey1], r2[settings.RightKey1], c)
                               && string.Equals(r1[settings.LeftKey2], r2[settings.RightKey2], c)
                               && string.Equals(r1[settings.LeftKey3], r2[settings.RightKey3], c);

        }
        public void DoMerge(FileSystemContext context, FileMergeJobSettings settings)
        {

            var file2 = FS.ApplyDateMask(settings.MergeFile, context);
            var output = FS.ApplyDateMask(settings.OutputFilePath, context);

            if (!File.Exists(file2))
            {
                context.LogError($"Could not find file at path '{file2}'.");
                context.SetStatus(ResultStatusCode.NS);
                return;
            }

            string fileName2 = Path.GetFileName(file2);

            if (File.Exists(output))
            {
                if (settings.Overwrite)
                {
                    File.Delete(output);
                }
                else
                {
                    context.LogError($"File at '{output}' already exists.");
                    context.SetStatus(ResultStatusCode.AE);
                    return;
                }
            }

            Func<Doc.DocRecord, Doc.DocRecord, bool> evaluate = ProcessKeys(settings);

            //JobExecution's FilePath
            var in1 = context.GetExecutionLocalFile();
            if (Path.GetFileNameWithoutExtension(context.CurrentFileName).Equals(Path.GetFileNameWithoutExtension(fileName2)))
                in1.TimeStamp(); //Prevent issue with overlapping local filepath.

            //Merge file's local file
            var in2 = context.GetLocalFile(file2);

            //Output file
            var lout = context.ReserveBasicLocalFile(output, true);
            context.WorkingFile = lout; //Base class auto call Finish on the working file once we return.

            const string TEXT_QUALIFIER = "\"";
            var mdL = in1.GetDocMetaData();
            mdL.HasHeader = settings.LeftInputHasHeader;

            var mdR = in2.GetDocMetaData();
            mdR.HasHeader = settings.RightInputHasHeader;

            if (settings.HasTextQualifier)
            {
                mdL.Columns.TextQualifier = mdR.Columns.TextQualifier = TEXT_QUALIFIER;
            }


            //Use Default meta data parse. Need to make sure that column names are unique before we get into here.
            using (var r1 = new Doc.DocReader(mdL)) //Alias will come from temp file's name.
            using (var r2 = new Doc.DocReader(mdR))
            {

                context.LogInfo("File1 RecordCount: " + r1.RecordCount);
                mdL.SetColumnMatchStrictMode();
                StringComparison c = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                bool columnIssue = false;
                for (int i = 0; i < r1.Columns.Count - 1; i++)
                {
                    for (int j = i + 1; j < r1.Columns.Count; j++)
                    {
                        if (r1.Columns[i].ColumnName.Equals(r1.Columns[j].ColumnName, c))
                        {
                            columnIssue = true;
                            context.LogError($"Column '{r1.Columns[i].ColumnName}' found at position {i} and {j} in '{context.Execution.FileName}'.");
                        }
                    }
                    /* //Not actually an issue if the different files have the same column name, because of file alias.
                    for (int j = 0; j < r2.Columns.Count; j++)
                    {
                        if (r1.Columns[i].ColumnName.Equals(r2.Columns[j].ColumnName))
                        {
                            dupeColumn = true;
                            context.LogError($"Column '{r1.Columns[i].ColumnName}' found at position {i} in '{context.Execution.FileName}' and position {j} in {fileName2}.");
                        }
                    }*/
                }

                for (int i = 0; i < r2.Columns.Count; i++)
                {
                    for (int j = i + 1; j < r2.Columns.Count; j++)
                    {
                        if (r2.Columns[i].ColumnName.Equals(r2.Columns[j].ColumnName, c))
                        {
                            columnIssue = true;
                            context.LogError($"Column '{r2.Columns[i].ColumnName}' found at position {i} and position {j} in '{fileName2}'.");
                        }
                    }
                }
                if (columnIssue)
                {
                    context.LogError("Cannot Merge files if columns are specified more than once.");
                    context.SetStatus(ResultStatusCode.DC);
                    return;
                }

                if (mdL.Columns.NotExists(col => col.ColumnName == settings.LeftKey1))
                {
                    context.LogError($"Column '{settings.LeftKey1}' not found in '{context.Execution.FileName}'.");
                    columnIssue = true;
                }
                if(mdR.Columns.NotExists(col => col.ColumnName == settings.RightKey1))
                {
                    context.LogError($"Column '{settings.RightKey1}' not found in '{fileName2}'.");
                    columnIssue = true;
                }

                if (settings.LeftKey2 != null)
                {

                    if (mdL.Columns.NotExists(col => col.ColumnName == settings.LeftKey2))
                    {
                        context.LogError($"Column '{settings.LeftKey2}' not found in '{context.Execution.FileName}'.");
                        columnIssue = true;
                    }
                    if (mdR.Columns.NotExists(col => col.ColumnName == settings.RightKey2))
                    {
                        context.LogError($"Column '{settings.RightKey2}' not found in '{fileName2}'.");
                        columnIssue = true;
                    }

                    if (settings.LeftKey3 != null)
                    {
                        if (mdL.Columns.NotExists(col => col.ColumnName == settings.LeftKey3))
                        {
                            context.LogError($"Column '{settings.LeftKey3}' not found in '{context.Execution.FileName}'.");
                            columnIssue = true;
                        }
                        if (mdR.Columns.NotExists(col => col.ColumnName == settings.RightKey3))
                        {
                            context.LogError($"Column '{settings.RightKey3}' not found in '{fileName2}'.");
                            columnIssue = true;
                        }
                    }
                }

                if (columnIssue)
                {
                    context.LogError("Unable to merge with missing Join columns");
                    context.SetStatus(ResultStatusCode.MC);
                    return;
                }

                System.Diagnostics.Debug.Assert(r1.MetaData.Delimiter.HasValue);

                // Merge meta data from the two files, maintaining alias (FileName w/out extension)
                // to indicate where each column maps from.
                var cols = Doc.DocRecordColumnCollection.Merge(r1, r2);
                cols.TextQualifier = TEXT_QUALIFIER;
                if (settings.RemoveDuplicateColumns)
                {
                    var q = from col in r2.Columns
                            where mdL.Columns.Exists(r1C => r1C.ColumnName.Trim() == col.ColumnName.Trim())
                            select col;
                    cols.RemoveColumnInfo(q);
                }

                if (settings.RemoveExtraMergeColumns)
                {
                    var col = r2.Columns.GetBestMatch(settings.RightKey1);
                    cols.RemoveColumnInfo(col);
                    if(!string.IsNullOrEmpty(settings.RightKey2))
                    {
                        col = r2.Columns.GetBestMatch(settings.RightKey2);
                        cols.RemoveColumnInfo(col);
                    }

                    if (settings.RightKey3 != null)
                    {
                        col = r2.Columns.GetBestMatch(settings.RightKey3);
                        cols.RemoveColumnInfo(col);
                    }
                }

                bool checkExist = settings.RemoveDuplicateColumns || settings.RemoveExtraMergeColumns;
                var md = new Doc.DocMetaData(lout);
                md.SetLineEndDelimiter(Environment.NewLine);
                if (settings.KeepDelimiter)
                	md.SetDelimiter(r1.MetaData.Delimiter.Value);
                else
                    md.SetDelimiter('|');
                md.HasHeader = settings.IncludeHeader;
                md.CopyDetailedColumnCollection(cols);
                using (var w = new Doc.DocWriter(md))
                {
                    if (settings.PreSorted)
                    {
                        long ridx = 0;
                        //Driven from left side file
                        for (long idx = 0; idx < r1.RecordCount; idx++)
                        {
                            bool found = false;
                            var left = r1[idx++];

                            if (ridx >= r2.RecordCount)
                            {
                                if (settings.InnerJoin)
                                {
                                    return;
                                }

                                w.AddRecord(left);
                                continue;
                            }

                            var right = r2[ridx++];
                            while (evaluate(left, right))
                            {
                                found = true;
                                w.AddRecord(Doc.DocRecord.Merge(md, left, right, checkExist));
                                if (ridx >= r2.RecordCount)
                                {
                                    if (settings.InnerJoin)
                                        return;
                                    ridx++; //Going to subtract 1 from ridx because normally we exit the loop when we don't find a match.
                                    //but here we leave loop because we finished the right side.
                                    break;
                                }
                                right = r2[ridx++];
                            }

                            if (found)
                                ridx--; //need to reevaluate the record that just failed and caused the loop to exit.
                            if (found || settings.InnerJoin)
                                continue;
                            w.AddRecord(left);
                        }
                        return;
                    }

                    object wLock = new object();
                    object counterLock = new object();
                    for (int lPage = 0; lPage < r1.PageCount; lPage++)
                    {
                        var leftPage = r1.GetPage(lPage);
                        bool[] matched = new bool[leftPage.Count];
                        //int leftCounter = 0; //for left Record matching.
                        for (int rPage = 0; rPage < r2.PageCount; rPage++)
                        {
                            var rightPage = r2.GetPage(rPage);
                            Action<Doc.DocRecord, ParallelLoopState, long> a = (left, s, idx) =>
                            {
                                /*int idx;
                                lock(counterLock)
                                    idx = leftCounter++;*/
                                foreach (var right in rightPage)
                    {
                                    if (!evaluate(left, right))
                                        continue;
                                    matched[idx] = true;
                                    lock (wLock)
                        {
                                        // ReSharper disable once AccessToDisposedClosure
                                        w.AddRecord(Doc.DocRecord.Merge(md, left, right, checkExist));
                                    }
                                }
                            };
                            Parallel.ForEach(leftPage, a);
                            //Next page from right file.
                        }
                        if (settings.InnerJoin)
                            continue; //next page from original file.
                        for (int i = 0; i < matched.Length; i++)
                            {
                            if (matched[i])
                                continue;
                            w.AddRecord(leftPage[i]);
                            }
                        }
                    }
                }
            }
        public override void Process(FileSystemContext context)
        {
            FileMergeJobSettings settings = context.Manager.SelectSingle<FileMergeJobSettings>(context);

            DoMerge(context, settings);

        }
    }
}
