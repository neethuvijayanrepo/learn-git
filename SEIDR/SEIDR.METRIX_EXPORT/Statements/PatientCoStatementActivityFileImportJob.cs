using System.Data;
using System.IO;
using SEIDR.Doc;
using SEIDR.METRIX_EXPORT.Utility;

namespace SEIDR.METRIX_EXPORT.Statements
{

    [JobBase.IJobMetaData(nameof(PatientCoStatementActivityFileImportJob), nameof(METRIX_EXPORT),
                          "PatientCo Statement Activity File Import",
                          true,
                          false,
                          DEFAULT_CONFIGURATION_TABLE,
                          SingleThreaded: true)]
    public class PatientCoStatementActivityFileImportJob : ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            if (!File.Exists(context.CurrentFilePath))
            {
                return ResultStatusCode.NF;
            }
            //Need to use the file that was pulled from the vendor. Copy to local machine so we don't need to read across a network
            workingFile.InitializeFromFile(context.CurrentFilePath);

            var batchModel = GetExportBatch(context, context.Settings.ImportType);
            var metrix = context.MetrixManager;

            //using (var h = context.GetExportBatchHelperModel("STAGING.usp_Edi277_CleanLoadTables"))
            //    metrix.ExecuteNonQuery(h); //Clean load tables before insert.

            string filePath = workingFile;


            if (File.Exists(filePath))
            {
                DataTable dt = new DataTable();
                using (var addressFile = GetReader(filePath))
                {
                    foreach (DocRecord fileLine in addressFile)
                    {
                        var row = dt.NewRow();//add a row to the datatable, then update the values of that row to use for bulk copying
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            row[i] = fileLine[i];
                        }
                    }
                    // Bulk load the data table into our Andromeda table
                }

                int recordCount = dt != null ? dt.Rows.Count : 0;
                batchModel.RecordCount = recordCount;

                if (recordCount > 0)
                {
                    PatientCoActivityFileUtil util = new PatientCoActivityFileUtil(metrix.GetConnection().ConnectionString);
                    bool success = util.ActivityFileBulkCopy(dt, batchModel.ExportBatchID);
                    if (success)
                    {
                        batchModel.SetExportStatus(ExportStatusCode.C);
                        context.LogInfo($"Loaded {recordCount} records to IMPORT.PatientCoActivityFile");


                        ////reconciliation vs statements ??
                    }
                    else
                    {
                        batchModel.SetExportStatus(ExportStatusCode.SF);
                        context.LogInfo($"Load Fail : IMPORT.PatientCoActivityFile");
                    }
                }

                UpdateExportBatch(context, batchModel);
            }
            else
            {
                batchModel.SetExportStatus(ExportStatusCode.ND);
                UpdateExportBatch(context, batchModel);
                return ResultStatusCode.NF;
            }
            workingFile.ClearWork();
            return DEFAULT_RESULT;

            UpdateExportBatch(context, batchModel);

            workingFile.ClearWork();
            return DEFAULT_RESULT;
        }
    }
}
