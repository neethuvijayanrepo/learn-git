using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.FileValidation
{
    public class SizeCheckModel
    {
        public SizeCheckModel(JobBase.JobExecution execution)
        {
            JobExecutionID = execution.JobExecutionID.Value;
            JobProfile_JobID = execution.JobProfile_JobID;
            FileSize = execution.FileSize.Value;
            ProcessingDate = execution.ProcessingDate;
            FilePath = execution.FilePath;

        }
        public long JobExecutionID { get; }
        public int JobProfile_JobID { get; }
        public long FileSize { get; }
        public string FilePath { get; }
        public DateTime ProcessingDate { get; }
        public bool AllowContinue { get; set; } = false;
        public long Deviation { get; set; } = 0;
        public string Message { get; set; }
        public bool Empty { get; set; }
        public bool LargeFile { get; set; }
    }
}
