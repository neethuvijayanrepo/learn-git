using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    /// <summary>
    /// Job Meta data. Can be single or multi threaded
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IJobMetaDataAttribute: ExportAttribute, IJobMetaData
    {
        string _Description = null;
        string _JobName;
        string _NameSpace;
        bool _ThreadCheck = true;
        bool _SafeCancel = false;
        bool _SingleThread = false;
        string _ThreadName = null;
        int _NotificationTime = 10;
        string _ConfigurationTable = null;
        bool _AllowRetry = false;
        int? _DefaultRetryTime = null;
        bool _NeedsFilePath = true;
        /// <summary>
        /// MetaData used to tell the Windows service what this job is for. Used to populate the SEIDR.Job table.
        /// </summary>
        /// <param name="JobName">Combine with NameSpace to give a unique identifier for the job.</param>
        /// <param name="NameSpace">A grouping for the job. Should usually just match the project.</param>
        /// <param name="Description">A description of the job to give the team an idea of what the job does.</param>
        /// <param name="NeedsFilePath">Indicate that the job requires the FilePath of the JobExecution to be populated to work correctly.</param>
        /// <param name="AllowRetry">Indicates that the team is allowed to configure this job to automatically try running the step again.
        /// <para>Most new jobs probably will not actually need this, but it doesn't hurt to set the value to true.</para>
        /// <para>Would suggest setting to false if a failure of the job indicates that manual intervention will likely be needed.</para></param>
        /// <param name="ConfigurationTable">Schema + Table name of what table to look for in the database to configure settings.</param>
        /// <param name="RerunThreadCheck"></param>
        /// <param name="SafeCancel"></param>
        /// <param name="SingleThreaded">Indicates that the job should not be allowed to call from multiple threads at the same time.</param>
        /// <param name="ThreadName"></param>
        /// <param name="NotificationTime">Number of minutes the job is allowed to run before the service sends out a long running notification mail to the team.</param>
        /// <param name="DefaultRetryTime">Default number of minutes to use for RetryDelay when AllowRetry is set to true.</param>
        public IJobMetaDataAttribute(string JobName, string NameSpace, string Description,
            bool NeedsFilePath, bool AllowRetry, string ConfigurationTable,
            bool RerunThreadCheck = true, bool SafeCancel = false, bool SingleThreaded = false, 
            string ThreadName = null, int NotificationTime = 10, int DefaultRetryTime = 5) 
            :base(typeof(IJob))
        {
            if (string.IsNullOrWhiteSpace(JobName))
                throw new ArgumentException(nameof(JobName) + " must be populated.", nameof(JobName));
            if (string.IsNullOrWhiteSpace(NameSpace))
                throw new ArgumentException(nameof(NameSpace) + " must be populated.", nameof(NameSpace));
            if (string.IsNullOrWhiteSpace(Description))
                throw new ArgumentException(nameof(Description) + " must be populated.", nameof(Description));
            

            _JobName = JobName;
            _Description = Description;
            _NameSpace = NameSpace;
            _ThreadCheck = RerunThreadCheck;
            _SafeCancel = SafeCancel;
            _SingleThread = SingleThreaded;
            _ThreadName = ThreadName;
            _NotificationTime = NotificationTime;
            _ConfigurationTable = ConfigurationTable;
            //Majority of new jobs are probably not going to be allow retry.
            _AllowRetry = AllowRetry;
            _DefaultRetryTime = DefaultRetryTime;
            //Majority of new jobs are going to need a specific file as input.
            _NeedsFilePath = NeedsFilePath;
        }
        public string Description => _Description;

        public string JobName => _JobName;

        public string NameSpace => _NameSpace;

        public bool RerunThreadCheck => _ThreadCheck;
        public bool SafeCancel => _SafeCancel;

        public bool SingleThreaded => _SingleThread;

        public string ThreadName => _ThreadName;

        public int NotificationTime => _NotificationTime;

        public string ConfigurationTable => _ConfigurationTable;

        public bool AllowRetry => _AllowRetry;

        public int? DefaultRetryTime => _DefaultRetryTime;

        public bool NeedsFilePath => _NeedsFilePath;
    }
    
    public interface IJobMetaData
    {
        string JobName { get; }
        [DefaultValue(null)]
        string Description { get; }
        /// <summary>
        /// Limit 128 characters. Should be able to keep your job unique, as well as isolate your statuses and some other special handling.
        /// Possible example: Need specific handling for different vendors with the same file. You might use the same JobName for sorting purposes, but keep them unique via NameSpace
        /// </summary>
        string NameSpace { get; }
        /// <summary>
        /// If a Job cannot share the same thread as other jobs, it should share a name. 
        /// <para>When the job is picked up, the Executor thread will take on the name from the current job if this is specified.</para>
        /// <para>If a job is queued and then ready while another thread is already running with this threadName, the jobExecution will either be held or moved to the other thread's queue.</para>
        /// </summary>
        [DefaultValue(null)]
        string ThreadName { get; }
        /// <summary>
        /// Indicates if the job needs to be run on a single thread. <para>
        /// E.g., it needs to store a complex state outside of local variables in the Execute method.</para>
        /// </summary>
        [DefaultValue(false)]
        bool SingleThreaded { get; }
        /// <summary>
        /// The job is able to call <see cref="IJobExecutor.checkAcknowledgeCancel"/> and stops if requested.
        /// </summary>
        [DefaultValue(false)]
        bool SafeCancel { get; }
        /// <summary>
        /// Indicates whether the job needs to rerun the thread check
        /// </summary>
        [DefaultValue(false)]
        bool RerunThreadCheck { get; }
        /// <summary>
        /// Indicates amount of minutes to notify about long running.
        /// </summary>
        [DefaultValue(10)]
        int NotificationTime { get; }
        /// <summary>
        /// Allow ease of documenting where else a Job needs to be configured.
        /// </summary>
        [DefaultValue(null)]
        string ConfigurationTable { get; }
        /// <summary>
        ///  Eventually use as a default value for the CanRetry on the JobProfile_Job table (GUI/ stored procedures).
        /// </summary>
        [DefaultValue(true)]
        bool AllowRetry { get; }
        /// <summary>
        /// Default retry time.
        /// </summary>
        [DefaultValue(5)]
        int? DefaultRetryTime { get; }
        /// <summary>
        /// This should be helpful for people to look at when deciding how to configure a step. 
        /// If 0, the person configuring will need to set up a Source Path in the job's configuration table, so this is mainly informational. (May be useful for validations in a GUI).
        /// </summary>
        [DefaultValue(false)]
        bool NeedsFilePath { get; }
    }
    public interface IJob
    {
        /// <summary>
        /// Check if the job is okay to run for the specified thread, or if it should be moved to another thread. 
        /// </summary>
        /// <param name="jobCheck">The job to check. E.g., if the job has to determine thread based on user keys or the profile in order to avoid stepping on other processes.</param>
        /// <param name="passedThreadID">The thread attempting to run. If it's okay to use, then PassedThreadID should just be returned</param>
        /// <param name="NewThreadID"></param>
        /// <returns>Thread that should be used<para>   
        /// </para></returns>
        int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor);
        /// <summary>
        /// Called by the jobExecutor.
        /// </summary>
        /// <param name="execution"></param>        
        /// <param name="status">Optional status set, to allow a more detailed status. If the status does not have a namespace set, the NameSpace from the job meta data will be used.</param>        
        /// <returns>True for success, false for failure.</returns>
        bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status);

    }
}
