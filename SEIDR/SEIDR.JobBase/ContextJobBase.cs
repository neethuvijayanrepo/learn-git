using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    /// <summary>
    /// A partial Job Implementation which can serve as a base implementation.
    /// <para>May help to simplify set up for some more complex jobs by making use of inheritance.</para>
    /// <para>A shared implementation of Context can simplify development of similar jobs in the long run.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ContextJobBase<T>: IJob where T: BaseContext, new()
    {
        public virtual int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            T context = new T();
            context.Init(jobExecutor, execution);
            BasicLocalFileHelper.ClearWorkingDirectory(context);
            try
            {
                Process(context);
                if (context.RequeueRequested)
                    return false; //Requeue should not change state of anything. So not checking for working file/currentFilePath.

                if (context.ResultStatus == null) //Override provided status if the context result doesn't line up.
                {
                    context.SetStatus(context.Success);
                    if (context.ResultStatus == null)
                        throw new Exception("Unknown Result");
                }

                status = context.ResultStatus;
                if (context.WorkingFile != null && context.WorkingFile.Working)
                {
                    if (context.WorkingFile.Working)
                        context.WorkingFile.Finish();
                }

                if (context.CurrentFilePath != context.FilePath)
                {
                    context.Execution.SetFileInfo(context.WorkingFile);
                }

                return context.Success;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                BasicLocalFileHelper.ClearWorkingDirectory(context);
            }
        }

        public void FinalizeWorkingFile(T context)
        {
            if (context.WorkingFile != null)
            {
                context.WorkingFile.Finish();
            }
            else
                throw new InvalidOperationException("Unable to Finish Working File because there is no working file in the current context.");
        }
        /// <summary>
        /// Process the job using the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if successfully processed. Otherwise false.</returns>
        public abstract void Process(T context);
        /// <summary>
        /// Gets a basic local file helper
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public F GetLocalFile<F>(T context) where F: BasicLocalFileHelper, new()
        {
            return context.GetLocalFile<F>();
        }

        public ChildExecutionInfo GetNewChildInfo(T context, bool continueToNextStep = false)
        {
            return new ChildExecutionInfo(context.Execution, continueToNextStep);
        }
        /// <summary>
        /// Registers a child JobExecution as a new JobExecution, available to be picked up by the service.
        /// </summary>
        /// <param name="childExecutionInfo"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static JobExecution RegisterChildExecution(ChildExecutionInfo childExecutionInfo, T context)
        {
            if (string.IsNullOrEmpty(childExecutionInfo.Branch))
                childExecutionInfo.Branch = context.Execution.Branch;
            if (string.IsNullOrEmpty(childExecutionInfo.InitializationStatusCode))
                childExecutionInfo.InitializationStatusCode = ExecutionStatus.SPAWN;
            using (var help = context.Manager.GetBasicHelper())
            {
                help.QualifiedProcedure = "SEIDR.usp_JobExecution_i_ss";
                help.ParameterMap = childExecutionInfo;
                var je = context.Manager.SelectSingle<JobExecution>(help);
                context.LogInfo("Registered Child Execution - JobExecutionID " + je.JobExecutionID);
                return je;
            }
            
        }
    }
}
