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
    [IJobMetaData(nameof(StatementXMLGenerationJob), nameof(METRIX_EXPORT), "Generation of Statement XML Files for Metrix", false, true, DEFAULT_CONFIGURATION_TABLE)]
    public class StatementXMLGenerationJob : ExportJobBase
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
                    help[nameof(context.ProcessingDate)] = context.ProcessingDate;
                    help["@LetterVendor"] = context.Settings.VendorName;
                    help["@IncludeFacility"] = false; //Facility is currently necessary for letterInstance generation - not actually for creating the output file, though
                    help.QualifiedProcedure = "EXPORT.usp_ProjectFacilityInfo_sl_Statements";
                    var projFacilityList = db.SelectList<ProjectFacilityInfo>(help);
                    foreach (var projFacility in projFacilityList)
                    {
                        var ci = new ExportChildExecutionInfo(context, context.Settings.ExportType)
                        {
                            Branch = "EXPORT",
                            ProjectID = projFacility.ProjectID,
                            //UserKeyOverride = projFacility.FacilityID
                            ////Because we're not actually calling the letter generation, we do not need to be at facility level here.
                        };
                        context.RegisterChildExecutionWithExport(ci); //Not handling errors, so could potentially need manual cleanup if there's an issue while creating the ExportBatches/JobExecutions
                    }
                }
                context.SkipSuccessNotification = true;
                return DEFAULT_COMPLETE;
            }

            var eb = GetExportBatch(context, context.Settings.ExportType);
            //Check if has previously completed (leave exportBatch record alone if so)
            bool testMode = eb.ExportBatchStatusCode.In(ExportStatusCode.C, ExportStatusCode.SC); 


            XmlDocument document = new XmlDocument();

            using (var conn = db.GetConnection())
            using (var cmd = new SqlCommand("EXPORT.usp_LetterInstance_Export_SEIDR", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 180;
                cmd.Parameters.AddWithValue("@ProjectID", eb.ProjectID);
                cmd.Parameters.AddWithValue("@FacilityID", (object) eb.FacilityID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ExportBatchID", eb.ExportBatchID);
                cmd.Parameters.AddWithValue("@UID", 1);
                using (var xmlr = cmd.ExecuteXmlReader())
                    document.Load(xmlr);
            }
             
            XmlNodeList elemList = document.GetElementsByTagName("Letter");
            int letterCount = elemList.Count;
            eb.RecordCount = letterCount;
            if (letterCount == 0)
            {
                eb.SetExportStatus(ExportStatusCode.ND);
                if (!testMode)
                    UpdateExportBatch(context, eb);
                return ResultStatusCode.ND; //Done, no more to do.
            }

            workingFile.OutputDirectory = context.Settings.ArchiveLocation;
            workingFile.OutputFileName = $"Project{eb.ProjectID}_{letterCount}_{DateTime.Now:yyyyMMdd_HH_mm_ss}.xml";
            document.Save(workingFile);
            workingFile.Finish(); // Move file to output location and update JobExecution
            eb.OutputFilePath = context.CurrentFilePath;
            eb.SetExportStatus(ExportStatusCode.SI);
            if (!testMode)
                UpdateExportBatch(context, eb);
            return DEFAULT_RESULT;
        }
    }
}
