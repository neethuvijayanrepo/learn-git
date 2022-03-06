using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.ARxChangeExport
{
    [IJobMetaData(nameof(ARxChangeReconciliationJob),
        nameof(METRIX_EXPORT),
        "ARxChange Reconciliation File Generation", NotificationTime: 10,
        NeedsFilePath: false, AllowRetry: true, ConfigurationTable: DEFAULT_CONFIGURATION_TABLE)]
    public class ARxChangeReconciliationJob : ExportJobBase
    {
        #region Declaration.   

        #endregion

        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            var manager = context.MetrixManager;
            int? ExportBatchID = context.Execution.METRIX_ExportBatchID;
            string FilePath = Path.Combine(context.Settings.ArchiveLocation, context.Execution.JobProfileID + "CYM_Reconciliation" + context.Execution.ProcessingDate.ToString("yyyy_MM_dd") + ".csv");
            ExportBatchModel batch;
            const string EXPORT_TYPE = "ARxChange Export";
            if (ExportBatchID == null)
            {
                if (manager == null)
                {
                    context.LogError("Unable to get Metrix connection.");
                    return DEFAULT_FAILURE_CODE;
                }

                using (var help = context.GetExportBatchHelperModel("[EXPORT].[usp_ARxChangeFile_sl]"))
                {
                    var ds = manager.Execute(help);
                    var chk = context.GetLastCheckPoint();
                    bool checkAlreadyRegistered = (chk != null); //Only check if we've already registered the child JobExecution if a checkpoint exists.
                    for (int index = 0; index < ds.Tables[0].Rows.Count; index++)
                    {
                        string childfilepath = Path.Combine(FilePath, index.ToString());
                        Utility.ExportChildExecutionInfo child = new Utility.ExportChildExecutionInfo(context, childfilepath, EXPORT_TYPE)
                        {
                            ProjectID = (short)ds.Tables[0].Rows[index]["ProjectID"],
                            OrganizationID = (int)ds.Tables[0].Rows[index]["OrganizationID"]
                        };
                        if (checkAlreadyRegistered && chk.CheckPointNumber >= child.OrganizationID)
                        {
                            if (child.ProjectID != null && !string.IsNullOrEmpty(chk.CheckPointKey))
                            {
                                int proj = int.Parse(chk.CheckPointKey);
                                if (proj >= child.ProjectID.Value)
                                    continue; //have already created a child execution for this org/project
                            }
                            else
                                continue; //have already created a child execution for this org (no Project)
                        }
                        child.FilePath = FilePath = Path.Combine(context.Settings.ArchiveLocation, $"{child.ProjectID}_CYM_Reconciliation_{child.ProcessingDate:yyyy_MM_dd}.csv");
                        if (File.Exists(FilePath))
                        {
                            //throw new InvalidOperationException("File with this projectID already got generated for today!!");
                            context.LogError($"File with this ProjectID {child.ProjectID} already got generated for today!! Continuing");
                            continue;
                        }
                        checkAlreadyRegistered = false;
                        var results = context.RegisterChildExecutionWithExport(child);
                        if (results == null)
                        {
                            context.LogError($"Unable to register LoadBatch/ExportBatch for OrganizationID {child.OrganizationID}, ProjectID {child.ProjectID}. Continuing");
                            return DEFAULT_FAILURE_CODE;
                        }

                        chk = context.LogCheckPoint(Convert.ToInt32(child.OrganizationID), "Registered Child JobExecution for Organization", child.ProjectID?.ToString());
                    }
                }

                return DEFAULT_COMPLETE;
            }
            else
            {
                //Export batch was created earlier.
                batch = GetExportBatch(context, EXPORT_TYPE);
                if (batch.OutputFilePath == null)
                {
                    FilePath = context.Execution.FilePath;
                }
                else
                {
                    FilePath = batch.OutputFilePath;
                }
                context.Execution.METRIX_ExportBatchID = batch.ExportBatchID;
                if (CheckStage(context) < ExportBatchStage.DATA_PREP)
                {
                    using (var help = context.GetExportBatchHelperModel("[EXPORT].[usp_SEIDR_DataPrep_ARxChange]"))
                    {
                        help[EXPORT_BATCH_ID_PARAMETER] = context.Execution.METRIX_ExportBatchID;
                        help[nameof(batch.ProjectID)] = batch.ProjectID;
                        manager.IncreaseCommandTimeOut(300);
                        manager.Execute(help);
                    }
                    SetCheckPoint_DataPrep(context);
                }

                workingFile.OutputFilePath = batch.OutputFilePath = FilePath;
                using (var help = context.GetExportBatchHelperModel("[EXPORT].[usp_SEIDR_DataPull_ARxChange]"))
                {
                    help[nameof(context.ProcessingDate)] = context.Execution.ProcessingDate;
                    help[nameof(batch.ProjectID)] = batch.ProjectID;
                    var ds = manager.Execute(help);
                    batch.RecordCount = ds.Tables[0].Rows.Count;
                    if (batch.RecordCount == 0)
                    {
                        string filepath = null;
                        context.Execution.SetFileInfo(filepath);
                        batch.SetExportStatus(ExportStatusCode.ND);
                        batch.Active = false;
                        UpdateExportBatch(context, batch);
                        context.LogError("Returns empty records");
                        return NO_DATA_FAILURE_CODE;
                    }
                    var items = from dtable in ds.Tables[0].AsEnumerable()
                                select new ExportBatchARxChangeModel
                                {
                                    Metrix_Reference_Number = Convert.ToString(dtable["Metrix_Reference_Number"]),
                                    Total_Balance = dtable["Total_Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Total_Balance"]) as decimal?,
                                    Insurance_Balance = dtable["Insurance_Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Insurance_Balance"]) as decimal?,
                                    Patient_Balance = dtable["Patient_Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Patient_Balance"]) as decimal?,
                                    Primary_Insurance_Description = Convert.ToString(dtable["Primary_Insurance_Description"]),
                                    Total_PatientPayments = dtable["Total_PatientPayments"] is DBNull ? null : Convert.ToDecimal(dtable["Total_PatientPayments"]) as decimal?,
                                    Total_InsurancePayments = dtable["Total_InsurancePayments"] is DBNull ? null : Convert.ToDecimal(dtable["Total_InsurancePayments"]) as decimal?,
                                    Total_Adjustments = dtable["Total_Adjustments"] is DBNull ? null : Convert.ToDecimal(dtable["Total_Adjustments"]) as decimal?,
                                    Total_Charges = dtable["Total_Charges"] is DBNull ? null : Convert.ToDecimal(dtable["Total_Charges"]) as decimal?,
                                    ARxChange_Scored = dtable["ARxChange_Scored"] is DBNull ? null : Convert.ToByte(dtable["ARxChange_Scored"]) as byte?

                                };
                    
                    context.Execution.FilePath = FilePath;
                    workingFile.OutputFilePath = FilePath;
                    ARxChangeFile _ARxChangeFile = new ARxChangeFile();
                    //Create file
                    using (var writer = _ARxChangeFile.CreateFile(workingFile))
                    {
                        _ARxChangeFile.WriteExportFileHeader(writer);
                        foreach (var item in items)
                        {
                            _ARxChangeFile.WriteExportFileRow(writer, item);
                        }
                    }                    
                    var fi = new System.IO.FileInfo(workingFile);                    
                    workingFile.Finish();
                    UpdateExportBatch(context, batch);
                    return DEFAULT_RESULT;
                }
            }
        }
    }
}



