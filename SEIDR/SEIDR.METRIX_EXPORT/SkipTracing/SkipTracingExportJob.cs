using SEIDR.METRIX_EXPORT.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.SkipTracing
{
    [JobBase.IJobMetaData(nameof(SkipTracingExportJob), nameof(METRIX_EXPORT),
                       "SkipTracing Export",
                       false,
                       false,
                       DEFAULT_CONFIGURATION_TABLE,
                       SingleThreaded: true)]
   public class SkipTracingExportJob : ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            if (context.ProcessingDate < DateTime.Today)
                return ResultStatusCode.OD;

            ExportBatchModel exportBatch;

            if (context.ExportBatchID == 0)
            {
                context.UpdateFilePath($"{context.Settings.ArchiveLocation}\\LexisNexisBatchFile_{context.Execution.OrganizationID}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.csv");
                exportBatch = BeginExportBatch(context, context.ExportType);
            }
            else
            {
                exportBatch = GetExportBatch(context, context.ExportType);
            }

            
            var stage = CheckStage(context);
            var metrix = GetMetrixDatabaseManager(context);
            if (stage < ExportBatchStage.FINALIZED)
            {
                context.LogInfo($"Vendor '{context.VendorName}'/ : ExportBatchID :'{exportBatch.ExportBatchID.ToString()}'");

                using (var helper = context.GetExportBatchHelperModel("usp_SkipTraceRequest_Generate"))
                {
                    metrix.IncreaseCommandTimeOut(600);

                    helper[nameof(context.VendorName)] = context.VendorName;
                    helper[nameof(exportBatch.ExportProfileID)] = exportBatch.ExportProfileID;
                    helper[nameof(exportBatch.ExportBatchID)] = exportBatch.ExportBatchID;
                    helper[nameof(exportBatch.ProjectID)] = exportBatch.ProjectID; 

                    var data = metrix.Execute(helper);
                    if (data.Tables.Count == 0 || data.Tables[0].Rows.Count == 0)
                    {
                        exportBatch.SetExportStatus(ExportStatusCode.ND);
                        return ResultStatusCode.ND;
                    }

                    SetCheckPoint_FileCreation(context);
                    SkipTracingUtil util = new SkipTracingUtil(metrix.GetConnection().ConnectionString);
                    bool isFileCreated = util.ExportSkipTracingDataToCSVFile(context.CurrentFilePath, data);
                    if (isFileCreated)
                    {
                        exportBatch.SetExportStatus(ExportStatusCode.SI);
                        exportBatch.RecordCount = data.Tables[0].Rows.Count;
                        UpdateExportBatch(context, exportBatch);
                        
                        if (context.ImportType != null)
                        {
                            string importFilePath = context.CurrentFilePath.Replace(".csv", "__out.csv");
                            FileInfo temp = new FileInfo(importFilePath);
                            DirectoryInfo importDir = new DirectoryInfo(Path.Combine(context.Settings.ArchiveLocation, context.ImportType));
                            try
                            {
                                if (!importDir.Exists)
                                    importDir.Create();

                                /*  Below adds 'EXPORT' folder to the import process.
                                    Can skip for now - may want to add back if we add project subfolders to the Export      
                                */
                                //if (temp.Directory != null && temp.Directory.Name != context.ExportType)
                                //{
                                //    importDir = new DirectoryInfo(Path.Combine(importDir.FullName, temp.Directory.Name));
                                //    if (!importDir.Exists)
                                //        importDir.Create();
                                //}
                            }
                            catch
                            {
                                context.LogError("ImportDir path:" + importDir.FullName);
                                throw;
                            }

                            string finalImportPath = Path.Combine(importDir.FullName, temp.Name);

                            //Note: Current Execution ProjectID already matches the export batch, but we may still want to set the userKeyOverride so that we don't source from the profile.
                            ExportChildExecutionInfo ci = new ExportChildExecutionInfo(context, finalImportPath, context.ImportType)
                            {
                                Branch = "IMPORT",
                                UserKeyOverride = exportBatch.FacilityID
                            };
                            CreateImportJobExecutionBatch(context, ci, exportBatch.FacilityID);
                        }
                    }
                    else
                    {
                        context.LogInfo($"Vendor '{context.VendorName}'/ : ExportSkipTracingDataToCSVFile failed, ExportBatchID :'{exportBatch.ExportBatchID.ToString()}'");
                        exportBatch.SetExportStatus(ExportStatusCode.SF);
                    }
                }
            }

            return DEFAULT_RESULT;
        }
    }
}
