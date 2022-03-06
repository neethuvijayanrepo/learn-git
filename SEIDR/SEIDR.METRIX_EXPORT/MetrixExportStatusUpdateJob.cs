using SEIDR.DataBase;
using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT
{
    [IJobMetaData(nameof(MetrixExportStatusUpdateJob), 
		nameof(METRIX_EXPORT), 
		"Metrix Export Status - ExportBatchStatusCode Update", 
        NotificationTime: 2, ConfigurationTable: null,
        NeedsFilePath: false, AllowRetry:true)]
    public class MetrixExportStatusUpdateJob : ExportJobBase
    {
        public const ExportStatusCode METRIX_EXPORT_SUCCESS = ExportStatusCode.SC;
        public const ExportStatusCode METRIX_EXPORT_FAILURE = ExportStatusCode.SF;

        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            ResultStatusCode result = context.IsError ? DEFAULT_COMPLETE : DEFAULT_RESULT;
            //If JobExecution was an error, we want to tell the Metrix ExportBatch table that the call has failed and will no longer re-try.
            //After that, SEIDR JobExecution is done (unless someone manually updates the JobExecution)

            UpdateExportBatchStatus(context.Executor, context.Execution);
            //Note: if status >= Success does not match up with whether or not the explicit status is an error,
            //will likely end up with your explicit status being ignored. (See SEIDR.usp_JobExecution_SetStatus - @Success versus IsError)
            return result;
        }
       
        /// <summary>
        /// Update ExportBatchStatusCode in EXPORT.ExportBatch table of Andromeda
        /// </summary>
        /// <param name="jobExecutor"></param>
        /// <param name="execution"></param>
        public void UpdateExportBatchStatus(IJobExecutor jobExecutor, JobExecution execution)
        {
            //1. Get connection of Andromeda DB from DatabaseLookup Table.
            var executionStatusCode = execution.IsError ? METRIX_EXPORT_FAILURE : METRIX_EXPORT_SUCCESS;
            var db = GetMetrixDatabaseManager(jobExecutor);
            using (var help = db.GetBasicHelper())
            {
                help.QualifiedProcedure = "EXPORT.usp_SEIDR_ExportBatch_Status_U";
                help["ExportBatchID"] = execution.METRIX_ExportBatchID;
                help["SEIDR_JobExecutionID"] = execution.JobExecutionID;
                help["ExportBatchStatusCode"] = executionStatusCode;
                db.Execute(help);
            }
        }
    }
}
