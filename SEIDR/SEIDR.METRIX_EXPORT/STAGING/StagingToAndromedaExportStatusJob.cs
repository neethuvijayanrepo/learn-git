using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using SEIDR.JobBase;

namespace SEIDR.METRIX_EXPORT.STAGING
{
    [IJobMetaData(nameof(StagingToAndromedaExportStatusJob), nameof(METRIX_EXPORT),
        "Check Metrix Export Status from Staging to Andromeda",
        NeedsFilePath: false,AllowRetry: true,
        ConfigurationTable: "SEIDR.StagingToAndromedaExportStatusJob")]
    public class StagingToAndromedaExportStatusJob : ContextJobBase<ExportContext>
    {
        public override void Process(ExportContext context)
        {
            StagingToAndromedaExportStatusJobSettings settings = context.Manager.SelectSingle<StagingToAndromedaExportStatusJobSettings>(context);
            if (settings.RequireCurrentProcessingDate && context.ProcessingDate < DateTime.Today)
            {
                context
                    .SetStatus(ResultStatusCode.OD)
                    .IsComplete = true;
                return;
            }
            settings.ProcessingDate = context.ProcessingDate;

            var batches = settings.LoadBatchTypeList.Split(new[]{';', ','}, StringSplitOptions.RemoveEmptyEntries);
            string BatchTypeList = string.Join(",", batches);
            var m = context.Executor.GetManager(settings.DatabaseLookupID);
            using (var help = m.GetBasicHelper())
            {
                help.QualifiedProcedure = "ETL.usp_CheckExportStatus";
                help.ParameterMap = settings;

                if (settings.CheckOrganization)
                    help[nameof(context.Execution.OrganizationID)] = context.Execution.OrganizationID;
                else
                    help[nameof(context.Execution.OrganizationID)] = DBNull.Value;
                if (settings.CheckProject)
                    help[nameof(context.Execution.ProjectID)] = context.Execution.ProjectID;
                else
                    help[nameof(context.Execution.ProjectID)] = DBNull.Value;

                help[nameof(BatchTypeList)] = BatchTypeList;
                List<LoadInfo> ds = m.SelectList<LoadInfo>(help);
                

                if (help.ReturnValue != 0 || ds?.Count > 0)
                {
                    if(settings.Message != null)
                        context.LogError(settings.Message);
                    foreach (var record in ds)
                    {
                        context.LogError($"LoadProfileID {record.LoadProfileID} (LoadType='{record.LoadBatchTypeCode}') - Last Export Completion was '{record.MinLastExportCompleteDate:MMMM dd, yyyy}' and is not ready for Processing Date of '{context.ProcessingDate:MMMM dd, yyyy}'");
                    }
                    context.SetStatus(ResultStatusCode.NE);
                }
                else if (settings.Message != null)
                {
                    context.LogInfo(settings.Message);
                }
            }
        }
    }
}