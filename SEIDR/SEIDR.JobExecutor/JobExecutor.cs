using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using SEIDR.DataBase;
using SEIDR.JobBase;
using System.Threading;
using SEIDR.ThreadManaging;

namespace SEIDR.JobExecutor
{
    public sealed class JobExecutor : Executor, IJobExecutor
    {
        int BatchSize => CallerService.BatchSize;
        static JobLibrary Library { get; set; } = null;
        static DateTime LastLibraryCheck = new DateTime(1, 1, 1);

        volatile static List<ExecutionStatus> statusList = new List<ExecutionStatus>();
        public static void PopulateStatusList(DatabaseManager manager)
        {
            //maybe switch over to readWrite lock, to reduce overhead? 
            //Possible ToDo: Wrapper for readWrite, since it's kind of bulkier to set up and needs extra try/catches, especially if transitioning to/from a Write lock..
            using (new LockHelper(Lock.Exclusive, STATUS_TARGET)) 
            {
                statusList.Clear();
                statusList = manager.SelectList<ExecutionStatus>(Schema:"SEIDR");
            }
        }
        void CheckStatus(ExecutionStatus check)
        {            
            //lock(statusListLock)
            using(var h= new LockHelper(Lock.Shared, STATUS_TARGET))
            {
                if (statusList.Exists(s => s.NameSpace == check.NameSpace && s.ExecutionStatusCode == check.ExecutionStatusCode))
                    return;
                h.Transition(Lock.Exclusive);
                statusList.Add(check);
            }
            //format: SEIDR.usp_{0}_i
            _Manager.Insert(check);
        }
        const string LIBRARY_TARGET = nameof(SEIDR.JobExecutor) + "." + nameof(Library);
        LockManager libraryLock = new LockManager(LIBRARY_TARGET); //NOT static.
        const string STATUS_TARGET = nameof(SEIDR.JobExecutor) + "." + nameof(statusList);
        //static object statusListLock = new object();
        public override string InfoDescription
        {
            get
            {
                string result = LogName + "(" + ThreadName + ")";
                if (CurrentExecution != null)
                {
                    result += " Job ID: " + CurrentExecution.JobID.ToString() + " (Job Execution ID: " + CurrentExecution.JobExecutionID.ToString() + ")";
                }
                return result;
            }
        }
        public static void ConfigureLibrary(string location)
        {
            if (Library == null)
                Library = new JobLibrary(location);
        }
        /// <summary>
        /// Work Queue lock
        /// </summary>
        static object workLockObj = new object();
        const string WORK_LOCK_TARGET = nameof(SEIDR.JobExecutor) + "." + nameof(workQueue);
        
        /// <summary>
        /// Thread name lock. (Since Job imports can be single thread required, organized by Name)
        /// </summary>
        static object NameLock = new object();

        public JobExecutor( JobExecutorService caller, DatabaseManager manager)
            : base(manager, caller, ExecutorType.Job)
        {
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.COMPLETE, Description = nameof(ExecutionStatus.COMPLETE), IsComplete = true, NameSpace = "SEIDR" });
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.FAILURE, Description= nameof(ExecutionStatus.FAILURE), IsError = true, NameSpace = "SEIDR" });

            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.REGISTERED, Description = nameof(ExecutionStatus.REGISTERED), NameSpace = "SEIDR" });
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.SCHEDULED, Description = nameof(ExecutionStatus.SCHEDULED), NameSpace = "SEIDR" });
            CheckStatus(new ExecutionStatus
            {
                ExecutionStatusCode = ExecutionStatus.MANUAL,
                Description = nameof(ExecutionStatus.MANUAL),
                NameSpace = "SEIDR"
            });

            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.STEP_COMPLETE, Description = nameof(ExecutionStatus.STEP_COMPLETE), NameSpace = "SEIDR" });            
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.CANCELLED, Description = nameof(ExecutionStatus.CANCELLED), NameSpace = "SEIDR" });
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.SPAWN, Description = nameof(ExecutionStatus.SPAWN), NameSpace = "SEIDR" });
            CheckStatus(new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.INVALID, Description = "INVALID FILE PROGRESSION - Path used by another JobExecution", IsError = true, NameSpace = "SEIDR" });
            
        }
        const string SET_STATUS = "SEIDR.usp_JobExecution_SetStatus";
        const string REQUEUE = "SEIDR.usp_JobExecution_Requeue";
        const string GET_WORK = "SEIDR.usp_JobExecution_sl_Work";
        const string START_WORK = "SEIDR.usp_JobExecution_StartWork";
        
        public volatile JobExecutionDetail CurrentExecution;
        volatile IJobMetaData currentJobMetaData;

        public volatile bool CancelRequested = false;
        volatile bool cancelSuccess = false;
        public bool checkAcknowledgeCancel()
        {
            if (!CancelRequested)
                return false;

            cancelSuccess = true;
            return true;
        }

        
        public DatabaseConnection connection => _Manager.CloneConnection();
        public DatabaseManager Manager { get { return _Manager; } }

        public JobProfile job => CurrentExecution.ExecutionJobProfile;

        volatile bool _requeue = false;
        public void Requeue(int delayMinutes)
        {
            CurrentExecution.DelayStart = DateTime.Now.AddMinutes(delayMinutes);
            CurrentExecution.ThreadChecked = false;
            _requeue = true;
            LogInfo("Requeue requested: " + delayMinutes + " minutes.");
        }

        //public override int? ExecutionTimeSeconds => currentExecution != null ? currentExecution.ExecutionTimeSeconds : null;
        public override bool CheckForLongRun(ref int executionTimeSeconds)
        {
            executionTimeSeconds = 0;
            bool result = false;
            if (IsWorking && CurrentExecution != null && currentJobMetaData != null)
            {
                int? execSeconds = CurrentExecution.GetExecutionTimeSeconds();
                executionTimeSeconds = execSeconds ?? 0;
                if (executionTimeSeconds > 0)
                {
                    int notificationSec = currentJobMetaData.NotificationTime * 60;
                    result = (executionTimeSeconds > notificationSec);
                }
            }

            return result;
        }

        protected override void Work()
        {            
            cancelSuccess = false;
            CancelRequested = false;
            _requeue = false;
            try
            {
                CurrentExecution = CheckWork();
                if (CurrentExecution == null)
                    return;
                using (var h = _Manager.GetBasicHelper())
                {
                    bool tc = CurrentExecution.ThreadChecked;
                    int? thread = tc? CurrentExecution.RequiredThreadID : null;
                    
                    Debug.Assert(CurrentExecution.JobExecutionID.HasValue);
                    long JobExecutionID = CurrentExecution.JobExecutionID.Value;

                    h.QualifiedProcedure = START_WORK;
                    h[nameof(JobExecutionID)] = JobExecutionID; //CurrentExecution.JobExecutionID; Keep for logging below
                    CurrentExecution = _Manager.SelectSingle<JobExecutionDetail>(h);
                    //currentExecution.ExecutionTimeSeconds
                    if (h.ReturnValue != 0 || CurrentExecution == null)
                    {
                        LogInfo("JobExecutionID " + JobExecutionID + " - unable to verify data. Removing from queue.", true);
                        return; //Executor's ThreadName will have been changed, but that should be okay. Execution is no longer executable, so it's being removed from queue/service.
                    }

                    CurrentExecution.ThreadChecked = tc;
                    if(thread.HasValue)
                        CurrentExecution.RequiredThreadID = thread;
                }
                CurrentExecution.ExecutionJobProfile = _Manager.SelectSingle<JobProfile>(CurrentExecution);
                
                ExecutionStatus status = null;
                bool success = false;
                using (new LockHelper(Lock.Shared, libraryLock))
                {
                    //remove Volatile warning as ref
#pragma warning disable 420
                    IJob job = Library.GetOperation(CurrentExecution.JobName,
                            CurrentExecution.JobNameSpace,
                            out currentJobMetaData);
#pragma warning restore 420
                    if (job == null)
                    {
                        LogInfo("Unable to load job..."); //shouldn't happen unless someone manually adds something, or manually sets Loaded to 1, or something like that.
                        return;
                    }
                    if(currentJobMetaData.NeedsFilePath && CurrentExecution.FilePath == null)
                    {
                        LogError("Job requires a FilePath set in the JobExecution table.", null, CurrentExecution.JobID);
                        SetExecutionStatus(false, ExecutionStatus.INVALID);
                        return;
                    }
                    int matchID = ThreadID == JobExecutorCount ? 0 : ThreadID;
                    if (!CurrentExecution.ThreadChecked 
                        && !currentJobMetaData.SingleThreaded) //If single threaded, don't care.
                    {
                        int newThread = job.CheckThread(CurrentExecution, ThreadID, this) % JobExecutorCount;
                        if (newThread >=0 && newThread != matchID)
                        {
                            /*
                             * if new thread goes over the ExecutorCount, it might still be okay in this thread. 
                             * If (newThread % ExecutorCount) + 1 is this ID (modulo is 0 based, but executor count starts at 1)
                             * 
                             */
                            using (var h = _Manager.GetBasicHelper())
                            {
                                h.QualifiedProcedure = "[SEIDR].[usp_JobExecution_UnWork]"; //Mark as not working again, we're reverting to 'InQueue'
                                h[nameof(CurrentExecution.JobExecutionID)] = CurrentExecution.JobExecutionID;
                                _Manager.ExecuteNonQuery(h);
                            }
                            CurrentExecution.RequiredThreadID = newThread;
                            if(!currentJobMetaData.RerunThreadCheck)
                                CurrentExecution.ThreadChecked = true;
                            
                            Queue(CurrentExecution); //put it back into the queue, since we removed it when first picking work

                            LogInfo("JobExecutionID " + CurrentExecution.JobExecutionID + ", changed required Thread to " + newThread, true);
                            return;
                        }
                    }

                    if (!TryGetJobLock(currentJobMetaData))
                    {
                        CurrentExecution.DelayStart = DateTime.Now.AddMinutes(1.5);
                        Queue(CurrentExecution);
                        LogInfo("Single Threaded Job already running. Requeue");
                        return;
                    }
                    LogStart();
                    success = job.Execute(this, CurrentExecution, ref status);

                    if (cancelSuccess)
                    {
                        status = new ExecutionStatus
                        {
                            ExecutionStatusCode = ExecutionStatus.CANCELLED,
                            NameSpace = nameof(SEIDR)
                        };
                    }
                    else if (status == null)
                    {
                        if (success)
                            status = new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.STEP_COMPLETE, NameSpace = nameof(SEIDR) };
                        else
                            status = new ExecutionStatus { ExecutionStatusCode = ExecutionStatus.FAILURE, NameSpace = nameof(SEIDR) };

                    }
                    else if (string.IsNullOrWhiteSpace(status.NameSpace))
                        status.NameSpace = currentJobMetaData.NameSpace;

                    LogFinish(status);
                } //libraryLock
                if (cancelSuccess)
                {
                    //CancellationExecutor picked up this JobExecutionID and requested stopping. Job was able to respond and cancel. 
                    SetExecutionStatus(false, ExecutionStatus.CANCELLED);
                }
                else
                {
                    if(!success && _requeue)
                    {
                        Manager.Execute("SEIDR.usp_JobExecution_ReQueued", 
                                new { JobExecutionID = CurrentExecution.JobExecutionID });
                        Queue(CurrentExecution);
                        return;
                    }
                    
                    CheckStatus(status);
                    
                    SetExecutionStatus(success, status.ExecutionStatusCode, status.NameSpace);
                    SendNotifications(CurrentExecution, success, status);                    
                }
            }
            catch (Exception ex)
            {                
                if (CurrentExecution?.Started == true)
                    LogFinish(fail);
                LogError("JobExecutor.Work()", ex, null);
                if (CurrentExecution != null)
                {
                    SetExecutionStatus(false, fail.ExecutionStatusCode, fail.NameSpace);
                    SendNotifications(CurrentExecution, false, fail);
                }
            }
            finally
            {
                ReleaseJobLock(currentJobMetaData);
                currentJobMetaData = null;
                CurrentExecution = null;
            }
        }
        readonly ExecutionStatus fail = new ExecutionStatus
        {
            IsError = true,
            ExecutionStatusCode = ExecutionStatus.FAILURE,
            NameSpace = nameof(SEIDR),
            IsComplete = false
        };
        #region single threaded logic
        private static readonly object SingleThreadJobLock = new object();
        private static readonly Dictionary<string, int> JobThread = new Dictionary<string, int>();
        bool TryGetJobLock(IJobMetaData metaData)
        {
            if (!metaData.SingleThreaded)
                return true;
            string Key = metaData.ThreadName ?? $"{metaData.NameSpace}.{metaData.JobName}";
            lock (SingleThreadJobLock)
            {
                if (JobThread.ContainsKey(Key) 
                    && JobThread[Key] != ThreadID) //If we somehow get stuck as the owner of this job, just let it work.
                    return false;
                JobThread[Key] = this.ThreadID;
                return true;
            }
        }

        void ReleaseJobLock(IJobMetaData metaData)
        {
            if (metaData == null || !metaData.SingleThreaded)
                return;
            string key = metaData.ThreadName ?? $"{metaData.NameSpace}.{metaData.JobName}";
            lock (SingleThreadJobLock)
            {
                if(JobThread[key] == ThreadID)
                    JobThread.Remove(key);
            }
        }
        #endregion
        void SendNotifications(JobExecutionDetail executedJob, bool success, ExecutionStatus result)
        {
            string subject, mailTo;
            string durationMessage = string.Empty;
            string branchMessage = executedJob.Branch == "MAIN" ? string.Empty : $"(BRANCH: '{executedJob.Branch}')";
            if (success)
            {
                if (string.IsNullOrWhiteSpace(executedJob.SuccessNotificationMail)
                    || !executedJob.Complete) //Out parameter on set status.
                    return;
                //Don't send a completion notification if the execution status indicates to skip success notifications. E.g., a status used after spawning child executions under the same profile which would be the true completions
                if (result.SkipSuccessNotification)
                    return; 
                mailTo = executedJob.SuccessNotificationMail;
                subject = $"Job Execution completed: Job Profile {executedJob.JobProfileID }{branchMessage} - {executedJob.ProcessingDate:MMM dd, yyyy}";
                
                Debug.Assert(executedJob.ExecutionTimeSeconds.HasValue);
                int duration = executedJob.ExecutionTimeSeconds.Value + executedJob.TotalExecutionTimeSeconds;
                durationMessage = ". Total Execution Duration: " + duration / 60 + "m, " + duration % 60 + "s";
            }
            else
            {
                //If retryCountBeforeFailure = 50, then RetryCount should reach 50 before sending a notification.
                // In SetStatus, current RetryCount must be <= RetryLimit to try again.
                // So the last execution attempt will be when current RetryCount = RetryLimit.
                //To only send one failure notification, should be able to set RetryCountBeforeFailureNotification = RetryLimit.
                if (string.IsNullOrWhiteSpace(executedJob.FailureNotificationMail)
                    || executedJob.RetryCountBeforeFailureNotification > executedJob.RetryCount) 
                    return;
                mailTo = executedJob.FailureNotificationMail;
                subject = $"Job Execution Step failure: Job Profile {executedJob.JobProfileID}{branchMessage} - {executedJob.ProcessingDate:MMM dd, yyyy}: Step # {executedJob.StepNumber}";
                if (executedJob.ExecutionTimeSeconds.HasValue)
                {
                    int duration = executedJob.ExecutionTimeSeconds.Value;
                    durationMessage = ". Step, Execution Duration: " + (duration / 60) + "m, " + duration % 60 + "s";
                }
            }
            string stepMessage = string.Empty;
            if (!success)
            {
                stepMessage = $"{executedJob.JobNameSpace}.{executedJob.JobName} - '{executedJob.Step}'{Environment.NewLine}";
                //Step description should never be null/white space in practice.
                // Configurations procedures almost all auto generate a description if one isn't provided after trimming
            }
            string fileMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(executedJob.FilePath))
            {
                fileMessage = $"{Environment.NewLine}" +
                              $"File Name: '{System.IO.Path.GetFileName(executedJob.FilePath)}'{Environment.NewLine}" +
                              $"Directory: '{System.IO.Path.GetDirectoryName(executedJob.FilePath)}'{Environment.NewLine}" +
                              $"File Path: '{executedJob.FilePath}'{Environment.NewLine}";

            }

            string statusMessage = result 
                                   + (cancelSuccess ? " - CANCELLED " : string.Empty)
                                   + Environment.NewLine 
                                   + "Execution Start Time: " + executedJob.DetailCreated 
                                   + durationMessage;

            string message = $"{executedJob.ExecutionJobProfile.Description}{Environment.NewLine}{Environment.NewLine}" 
                             + $"JobExecutionID: {executedJob.JobExecutionID}({executedJob.ProcessingDate:MM/dd/yyyy}){Environment.NewLine}"
                    + $"{executedJob.KeyInformation}{Environment.NewLine}{Environment.NewLine}"
                             + stepMessage
                             + fileMessage
                             + Environment.NewLine //Line Break between any above information and status information summary
                             + statusMessage;



            CallerService.TrySendMail(this, mailTo, subject, message, HTML: false);
        }
        
        static readonly object StatusLock = new object();
        void SetExecutionStatus(bool success, string statusCode, string StatusNameSpace = nameof(SEIDR))
        {            
            if (CurrentExecution == null)
                return;            
            using (var i = _Manager.GetBasicHelper(CurrentExecution, true))
            {
                i.RetryOnDeadlock = true;
                i.DeadlockRetryLimit = 20;
                i.ExpectedReturnValue = 0;
                i.QualifiedProcedure = SET_STATUS;
                i["Working"] = false; //Should be removed from DB parameter list, but it's okay to keep in dictionary
                i["Success"] = success;
                i["ExecutionStatusCode"] = statusCode;
                i["ExecutionStatusNameSpace"] = StatusNameSpace;
                JobExecutionDetail next;
                lock(StatusLock)
                    next = _Manager.SelectSingle<JobExecutionDetail>(i);

                if (i.ReturnValue == i.ExpectedReturnValue)
                {
                    if (next != null)
                    {
                        if (!success && next.CanStart)
                            next.DelayStart = DateTime.Now.AddMinutes(currentJobMetaData.DefaultRetryTime ?? 10);
                        Queue(next);
                    }
                }
                else
                {
                    string message = String.Format("SEIDR.usp_JobExecution_SetStatus failed with return code - {0}", i.ReturnValue);
                    if (i.ReturnValue == 70)
                    {
                        message = "Invalid or bad configuration prevented would set FilePath to one that has already processed.";
                    }

                    Exception ex = new Exception(message);                                      
                    LogError("JobExecutor.SetExecutionStatus()", ex, null);
                }
                //else
                //    currentExecution.Complete = (bool)i["@Complete"]; //completion notification
            }            
        }
        /// <summary>
        /// Add the Execution Detail to the workQueue
        /// </summary>
        /// <param name="job">ExecutionDetail to queue for execution</param>
        /// <param name="Cut">If true, adds to position 0 and skips sorting.</param>
        public static void Queue(JobExecutionDetail job, bool Cut = false)
        {
            lock (workLockObj)
            {
                if (workQueue.Exists(detail => detail.JobExecutionID == job.JobExecutionID))
                {
                    workQueue.RemoveAll(detail => detail.JobExecutionID == job.JobExecutionID && detail.DetailCreated <= job.DetailCreated);
                    //shouldn't happen, but as a safety, keep the latest one.                    
                    if (workQueue.Exists(detail => detail.JobExecutionID == job.JobExecutionID))
                        return;
                }
                //else if (replaceOnly) //Execution is going to be validated and refreshed before actually starting work anyway, so safe to add to the queue anyway.
                //    return;

                if (Cut)
                    workQueue.Insert(0, job);
                else
                {
                    workQueue.Add(job);
                    workQueue.Sort((a, b) =>
                    {
                        //positive: a is greater.                    
                        if (a.DelayStart.HasValue && b.DelayStart.HasValue)
                        {
                            if (a.DelayStart.Value > b.DelayStart.Value)
                                return 1;
                            return -1;
                        }
                        else if (a.DelayStart.HasValue)
                            return 1;
                        if (b.DelayStart.HasValue)
                            return -1; // (int)DateTime.Now.Subtract(b.DelayStart.Value).TotalSeconds; //Treat b as greater
                        if (a.WorkPriority > b.WorkPriority)
                            return 1;
                        return a.WorkPriority < b.WorkPriority ? -1 : 0;
                    });
                }
            }
        }
        public JobExecutionCheckPoint LogCheckPoint(int CheckPointNumber, string Message = null, string Key = null)
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                Message = $"CHKPNT [{CheckPointNumber}]";
                if (Key != null)
                    Message += "('" + Key + "')";
            }
            JobExecutionCheckPoint chk = new JobExecutionCheckPoint                
            (CurrentExecution)
            {
                CheckPointDuration = CurrentExecution.CheckPoint(),
                CheckPointKey = Key,
                Message = Message,
                CheckPointNumber = CheckPointNumber,
                ThreadID = this.ThreadID
            };
            _Manager.Insert(chk);
            string keyDesc = string.Empty;
            if (Key != null)
                keyDesc = "(" + keyDesc + ")";
            string checkpointLog  = $"{Environment.NewLine}CHECKPOINT #{chk.CheckPointNumber}{keyDesc}[{chk.CheckPointID}]:  JobID {chk.JobID} ({currentJobMetaData.JobName}.{currentJobMetaData.JobName}), JobProfile_JobID [{chk.JobProfile_JobID}]{Environment.NewLine}Elapsed Seconds(from last CheckPoint or ExecutionStart):{chk.CheckPointDuration}{Environment.NewLine}Message:{Message}";
            LogInfo(checkpointLog, true);
            return chk;
        }
        public JobExecutionCheckPoint GetLastCheckPoint()
        {            
            return _Manager.SelectSingle<JobExecutionCheckPoint>(CurrentExecution, suffix: "_GetLatest");                
        }
        

        /// <summary>
        /// Called by Service during startup, before setting up individual jobexecutors.
        /// </summary>
        /// <param name="Manager"></param>
        public static void CheckLibrary(DatabaseManager Manager)
        {
            if (LastLibraryCheck.AddMinutes(20) >= DateTime.Now)
                return;
            using (new LockHelper(Lock.Exclusive, LIBRARY_TARGET))
            {
                Library.RefreshLibrary();
                try
                {
                    Library.ValidateOperationTable(Manager);
                }
                finally
                {
                    LastLibraryCheck = DateTime.Now;
                }
            }
        }
        #region workload
        /// <summary>
        /// Goes through the work queue. If something workable is found, removes it from the queue and returns it
        /// </summary>
        /// <returns></returns>
        JobExecutionDetail CheckWork()
        {
            lock (NameLock)
                SetThreadName(null);
            lock (workLockObj)
            {                
                if (workQueue.UnderMaximumCount(Match, 0))
                    return null;                
                foreach (var detail in workQueue)
                {
                    if (!Match(detail))
                        continue; //Cannot start, or for a different thread

                    string threadName = detail.JobThreadName;
                    if (string.IsNullOrWhiteSpace(threadName))
                        threadName = $"{detail.JobNameSpace}.{detail.JobName}";

                    lock (NameLock)
                    {                        
                        //If we already have this ThreadName, don't need to check other threads, so skip
                        //If job is considered single threaded, need to check for any other thread running the job.
                        if (detail.JobSingleThreaded && !CallerService.CheckSingleThreadedJobThread(detail, ThreadID))
                        {                            
                            if (detail.RequiredThreadID != null)
                                detail.DelayStart = DateTime.Now.AddMinutes(1);


                            continue;
                            /*
                                If ThreadID is specified(not null, matches current threadID), then another thread is running with this JobName, 
                                but the job still has a required ThreadID. 
                                Add a delay and check the next record.
                                */
                        }
                        else
                            SetThreadName(threadName);

                    }
                    if (workQueue.Remove(detail))
                        return detail;
                }            
            }
            return null;
        } 
        /// <summary>
        /// Identify if the JobExecutor includes the JobExecutionID in its workload
        /// </summary>
        /// <param name="JobExecutionID"></param>
        /// <param name="remove">If it's not the current execution, remove from the workload queue.</param>
        /// <returns>True if the JobExecutionID is being worked or in the queue. 
        /// <para>Null if it has been removed from the queue as a result of this call.</para>
        /// <para>False if the execution was not on this Executor's workload</para>
        /// </returns>
        public bool? CheckWorkQueue(long JobExecutionID, bool remove)
        {
            lock (WorkLock)
            {
                if (!IsWorking)
                    return false;
            }
            if (CurrentExecution.JobExecutionID == JobExecutionID)
                return true;
            lock (workLockObj)
            {
                int i = workQueue.FindIndex(je => je.JobExecutionID == JobExecutionID);
                if (i >= 0)
                {
                    //h.Transition(Lock.Exclusive);
                    if (remove)
                    {
                        workQueue.RemoveAt(i);
                        return null;
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool CheckWorkQueue(long jobExecutionID)
        {
            lock(workLockObj)
            {
                return workQueue.Exists(d => d.JobExecutionID == jobExecutionID);
                    
            }
        }
        protected override void CheckWorkLoad()
        {           
            lock (workLockObj)
            {
                if(workQueue.HasMinimumCount(Match, 1))
                    return;
                
            }

            //first call a method on the callerService and see if there's any jobs we can grab from other threads.
            //If a thread has >= 5 jobs, grab a couple jobs from the thread. 
            using (var h = _Manager.GetBasicHelper())
            {
                h.QualifiedProcedure = GET_WORK;
                h.AddKey(nameof(ThreadID), ThreadID);
                h.AddKey("ThreadCount", JobExecutorCount);
                h.AddKey(nameof(BatchSize), BatchSize);

                var result = _Manager.SelectList<JobExecutionDetail>(h);
                if (result == null || result.Count == 0)
                    return;
                lock (workLockObj)
                {                    
                    workQueue.AddRange(result);
                    LogInfo("Added Jobs to WorkQueue. Queued Count:" + workQueue.Count(Match));
                }
            }
        }

        bool Match(JobExecutionDetail check)
        {
            if (check.RequiredThreadID == null)
                return check.CanStart;

            int MatchID = ThreadID == JobExecutorCount ? 0 : ThreadID;
            if ((check.RequiredThreadID % JobExecutorCount) != MatchID) //Modulo is 0 based, ThreadID is 1 based. If this is the last ThreadID, match against 0
                return false;
            return check.CanStart;
        }
        static List<JobExecutionDetail> workQueue = new List<JobExecutionDetail>();
        public override int Workload
        {
            get
            {                
                lock(workLockObj)
                    return workQueue.Count(Match);
            }
        }
        #endregion
        #region Service features
        
        public void SendMail(System.Net.Mail.MailMessage message)
        {
            int count = 10;
            while (!CallerService.TrySendMail(this, message))
            {
                count--;
                Thread.Sleep(LOG_FAILURE_WAIT);
            }
        }
        public override void Wait(int sleepSeconds, string logReason)
        {
            CallerService.LogToDB(this, CurrentExecution, "Sleep Requested: " + logReason, shared:false);
            SetStatus("Sleep requested:" + logReason, JobBase.Status.StatusType.Sleep_JobRequest);
            Thread.Sleep(sleepSeconds * 1000);
            SetStatus("Wake from Job Sleep Request");
        }
        const int LOG_FAILURE_WAIT = 5 * 1000;
        public void LogError(string message, Exception ex, int? extraID)
        {
            string exceptionMessage = string.Empty;
            if(ex != null)
            {
                exceptionMessage = Environment.NewLine + JobExecutorService.FormatExceptionMessage(ex) + Environment.NewLine + Environment.NewLine + ex.StackTrace;
            }
            //ToDo: handling for null Exception, allow Passing an ExtraID value?
            int count = 10;
            while(!CallerService.LogExecutionError(this, CurrentExecution, 
                message + exceptionMessage, 
                ExtraID: extraID) &&  count > 0)
            {
                count--;                
                Thread.Sleep(LOG_FAILURE_WAIT);
            }
        }
        void LogStart()
        {
            if (CurrentExecution == null)
                return;
            CurrentExecution.Start();
            int count = 5;
            ExecutionStatus startStatus = new ExecutionStatus
            {
                ExecutionStatusCode = CurrentExecution.ExecutionStatusCode,
                NameSpace = CurrentExecution.ExecutionStatusNameSpace
            };
            while(!CallerService.LogExecutionStartFinish(this, CurrentExecution, startStatus, true) && count > 0)
            {
                count--;
                Thread.Sleep(LOG_FAILURE_WAIT);
            }
        }
        void LogFinish(ExecutionStatus endStatus)
        {
            if (CurrentExecution == null)
                return;
            CurrentExecution.Finish();
            if (_requeue)
            {
                string message = "REQUEUE";
                if (CurrentExecution.DelayStart.HasValue)
                    message = $"REQUEUE - Will wait until {CurrentExecution.DelayStart.Value:yyyy-MM-dd HH:mm}";
                CallerService.LogToDB(this, CurrentExecution, message, true);
                return;
            }

            int count = 5;
            while (!CallerService.LogExecutionStartFinish(this, CurrentExecution, endStatus, false) && count > 0)
            {
                count--;
                Thread.Sleep(LOG_FAILURE_WAIT);
            }
        }
        void IJobExecutor.LogInfo(string message) => LogInfo(message, false);
        public override void LogInfo(string message, bool shared = false)
        {
            int count = 5;
            while(!CallerService.LogToDB(this, CurrentExecution, message, shared) && count > 0)
            {
                count--;
                Thread.Sleep(LOG_FAILURE_WAIT);
            }
        }

        protected override string HandleAbort()
        {
            if (CurrentExecution == null)
                return null;
            string msg = "JobExecutionID: " + CurrentExecution.JobExecutionID;
            SetExecutionStatus(false,  ExecutionStatus.CANCELLED);
            return msg;
            
        }
        public override bool Stop()
        {
            if (CallerService.ServiceAlive && (currentJobMetaData?.SafeCancel == true))
            {
                CancelRequested = true;
                return false;
            }            
            return base.Stop();
        }

        public DatabaseConnection GetConnection(string description)
        {
            var c = CallerService.DatabaseLookups.GetConnection(description);
            if (c == null)
                LogInfo($"Unable to get connection '{description}'");
            return c;
        }

        public DatabaseConnection GetConnection(int LookupID)
        {
            var c = CallerService.DatabaseLookups.GetConnection(LookupID);
            if (c == null)
                LogInfo($"Unable to get connection ID {LookupID}");
            return c;
        }

        public DatabaseManager GetManager(string description, bool ReadOnly = false)
        {
            var m = CallerService.DatabaseLookups.GetManager(description, ReadOnly);
            if (m == null)
                LogInfo($"Unable to get ConnectionManager '{description}'");
            return m;
        }

        public DatabaseManager GetManager(int LookupID, bool ReadOnly = false)
        {
            var m = CallerService.DatabaseLookups.GetManager(LookupID, ReadOnly);
            if (m == null)
                LogInfo($"Unable to get Connection manager for ConnectionID {LookupID}");
            return m;
        }

        #endregion
    }
}
