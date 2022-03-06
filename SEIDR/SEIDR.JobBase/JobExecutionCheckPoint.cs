using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    /// <summary>
    /// Helper class managed by the implementation of <see cref="IJobExecutor.LogCheckPoint(int, string, string)"/>
    /// and <see cref="IJobExecutor.GetLastCheckPoint"/>.
    /// </summary>
    public class JobExecutionCheckPoint
    {
        public JobExecutionCheckPoint() { }
        public JobExecutionCheckPoint(JobExecution job)
        {
            JobExecutionID = job.JobExecutionID.Value;
            JobProfile_JobID = job.JobProfile_JobID;
            JobID = job.JobID;
        }
		/// <summary>
        /// Database Key
        /// </summary>
        public int? CheckPointID { get; private set; }
        public long JobExecutionID { get; private set; }
        public int JobProfile_JobID { get; private set; }
        public int JobID { get; private set; }
        /// <summary>
        /// Determined by Job, when a bit more information is needed beyond the CheckPointNumber. Max length in DB: 10
        /// </summary>
        public string CheckPointKey { get; set; }
		/// <summary>
        /// Determined by Job, should indicate where in the process the job was, in case it may be able to recover.
        /// </summary>
        public int CheckPointNumber { get; set; }
        /// <summary>
        /// User friendly description of the CheckPoint
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// JobExecutor Thread that was running the job when the checkpoint was created.
        /// </summary>
        public int ThreadID {get;set;}
        /// <summary>
        /// The amount of time, in seconds, spent working since the previous checkpoint (or execution start) until this checkpoint was created.
        /// </summary>
	    public int CheckPointDuration { get; set; }
        public DateTime DC { get; private set; } = DateTime.Now;
    }
}
