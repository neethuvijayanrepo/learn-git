using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;
using System.IO;
using SEIDR.Doc;

namespace SEIDR.FileSystem.FileConcatenation
{
    [IJobMetaData(nameof(FileConcatenationJob), nameof(SEIDR.FileSystem), "Concatenate file with shared format to current File of JobExecution", AllowRetry:false, NeedsFilePath: true, ConfigurationTable: "SEIDR.FileConcatenationJob")]
    public class FileConcatenationJob: IJob
    {
        const string SS = "[SEIDR].[usp_FileConcatenationJob_ss]";

        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        public void DoConcatenation(JobExecution execution, FileConcatenationSettings settings)
        {
            settings.SecondaryFilePath = FS.ApplyDateMask(settings.SecondaryFilePath, execution.ProcessingDate);
            settings.OutputPath = FS.ApplyDateMask(settings.OutputPath, execution.ProcessingDate);
            var f1 = new Doc.DocMetaData(execution.FilePath)
                            .SetHasHeader(settings.HasHeader)
                            .SetMultiLineEndDelimiters("\r\n", "\n", "\r");
            var f2 = new Doc.DocMetaData(settings.SecondaryFilePath)
                            .SetHasHeader(settings.SecondaryFileHasHeader)
                            .SetMultiLineEndDelimiters("\r\n", "\n", "\r");

            using (DocReader r1 = new DocReader(f1))
            using (DocReader r2 = new DocReader(f2))
            {
                var output = new Doc.DocMetaData(settings.OutputPath)
                                    .SetHasHeader(settings.HasHeader || settings.SecondaryFileHasHeader)
                                    .SetLineEndDelimiter("\n")
                                    .SetDelimiter(f1.Delimiter ?? f2.Delimiter ?? '|');

                if (settings.HasHeader || !settings.SecondaryFileHasHeader)
                    output.AddDetailedColumnCollection(r1);
                else
                    output.AddDetailedColumnCollection(r2);

                using (DocWriter dw = new DocWriter(output))
                {
                    dw.BulkWrite(r1);
                    dw.BulkWrite(r2);
                }
                execution.FilePath = output.FilePath;
                execution.FileHash = output.FileHash;
                FileInfo fi = new FileInfo(output.FilePath);
                execution.FileSize = fi.Length;
            }
        }            
        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            if(string.IsNullOrWhiteSpace(execution.FilePath))
            {
                status = new ExecutionStatus {ExecutionStatusCode = "NS", Description = "No Source File", IsError = true };
                return false;
            }
            var settings = jobExecutor.Manager.SelectSingle<FileConcatenationSettings>(new { execution.JobExecutionID, execution.JobProfile_JobID });
            DoConcatenation(execution, settings);
            return true;
        }
    }
}
