using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobExecutor
{
    public class JobExecutionDetail : JobExecution, IJobExecution
    {
        public string KeyInformation
        {
            get
            {
                StringBuilder result = new StringBuilder();
                if (OrganizationID != 0)
                    result.AppendLine($"Organization ({OrganizationID}): '{Organization}'");
                if (ProjectID != null)
                    result.AppendLine($"Project ({ProjectID}): '{Project}'");
                result.Append("UserKey: '" + UserKey1 + "'");
                if (UserKey2 != null)
                    result.AppendFormat("({0})", UserKey2); //Free Form sub key
                return result.ToString();
            }
        }

        public JobProfile ExecutionJobProfile;
        /// <summary>
        /// Used with JobNameSpace to find the IJob to be used for execution
        /// </summary>
        public string JobName { get; private set; }
        public string JobNameSpace { get; private set; }
        /// <summary>
        /// Used for single threaded logic
        /// </summary>
        public string JobThreadName { get; private set; }
        /// <summary>
        /// Determine if we need to check for other Executor threads running the same job
        /// </summary>
        public bool JobSingleThreaded { get; private set; }
        /// <summary>
        /// Total job completion notification.
        /// </summary>
        public string SuccessNotificationMail { get; private set; }
        /// <summary>
        /// Step failure notification.
        /// </summary>
        public string FailureNotificationMail { get; private set; }
        /// <summary>
        /// Work Queue sorting. Determined in view by database based on: 
        /// <para>* the amount of time since it was last queued/worked on,</para><para> 
        /// * the Profile's Priority, the Execution's priority,</para><para> 
        /// * the Procesing age (older processingDates get a slight boost to priority, future processing dates get a slightly lowered priority) 
        /// </para></summary>
        public int WorkPriority { get; private set; } = 1;

        public int RetryCount { get; private set; } = 0;
        /// <summary>
        /// Ignore schedule or other 'InSequence' conditions that could prevent picking up a JobExecution
        /// </summary>
        public bool ForceSequence { get; private set; }
        /// <summary>
        /// Used for JobQueue (CanStart), either after failing a step or due to a job requesting to re-queue
        /// </summary>
        public DateTime? DelayStart { get; set; }

        /// <summary>
        /// Computed based on DelayStart - if DelayStart is null, then it can start at any time. 
		/// Otherwise, wait until DateTime.Now reaches DelayStart and then set DelayStart to null
        /// </summary>
        public bool CanStart
        {
            get
            {
                if (DelayStart == null)
                    return true;
                if (DelayStart < DateTime.Now)
                {
                    DelayStart = null;
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Based on settings in Database - requeue with delay if Job returns false
        /// </summary>
        public bool CanRetry { get; set; }
        /// <summary>
        /// How long to wait before a JobExecution can be retried after failure
        /// </summary>
        public int RetryDelay { get; set; }

		
        public bool Complete { get; set; } = false;

        public bool ThreadChecked { get; set; } = false;
        #region Performance monitoring, statistics
        /// <summary>
        /// Performance monitoring - number of seconds between start and finish
        /// </summary>
        public int? ExecutionTimeSeconds { get; set; } = null;
        DateTime? ExecutionStart = null;
        DateTime? LastCheckPoint = null;
        public void Start()
        {
            ExecutionStart = DateTime.Now;
            ExecutionTimeSeconds = null;
            LastCheckPoint = DateTime.Now;
        }

        public int? GetExecutionTimeSeconds()
        {
            return (ExecutionStart.HasValue ? (int?)(DateTime.Now - ExecutionStart.Value).TotalSeconds : null);
        }

        public void Finish()
        {
            ExecutionTimeSeconds = GetExecutionTimeSeconds();
            ExecutionStart = null;
            LastCheckPoint = null;
        }
        public bool Started => ExecutionStart.HasValue && !ExecutionTimeSeconds.HasValue;
        public int CheckPoint()
        {
            if (!LastCheckPoint.HasValue)
                throw new InvalidOperationException("Attempted to checkpoint, but Execution has not been marked as 'started'");
            DateTime now = DateTime.Now;
            int checkpointDuration = (int)now.Subtract(LastCheckPoint.Value).TotalSeconds;
            LastCheckPoint = now;
            return checkpointDuration;
        }
        /// <summary>
        /// The time that the ExecutionDetail object was created (data pulled from DB).
        /// <para>Note that the detail is refreshed at the beginning of work.</para>
        /// </summary>
        public readonly DateTime DetailCreated = DateTime.Now;
        #endregion
    }
}
