using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.Composition;

namespace SEIDR.JobBase
{    
    public interface IJobExecutor
    {
        /// <summary>
        /// A readonly copy of the database connection being used by the Executor
        /// </summary>
        [Obsolete("See about just using " + nameof(Manager))]
        DataBase.DatabaseConnection connection { get; }
        /// <summary>
        /// DatabaseManager used by Executor
        /// </summary>
        DataBase.DatabaseManager Manager { get; }
        /// <summary>
        /// Gets a copy of the database connection by using the description of the lookup.
        /// <para>Will return null if the lookup is not found.</para>
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        DataBase.DatabaseConnection GetConnection(string description);
        /// <summary>
        /// Gets a copy of the database connection by using the ID of the database lookup.
        /// <para>Will return null if the lookup is not found.</para>
        /// </summary>
        /// <param name="LookupID"></param>
        /// <returns></returns>
        DataBase.DatabaseConnection GetConnection(int LookupID);
        /// <summary>
        /// Gets a database connection manager from the cache by using the description of the database lookup, and whether or not it should be readonly or readwrite access.
        /// <para>Will return null if the database lookup is not found.</para>
        /// </summary>
        /// <param name="description"></param>
        /// <param name="ReadOnly"></param>
        /// <returns>Database manager for executing procedure calls and queries</returns>
        DataBase.DatabaseManager GetManager(string description, bool ReadOnly = false);
        /// <summary>
        /// Gets a database connection manager from the cache by using the ID of the database lookup, and whether or not it should be readonly or readwrite access.
        /// <para>Will return null if the database lookup is not found.</para>
        /// </summary>
        /// <param name="LookupID"></param>
        /// <param name="ReadOnly"></param>
        /// <returns>Database manager for executing procedure calls and queries</returns>
        DataBase.DatabaseManager GetManager(int LookupID, bool ReadOnly = false);


        /// <summary>
        /// Profile of current execution
        /// </summary>
        JobProfile job { get; }
        /// <summary>
        /// The actual number ID of the executing thread, used by the Service
        /// </summary>
        int ThreadID { get; }
        /// <summary>
        /// Name of the thread, for logging purposes.
        /// </summary>
        string ThreadName { get; }
        /// <summary>
        /// If called, will move the current jobExecution to the end of the queue with a mark to retry in at least <paramref name="delayMinutes"/> minutes.
        /// <para>The execution should return false, or else the status will be updated instead of requeueing.</para>
        /// </summary>
        /// <param name="delayMinutes"></param>
        void Requeue(int delayMinutes);
        /// <summary>
        /// If called, will cause the executor thread to change its status (JOB_REQUESTED_SLEEP) and then sleep. 
        /// <para>After sleep ends, status is reverted and returns control to the job.
        /// </para>
        /// </summary>
        /// <param name="sleepSeconds">Number of seconds to cause thread to sleep. Included in thread status XML doc and other logs</param>
        /// <param name="logReason">Included in logs, and the thread status XML doc.</param>
        /// <returns></returns>
        void Wait(int sleepSeconds, string logReason);
        /// <summary>
        /// Logs the error to the Executor's file log and to the SEIDR.JobExecutionError Table
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void LogError(string message, Exception ex = null, int? extraID = null); //ToDo: default value of null for ex
        
        /// <summary>
        /// Logs the message to the Executor's file log.
        /// </summary>
        /// <param name="message"></param>
        void LogInfo(string message);
        /// <summary>
        /// Call when at a point where the job can stop if requested.
        /// <para>It should only be called when the job is at a point where it's safe to return when the method returns true.</para>
        /// </summary>
        /// <returns>True if the job has been requested to stop.<para>
        /// If this returns true, the job should return false if it is able to stop.</para></returns>
        bool checkAcknowledgeCancel();
        /// <summary>
        /// Logs a checkpoint to the database/logFile and returns the CheckPoint object so that any conditional logic based on CheckPoint step can be the same <para>regardless of whether the checkpoint was just logged, or if it's the checkpoint picked up at the beginning of execution.</para>
        /// </summary>
        /// <param name="CheckPointNumber"></param>
        /// <param name="Message">Message to store with CheckPoint. If null a default value will be provided.</param>
        /// <param name="Key"></param>
        JobExecutionCheckPoint LogCheckPoint(int CheckPointNumber, string Message = null, string Key = null);
        /// <summary>
        /// Gets the latest Checkpoint that was logged to the database. May be useful if you have multiple long-running steps, or simply steps that should definitely not be repeated (especially if going to allow auto-retry)
        /// </summary>
        /// <returns></returns>
        JobExecutionCheckPoint GetLastCheckPoint();
        /// <summary>
        /// Attempts to send a mail message using the configuration from the Windows Service. 
        /// </summary>
        /// <param name="message"></param>
        void SendMail(System.Net.Mail.MailMessage message);
    }
}
