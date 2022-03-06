using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cymetrix.Andromeda.ClaimStatus;

namespace SEIDR.METRIX_EXPORT.EDI._277
{

    [JobBase.IJobMetaData(nameof(EDI277FileImportJob), nameof(METRIX_EXPORT),
                          "EDI 277 File Import",
                          true,
                          false,
                          DEFAULT_CONFIGURATION_TABLE,
                          SingleThreaded: true)]
    public class EDI277FileImportJob : EDIExportBase
    {
        public EDI277FileImportJob() : base(EDICode.EDI277)
        {
        }
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

            using (var h = context.GetExportBatchHelperModel("STAGING.usp_Edi277_CleanLoadTables"))
                metrix.ExecuteNonQuery(h); //Clean load tables before insert.

            string filePath = workingFile;
            using (FileStream fs = File.OpenRead(filePath))
            {
                EDI_277 e277 = new EDI_277(fs, 100000, batchModel.ExportBatchID, workingFile.OutputFileName);
                fs.Close(); //Actually finished with file stream after the 277 wrapper object is created.

                x12BulkLoader bl = new x12BulkLoader(10000, metrix.GetConnection().ConnectionString, 40);
                bl.bulkCopy(e277.Dt277Table);
                bl.bulkCopy(e277.StatusInfoTable);
                if (e277.ServiceLineParentTable.Rows.Count > 0)
                {
                    bl.bulkCopy(e277.ServiceLineParentTable);
                    if (e277.ServiceLineDataTable.Rows.Count > 0)
                    {
                        bl.bulkCopy(e277.ServiceLineDataTable);
                    }
                }
                if (e277.TA1Table.Rows.Count > 0)
                {
                    bl.bulkCopy(e277.TA1Table);
                }
                batchModel.RecordCount = e277.Dt277Table.Rows.Count;
            
            }

            if (batchModel.RecordCount > 0)
            {
                context.LogInfo($"Loaded {batchModel.RecordCount} records to STAGING.EDI277_Load");
                using (var h = context.GetExportBatchHelperModel("APP.usp_Insert277FromStaging"))
                {
                    int ReturnValue = 0;
                    //Because the original author used an output parameter instead of normal procedure return value
                    h[nameof(ReturnValue)] = 0;
                    metrix.IncreaseCommandTimeOut(360);
                    metrix.ExecuteNonQuery(h);
                    ReturnValue = (int)h[nameof(ReturnValue)];
                    if (ReturnValue != 0)
                    {
                        context.LogError("UnExpected @ReturnValue during 277 insert: " + ReturnValue);
                        return ResultStatusCode.IE;
                    }
                    batchModel.SetExportStatus(ExportStatusCode.C); //No FTP Step, so we're already done.
                }
            }
            else
            {
                context.LogInfo("No records imported.");
                batchModel.SetExportStatus(ExportStatusCode.ND); //No Data.
            }

            UpdateExportBatch(context, batchModel);

            workingFile.ClearWork();
            return DEFAULT_RESULT;
        }
    }
}
