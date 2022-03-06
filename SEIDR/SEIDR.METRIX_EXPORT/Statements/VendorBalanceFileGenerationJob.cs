using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT.Utility;

namespace SEIDR.METRIX_EXPORT.Statements
{
    [IJobMetaData(nameof(VendorBalanceFileGenerationJob), nameof(METRIX_EXPORT), "Account Balance export for vendor", false, true, DEFAULT_CONFIGURATION_TABLE)]
    public class VendorBalanceFileGenerationJob : ExportJobBase
    { 
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            if (context.ProcessingDate < DateTime.Today)
                return ResultStatusCode.OD;
            var db = context.MetrixManager;
            if (context.ExportBatchID == 0)
            {
                using (var help = db.GetBasicHelper())
                {
                    help["@VendorName"] = context.Settings.VendorName;
                    help.QualifiedProcedure = "EXPORT.usp_ProjectInfo_sl_VendorBalance";
                    var projList = db.SelectList<ProjectFacilityInfo>(help);
                    foreach (var proj in projList)
                    {
                        var ci = new ExportChildExecutionInfo(context, context.Settings.ExportType)
                        {
                            Branch = "EXPORT",
                            ProjectID = proj.ProjectID,
                        };
                        context.RegisterChildExecutionWithExport(ci);
                    }
                }
                context.SkipSuccessNotification = true;
                return DEFAULT_COMPLETE;
            }

            var eb = GetExportBatch(context, context.Settings.ExportType);
            if (eb != null)
            {
                string FilePath = $"{context.Settings.ArchiveLocation}\\PatientCo_VendorBalanceFile{eb.ProjectID}_{context.Execution.ProcessingDate:yyyy_MM_dd}.csv";
                //Check if has previously completed (leave exportBatch record alone if so)
                bool testMode = eb.ExportBatchStatusCode.In(ExportStatusCode.C, ExportStatusCode.SC);
                //////////////////////
                using (var help = db.GetBasicHelper())
                {
                    help.QualifiedProcedure = "EXPORT.usp_VendorBalanceFile_Export_SEIDR";

                    help[nameof(context.Execution.ProjectID)] = eb.ProjectID;
                    help[nameof(context.ProcessingDate)] = context.ProcessingDate;
                    List<VendorBalanceModel> lstRecords = db.SelectList<VendorBalanceModel>(help);

                    if (lstRecords?.Count > 0)
                    {
                        context.Execution.FilePath = FilePath;
                        workingFile.OutputFilePath = FilePath;
                        DelimitedFileHelper _docWriter = new DelimitedFileHelper('|');
                        //Create file
                        using (var writer = _docWriter.CreateFile(workingFile))
                        {
                            _docWriter.WriteExportFileHeader(writer, new VendorBalanceModel());
                            _docWriter.WriteExportFileRow(writer, lstRecords);
                        }
                        var fi = new System.IO.FileInfo(workingFile);
                        workingFile.Finish();
                        eb.SetExportStatus(ExportStatusCode.SI);
                        if (!testMode)
                            UpdateExportBatch(context, eb);
                    }
                    else
                    {
                        eb.SetExportStatus(ExportStatusCode.ND);
                        if (!testMode)
                            UpdateExportBatch(context, eb);
                        return ResultStatusCode.ND;
                    }

                }
            }
            return DEFAULT_RESULT;
        }
    }
}
