using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.METRIX_EXPORT.Utility
{
    public static class JobExecutionHelper
    {
        public const string JOB_EXECUTION_I_SS = "SEIDR.usp_JobExecution_i_ss";
        public const string EXPORT_BATCH_I_SEIDR = "EXPORT.usp_ExportBatch_i_SEIDR";
        public const string JOB_EXECUTION_U_BATCHID = "[SEIDR].[usp_JobExecution_u_ExportBatchID]";
        /// <summary>
        /// Registers a new, child job execution by using a ChildExecutionInfo model, created using the source JobExecution.
        /// <para>Will also create and link an ExportBatch</para>
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="childExecutionInfo"></param>
        /// <param name="metrix"></param>
        /// <returns></returns>
        public static Tuple<JobExecution, ExportBatchModel> RegisterChildExecution(this IJobExecutor caller, ExportChildExecutionInfo childExecutionInfo, DataBase.DatabaseManager metrix)
        {
            //ToDo: c#  7.0 language feature - value tuples: (JobExecution childExecution, ExportBatchModel childExport) RegisterChildExecution....
            using (var mtxHelper = metrix.GetBasicHelper())
            using (var help = caller.Manager.GetBasicHelper(true))
            {
                help.BeginTran();
                help.QualifiedProcedure = JOB_EXECUTION_I_SS;
                help.ParameterMap = childExecutionInfo;
                var ret = caller.Manager.SelectSingle<JobExecution>(help);

                mtxHelper.QualifiedProcedure = EXPORT_BATCH_I_SEIDR;
                mtxHelper[nameof(childExecutionInfo.ExportType)] = childExecutionInfo.ExportType;
                mtxHelper[nameof(childExecutionInfo.FacilityID)] = childExecutionInfo.FacilityID;
                mtxHelper.ParameterMap = ret;
                var exportBatch = metrix.SelectSingle<ExportBatchModel>(mtxHelper);
                if (exportBatch == null)
                {
                    help.RollbackTran();
                    return null;
                }

                ret.METRIX_ExportBatchID = exportBatch.ExportBatchID;

                help.QualifiedProcedure = JOB_EXECUTION_U_BATCHID;
                help.ParameterMap = ret;
                caller.Manager.Execute(help);
                help.CommitTran();
                return new Tuple<JobExecution, ExportBatchModel>(ret, exportBatch);
            }
        }
        /// <summary>
        /// Registers a new jobExecution, spawned by the source job execution.
        /// <para>ExecutionStatusCode will initialize as PD</para>
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="source"></param>
        /// <param name="childExecutionInfo"></param>
        /// <returns></returns>
        public static JobExecution RegisterChildExecution(this IJobExecutor caller, ExportChildExecutionInfo childExecutionInfo)
        {
            using (var help = caller.Manager.GetBasicHelper())
            {
                help.QualifiedProcedure = JOB_EXECUTION_I_SS;
                help.ParameterMap = childExecutionInfo;
                return caller.Manager.SelectSingle<JobExecution>(help);
            }
        }
    }
}
