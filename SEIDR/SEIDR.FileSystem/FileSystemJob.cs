using SEIDR.JobBase;
using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;

namespace SEIDR.FileSystem
{
    [IJobMetaData(JobName: nameof(FileSystemJob), NameSpace: 
        JOB_NAMESPACE, Description: 
        "File and Directory Management", 
        ConfigurationTable: "SEIDR.FileSystemJob",
        NeedsFilePath: false,
        AllowRetry:true)]
    public class FileSystemJob: ContextJobBase<FileSystemContext>
    {
        const string JOB_NAMESPACE = nameof(FileSystem);
        const string GET_EXECUTION_INFO = "SEIDR.usp_FileSystem_ss_JobExecution";
        public override void Process(FileSystemContext context)
        {
            var manager = context.Manager;
            using (var h = manager.GetBasicHelper())
            {
                h.QualifiedProcedure = GET_EXECUTION_INFO;
                h["JobProfile_JobID"] = context.JobProfile_JobID;
                var fs = manager.SelectSingle<FS>(h);
                if (fs == null)
                {
                    context.LogError
                        (
                         "No Configuration",
                         new InvalidOperationException("FileSystemJob is not configured for JobProfile_JobID " + context.JobProfile_JobID)
                        );
                    context.SetStatus(false);
                    return;
                }
                try
                {
                    fs.Process(context); 
                    // Note: don't need to set a default value because the base class will check for
                    // the value of _success if a Result Status has not already been set
                }
                catch (IOException iex)
                {
                    if (!string.IsNullOrEmpty(fs.Source))
                        context.LogInfo("SOURCE:" + fs.Source);
                    if (!string.IsNullOrEmpty(fs.OutputPath))
                        context.LogInfo("OUTPUT PATH:" + fs.OutputPath);

                    context.LogError("File Processing Exception", iex);
                    context.SetStatus(ResultStatusCode.IO);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    context.LogError(ex.Message, ex);
                    context.Requeue(10);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(fs.Source))
                        context.LogInfo("SOURCE:" + fs.Source);
                    if (!string.IsNullOrEmpty(fs.OutputPath))
                        context.LogInfo("OUTPUT PATH:" + fs.OutputPath);
                    context.LogError("File Processing Exception", ex);
                    context.SetStatus(false);
                }
            }
        }
    }
}
