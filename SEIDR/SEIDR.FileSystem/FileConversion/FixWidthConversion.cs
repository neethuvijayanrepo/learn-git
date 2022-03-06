using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.FileConversion
{
    [IJobMetaData(nameof(FixWidthConversionJob), nameof(SEIDR.FileSystem), "Convert Files from a fixed width report to a delimited output based a settings file.",
        ConfigurationTable: "SEIDR.JobProfile_Job_SettingsFile", AllowRetry: false, NeedsFilePath: true)]
    public class FixWidthConversionJob : IJob
    {
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        public void Process(JobProfile_Job_SettingsFile settingsInfo, JobExecution je, IJobExecutor caller)
        {
            FixWidthConverter fwc = FixWidthConverter.construct(je.FilePath, settingsInfo.SettingsFilePath);
            
            caller.LogInfo("Loaded settings from '" + settingsInfo.SettingsFilePath + "' for converting file '" + je.FilePath + "' to '" + fwc.OutputFilePath + "'");
            fwc.ConvertFile();
            je.SetFileInfo(fwc.OutputFilePath);
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            var f = JobProfile_Job_SettingsFile.GetRecord(jobExecutor.Manager, execution.JobProfile_JobID);
            Process(f, execution, jobExecutor);
            return true;            
        }
    }
}
