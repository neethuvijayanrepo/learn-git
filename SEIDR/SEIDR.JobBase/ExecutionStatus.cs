using System;
using SEIDR.DataBase;
using SEIDR.META;
using System.ComponentModel;

namespace SEIDR.JobBase
{
    
   
   
    public class ExecutionStatus
    {                
        public string ExecutionStatusCode { get; set; }
        /// <summary>
        /// Indicates if the execution is complete. Default: false
        /// </summary>
        public bool IsComplete { get; set; } = false;
        /// <summary>
        /// Indicates if the execution is at an error status. Default: false. 
        /// <para>Note: If JobProfile_Job is set to be able to retry, this status will be logged, but won't update the JobExecution's status</para>
        /// </summary>
        public bool IsError { get; set; } = false;
        /// <summary>
        /// Used to populate ExecutionStatus table when first added. Should be descriptive for users.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Specify for non default to prevent overlap. If not set, the Namespace from JobMetaData will be used.
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// Status allows being picked for queueing.
        /// </summary>
        public bool Queueable => !IsComplete && !IsError;
        /// <summary>
        /// Code-only property - Indicates that even if the status indicates <see cref="IsComplete"/>=true,  a success notification mail should not be sent.
        /// <para>Example usage: a vendor export job runs in two modes - the first mode identifies projects that need to be exported and creates follow up job executions for each. It then marks itself as Complete, but has not really done anything meaningful to warrant a completion notification.</para>
        /// </summary>
        public bool SkipSuccessNotification { get; set; } = false;

        public override string ToString()
        {
            return $"[{(NameSpace ?? "SEIDR")}].[{ExecutionStatusCode}]"
                + (string.IsNullOrWhiteSpace(Description) ? string.Empty : " - " + Description)
                + (IsError ? " (ERROR)": string.Empty);
        }

        public const string REGISTERED = "R";
        public const string SCHEDULED = "S";
        public const string MANUAL = "M";
        public const string COMPLETE = "C";
        public const string CANCELLED = "CX";
        public const string FAILURE = "F";
        public const string STEP_COMPLETE = "SC";
        public const string SPAWN = "SP";
        public const string INVALID = "X";
        /// <summary>
        /// A complete status that does not allow continuing even with a trigger execution status.
        /// <para>Primarily used when running out of retries on a failure trigger.</para>
        /// </summary>
        public const string FAILURE_STOP = "FF";
    }
}
