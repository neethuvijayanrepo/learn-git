using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.Scripting
{
    [IJobMetaData(JobName: nameof(BatchScriptJob), NameSpace: 
        nameof(SEIDR.FileSystem.Scripting), Description: "Execute Batch Scripts",
        ConfigurationTable: "SEIDR.BatchScriptJob",
        NeedsFilePath: false, AllowRetry:true)]
    public class BatchScriptJob : IJob
    {
        const string ParameterSet = "\"{0}\" {1} \"{2}\" \"{3}\"";
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        public bool Call(BatchScriptJobConfiguration config, IJobExecutor executor, ref ExecutionStatus result)
        {
            var f = new FileInfo(config.BatchScriptPath);
            var di = f.Directory;
            if (!f.Exists)
            {
                executor.LogError("Invalid script path: " + f.FullName);
                result.ExecutionStatusCode = "NS";
                result.Description = "No Script";
                result.NameSpace = nameof(Scripting);
                result.IsError = true;
                return false;
            }

            
            ProcessStartInfo procInfo = new ProcessStartInfo(f.FullName, config.Args);

            procInfo.UseShellExecute = false;
            procInfo.WorkingDirectory = di.FullName;

            procInfo.RedirectStandardError = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardInput = true;

            using (Process p = Process.Start(procInfo))
            {                
                if (p != null)
                {
                    string Error = p.StandardError.ReadToEnd();
                    p.WaitForExit(1000 * 60 * 10); // 10 minute max wait.
                    if (!string.IsNullOrWhiteSpace(Error))
                        executor.LogError(Error);
                    return p.ExitCode == 0;
                }
                else
                {
                    result.ExecutionStatusCode = "NP";
                    result.Description = "No Process Started";
                    result.NameSpace = nameof(Scripting);
                    result.IsError = true;
                    return false;
                }
            }
        }
        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            var m = jobExecutor.Manager;
            BatchScriptJobConfiguration config;
            using (var help = m.GetBasicHelper())
            {
                help.Procedure = "usp_BatchScriptJob_ss";
                help[nameof(execution.JobProfile_JobID)] = execution.JobProfile_JobID;
                config = m.SelectSingle<BatchScriptJobConfiguration>(help);
                if(config == null)
                {
                    jobExecutor.LogInfo("No Valid Configuration");
                    return false;
                }
            }
            config.SetupArgs(execution);
            return Call(config, jobExecutor, ref status);            
        }
    }
}
