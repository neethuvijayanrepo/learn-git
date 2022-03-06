using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.METRIX_EXPORT.Utility;

namespace SEIDR.METRIX_EXPORT.EDI
{
    [JobBase.IJobMetaData(nameof(EDI276FileGenerationJob), nameof(METRIX_EXPORT), 
                        "EDI 276 File Generation",
                        false, 
                        false,
                        DEFAULT_CONFIGURATION_TABLE,
                        SingleThreaded: true)]
    public class EDI276FileGenerationJob : EDIExportBase
    {
        
        
        public EDI276FileGenerationJob() 
            : base(EDICode.EDI276)
        {

        }
        
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            if (context.ProcessingDate < DateTime.Today)
                return ResultStatusCode.OD;
            if (context.ExportBatchID == 0)
            {
                var settings = context.Settings;
                //Not setup yet.
                var configurations = GetOpenEdiCriteriaList(context);
                context.LogInfo($"Vendor '{context.VendorName}'/ EDI Type: '{EdiTransactionCode}' - Configuration count:{configurations.Count}");
                foreach (var config in configurations)
                {
                    string proj = PrepForFileName(config.Project);
                    string fac = PrepForFileName(config.Facility);
                    string projFolderName = "ProjID" + config.ProjectID;
                    /*if (proj.IndexOf('_') >= 0)
                    {
                        projFolderName = proj.Replace('_', ' ');
                    }
                    else
                        projFolderName = proj;
                        */

                    const int MAX_FOLDER_NAME_LENGTH = 30; //Avoid path errors due to exceeding length limitations
                    if (projFolderName.Length > MAX_FOLDER_NAME_LENGTH)
                        projFolderName = projFolderName.Substring(0, MAX_FOLDER_NAME_LENGTH);
                    
                    string filePath;
                    string fileName = CreateFileName(proj, fac, context.ProcessingDate,this.EdiTransactionCode);

                    if (!string.IsNullOrWhiteSpace(fac))
                    {

                        string facFolderName = "FacID" + config.FacilityID;
                        if (fac.IndexOf('_') > 0)
                            facFolderName = $"FacID{config.FacilityID}{fac.Substring(0, fac.IndexOf('_'))}";

                        filePath = Path.Combine(settings.ArchiveLocation, settings.ExportType, projFolderName, facFolderName, fileName);
                    }
                    else
                    {
                        filePath = Path.Combine(settings.ArchiveLocation, settings.ExportType, projFolderName, fileName);
                    }

                    string dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    ExportChildExecutionInfo c = new ExportChildExecutionInfo(context, filePath, context.ExportType)
                    {
                        Branch = "EXPORT",
                        UserKeyOverride = config.FacilityID,
                        ProjectID = config.ProjectID, //Note: Not actually needed because we'll be using the ExportBatch value..
                        //NOTE: to allow more flexibility in testing, just set stopAfterStepNumber = 0 at profile level, then clear on individual job executions.
                        ContinueToNextStep = false
                    };
                    context.RegisterChildExecutionWithExport(c);
                }

                return ResultStatusCode.C;
            }

            var exportBatch = GetExportBatch(context, context.ExportType);
            var stage = CheckStage(context);
            workingFile.OutputFilePath = context.CurrentFilePath;
            bool GenerateBatchData = stage < ExportBatchStage.DATA_PULL 
                                     && 0 == (exportBatch.RecordCount ?? 0);

            var metrix = GetMetrixDatabaseManager(context);
            if (stage < ExportBatchStage.FINALIZED)
            {
                using (var helper = context.GetExportBatchHelperModel("usp_GenerateEdi276Requests"))
                {
                    metrix.IncreaseCommandTimeOut(600);
                    helper[nameof(context.VendorName)] = context.VendorName;
                    helper[nameof(exportBatch.ExportProfileID)] = exportBatch.ExportProfileID;
                    helper[nameof(exportBatch.FacilityID)] = exportBatch.FacilityID;
                    helper[nameof(GenerateBatchData)] = GenerateBatchData;

                    var data = metrix.Execute(helper);
                    if (data.Tables.Count == 0 || data.Tables[0].Rows.Count == 0)
                    {
                        exportBatch.SetExportStatus(ExportStatusCode.ND);
                        return ResultStatusCode.ND;
                    }

                    SetCheckPoint_FileCreation(context);
                    List<string> ExportedAccountIDs = new List<string>();
                    //In the future, if additional vendors - add vendor specific logic here
                    var fileData = EDI276Utility.GenerateEDI276String(data,
                                                                      new string[0],
                                                                      exportBatch,
                                                                      ExportedAccountIDs);

                    File.WriteAllText(workingFile, fileData);
                    workingFile.Finish();
                    exportBatch.SetExportStatus(ExportStatusCode.SI);
                    
                    StringBuilder ExportedIDs = new StringBuilder();
                    foreach (var account in ExportedAccountIDs)
                    {
                        ExportedIDs.AppendFormat("<acctid>{0}</acctid>", account);
                    }

                    helper.QualifiedProcedure = "EXPORT.usp_EDI276Request_UpdateExported"; //If testing, need to 
                    helper[nameof(ExportedIDs)] = $"<exported exportbatchid='{context.ExportBatchID}'>{ExportedIDs}</exported>";
                    helper["RowsUpdated"] = DBNull.Value;
                    metrix.ExecuteNonQuery(helper);

                    if (!exportBatch.RecordCount.HasValue)
                    {
                        exportBatch.RecordCount = (int) helper["RowsUpdated"];
                        UpdateExportBatch(context, exportBatch);
                    }
                    SetCheckPoint_Finalize(context);
                }
            }

            if (context.ImportType != null)
            {
                string importFilePath = System.Text.RegularExpressions.Regex.Replace(context.CurrentFilePath, "276$", "277");
                FileInfo temp = new FileInfo(importFilePath);
                DirectoryInfo importDir = new DirectoryInfo(Path.Combine(context.Settings.ArchiveLocation, context.ImportType));
                try
                {
                    if (!importDir.Exists)
                        importDir.Create();
                    if (temp.Directory != null && temp.Directory.Name != context.ExportType)
                    {
                        importDir = new DirectoryInfo(Path.Combine(importDir.FullName, temp.Directory.Name));
                        if (!importDir.Exists)
                            importDir.Create();
                    }
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

            return DEFAULT_RESULT;
        }
    }
}
