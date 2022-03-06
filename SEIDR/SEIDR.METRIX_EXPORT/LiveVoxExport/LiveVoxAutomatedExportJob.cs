using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SEIDR.JobBase;
using System.IO;

namespace SEIDR.METRIX_EXPORT.LiveVoxExport
{
    [IJobMetaData(nameof(LiveVoxAutomatedExportJob), nameof(SEIDR.METRIX_EXPORT),
        "LiveVox Automated Export File Generation", NotificationTime: 10,
        ConfigurationTable: ExportJobBase.DEFAULT_CONFIGURATION_TABLE,
        NeedsFilePath: false, AllowRetry:true)]
    public class LiveVoxAutomatedExportJob : ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            var manager = context.MetrixManager;

            string filePath = Path.Combine(context.Settings.ArchiveLocation, "ContactsManager_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");

            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("File already got generated for today!!");
            }

            try
            {
                ExportBatchModel batch;
                const string EXPORT_TYPE = "LiveVox Campaign Export";
                if (CheckStage(context) < ExportBatchStage.SETUP) //Do not have an ExportBatchID yet. NOTE: If we need to clean up an ExportBatch, then we can also delete the checkpoints so that this starts over...
                {
                    batch = BeginExportBatch(context, EXPORT_TYPE);
                }
                else
                {
                    //Export batch was created earlier.
                    batch = GetExportBatch(context, EXPORT_TYPE);
                }

                context.Execution.METRIX_ExportBatchID = batch.ExportBatchID;
                if (CheckStage(context) < ExportBatchStage.DATA_PREP)
                {
                    using (var help = context.GetExportBatchHelperModel("[EXPORT].[usp_SEIDR_DataPrep_LiveVoxAutomation]"))
                    {
                        manager.IncreaseCommandTimeOut(600);
                        manager.Execute(help);
                    }

                    SetCheckPoint_DataPrep(context);
                }

                workingFile.OutputFilePath = batch.OutputFilePath = filePath;

                using (var help = context.GetExportBatchHelperModel("[EXPORT].[usp_SEIDR_DataPull_LiveVoxAutomation]"))
                {
                    var ds1 = manager.Execute(help);
                    var items = from dtable in ds1.Tables[0].AsEnumerable()
                                select new ExportBatchItem
                                {
                                    AccountID = Convert.ToString(dtable["AccountID"]),
                                    LastName = Convert.ToString(dtable["PatientLastName"]),
                                    FirstName = Convert.ToString(dtable["PatientFirstName"]),
                                    Address = ((Convert.ToString(dtable["PatientAddress1"]) ?? "") + " " + (Convert.ToString(dtable["PatientAddress2"]) ?? "")).Trim(),
                                    Balance = Convert.ToDecimal(dtable["PatientBalance_Client"]),
                                    City = Convert.ToString(dtable["PatientCity"]),
                                    State = Convert.ToString(dtable["PatientState"]),
                                    ZipCode = Convert.ToString(dtable["PatientZip"]),
                                    Phone1 = Convert.ToString(dtable["GuarantorPhoneNumber"]),
                                    Phone2 = Convert.ToString(dtable["PatientPhoneNumber"]),
                                    Phone3 = null,
                                    Phone4 = null,
                                    GuarantorFirstName = Convert.ToString(dtable["GuarantorFirstName"]),
                                    GuarantorLastName = Convert.ToString(dtable["GuarantorLastName"]),
                                    PatientBirthDate = dtable["PatientBirthDate"] is DBNull ? null : Convert.ToDateTime(dtable["PatientBirthDate"]) as DateTime?,
                                    PatientAge = dtable["PatientAge"] is DBNull ? null : Convert.ToInt32(dtable["PatientAge"]) as int?,
                                    AccountNumber = Convert.ToString(dtable["AccountNumber"]),

                                    AdmissionOrDischargeDate = dtable["AdmissionOrDischargeDate"] is DBNull ? null : Convert.ToDateTime(dtable["AdmissionOrDischargeDate"]) as DateTime?,
                                    ServiceDate = dtable["ServiceDate"] is DBNull ? null : Convert.ToDateTime(dtable["ServiceDate"]) as DateTime?,
                                    PatientType = Convert.ToString(dtable["PatientType"]),
                                    ServiceLocation = Convert.ToString(dtable["ServiceLocation"]),
                                    PhysicianName = Convert.ToString(dtable["PhysicianName"]),
                                    AttendingPhysicianName = Convert.ToString(dtable["AttendingPhysicianName"]),
                                    InsurancePacketBalance = dtable["InsurancePacketBalance"] is DBNull ? null : Convert.ToDecimal(dtable["InsurancePacketBalance"]) as decimal?,
                                    SelfPayPacketBalance = dtable["SelfPayPacketBalance"] is DBNull ? null : Convert.ToDecimal(dtable["SelfPayPacketBalance"]) as decimal?,
                                    TotalPacketBalance = dtable["TotalPacketBalance"] is DBNull ? null : Convert.ToDecimal(dtable["TotalPacketBalance"]) as decimal?,
                                    Insurance1Name = Convert.ToString(dtable["Insurance1Name"]),
                                    Insurance2Name = Convert.ToString(dtable["Insurance2Name"]),
                                    Insurance3Name = Convert.ToString(dtable["Insurance3Name"]),
                                    Insurance1Balance = dtable["Insurance1Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Insurance1Balance"]) as decimal?,
                                    Insurance2Balance = dtable["Insurance2Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Insurance2Balance"]) as decimal?,
                                    Insurance3Balance = dtable["Insurance3Balance"] is DBNull ? null : Convert.ToDecimal(dtable["Insurance3Balance"]) as decimal?,
                                    SelfPaySegmentCategoryID = dtable["SelfPaySegmentCategoryID"] is DBNull ? null : Convert.ToInt32(dtable["SelfPaySegmentCategoryID"]) as int?,
                                    SelfPayDeterminationDate = dtable["SelfPayDeterminationDate"] is DBNull ? null : Convert.ToDateTime(dtable["SelfPayDeterminationDate"]) as DateTime?,
                                    NumberofcallsConnected = dtable["NumberofcallsConnected"] is DBNull ? null : Convert.ToInt32(dtable["NumberofcallsConnected"]) as int?,
                                    DateLastMessageLeft = dtable["DateLastMessageLeft"] is DBNull ? null : Convert.ToDateTime(dtable["DateLastMessageLeft"]) as DateTime?,
                                    DateofLastConnectedCall = dtable["DateofLastConnectedCall"] is DBNull ? null : Convert.ToDateTime(dtable["DateofLastConnectedCall"]) as DateTime?,
                                    DaysSinceLastConnectedCall = dtable["DaysSinceLastConnectedCall"] is DBNull ? null : Convert.ToInt32(dtable["DaysSinceLastConnectedCall"]) as int?,
                                    LastPaymentDate = dtable["LastPaymentDate"] is DBNull ? null : Convert.ToDateTime(dtable["LastPaymentDate"]) as DateTime?,
                                    AccountStatusDescription = Convert.ToString(dtable["AccountStatusDescription"]),
                                    LastLetterInSeriesSent = dtable["LastLetterInSeriesSent"] is DBNull ? null : Convert.ToInt32(dtable["LastLetterInSeriesSent"]) as int?,
                                    LastLetterDate = dtable["LastLetterDate"] is DBNull ? null : Convert.ToDateTime(dtable["LastLetterDate"]) as DateTime?,
                                    ARXQuality = Convert.ToString(dtable["ARXQuality"]),
                                    EstimatedIncome = dtable["EstimatedIncome"] is DBNull ? null : Convert.ToInt32(dtable["EstimatedIncome"]) as int?,
                                    ProjectID = dtable["ProjectID"] is DBNull ? null : Convert.ToInt32(dtable["ProjectID"]) as int?
                                };

                    LiveVoxFile liveVoxFile = new LiveVoxFile();
                    using (var writer = liveVoxFile.CreateFile(workingFile))
                    {
                        liveVoxFile.WriteExportFileHeader(writer);
                        foreach (var item in items)
                        {
                            liveVoxFile.WriteExportFileRow(writer, item);
                        }
                    }

                    batch.RecordCount = ds1.Tables[0].Rows.Count;
                }

                UpdateExportBatch(context, batch);
                workingFile.Finish(); //Takes care of setting/refreshing FileInfo
                return DEFAULT_RESULT;
            }
            catch (Exception ex)
            {
                context.LogError("LiveVox automated Job Error", ex);
                return DEFAULT_FAILURE_CODE;
            }
        }
    }
}