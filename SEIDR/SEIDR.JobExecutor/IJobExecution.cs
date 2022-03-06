using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobExecutor
{
    public interface IJobExecution
    {
        int JobProfileID {get;}
        int JobProfile_JobID { get; }
        long? JobExecutionID { get; }
    }
}
