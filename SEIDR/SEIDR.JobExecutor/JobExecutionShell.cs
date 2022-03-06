using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobExecutor
{
    class JobExecutionShell : IJobExecution
    {
        public JobExecutionShell(int JobProfileID, int? JobProfile_JobID = null, long? JobExecutionID = null)
        {
            this.JobProfileID = JobProfileID;
            this.JobProfile_JobID = JobProfile_JobID ?? 0;
            this.JobExecutionID = JobExecutionID;
        }
        public int JobProfileID { get; private set; }
        public int JobProfile_JobID { get; private set; }
        public long? JobExecutionID { get; private set; }

        public string SourceFile { get; set; } = null;
        public string Directory { get; set; } = null;
        public string ExtraMessage { get; set;  } = null;
    }
}
