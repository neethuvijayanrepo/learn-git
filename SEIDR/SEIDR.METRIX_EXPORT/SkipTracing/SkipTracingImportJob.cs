using SEIDR.METRIX_EXPORT.Utility;
using System.Data;
using System.IO;

namespace SEIDR.METRIX_EXPORT.SkipTracing
{
    [JobBase.IJobMetaData(nameof(SkipTracingImportJob), nameof(METRIX_EXPORT),
                          "SkipTracing Import",
                          true,
                          false,
                          DEFAULT_CONFIGURATION_TABLE,
                          SingleThreaded: true)]
   public class SkipTracingImportJob : ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            if (!File.Exists(context.CurrentFilePath))
            {
                return ResultStatusCode.NF;
            }
            workingFile.InitializeFromFile(context.CurrentFilePath);
            var importBatch = GetExportBatch(context, context.Settings.ImportType);
            var metrix = context.MetrixManager;
            string filePath = workingFile;

            if (File.Exists(filePath))
            {
                DataTable dt = CsvHelper.ConvertCSVtoDataTable(filePath);
                int recordCount = dt != null ? dt.Rows.Count : 0;
                importBatch.RecordCount = recordCount;

                if (recordCount > 0)
                {
                    SkipTracingUtil util = new SkipTracingUtil(metrix.GetConnection().ConnectionString);
                    bool success = util.SkipTraceResponseBulkCopy(dt, importBatch.ExportBatchID);
                    if (success)
                    {
                        importBatch.SetExportStatus(ExportStatusCode.C);
                        context.LogInfo($"Loaded {recordCount} records to EXPORT.SkipTraceResponse");
                    }
                    else
                    {
                        importBatch.SetExportStatus(ExportStatusCode.SF);
                        context.LogInfo($"Load Fail : EXPORT.SkipTraceResponse");
                    }
                }

                UpdateExportBatch(context, importBatch);
            }
            else
            {
                importBatch.SetExportStatus(ExportStatusCode.ND);
                UpdateExportBatch(context, importBatch);
                return ResultStatusCode.NF;
            }
            workingFile.ClearWork();
            return DEFAULT_RESULT;
        }
    }
}