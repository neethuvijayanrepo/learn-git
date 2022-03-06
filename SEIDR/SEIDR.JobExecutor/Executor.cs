using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase.Status;
using System.Threading;

namespace SEIDR.JobExecutor
{
    public abstract class Executor
    {

        static int _maintenanceCounter = 0;
        static int _jobCounter = 0;
        static int _Queue_Counter = 0;
        static int _ReportCounter = 0;
        public static int JobExecutorCount => _jobCounter;
        public static int MaintenanceCount => _maintenanceCounter;
        public static int QueueCount => _Queue_Counter;
        public static int ReportCount => _ReportCounter;

        protected const int DEADLOCK_TIME_INCREASE = 45;
        protected const int MAX_TIMEOUT = 1200;

        public int ThreadID { get; private set; }
        volatile string _ThreadName;
        public string ThreadName
        {
            get { return _ThreadName; }
            private set { _ThreadName = value; }
        }
        public string LogName { get; private set; }

        public virtual string InfoDescription { get { return LogName + "(" + ThreadName + ")"; } }
        public DataBase.DatabaseManager _Manager { get; private set; }
        protected JobExecutorService CallerService { get; private set; }
        public ExecutorType ExecutorType { get; private set; }
        protected ThreadInfo Info { get; private set; }
        public ThreadStatus Status { get; private set; }
        protected object WorkLock = new object();
        protected abstract void CheckWorkLoad();
        public Executor(DataBase.DatabaseManager manager, JobExecutorService caller, ExecutorType type)
        {
            int id;
            switch (type)
            {
                case ExecutorType.Job:
                    id = ++_jobCounter;
                    break;
                case ExecutorType.Queue:
                    id = ++_Queue_Counter;
                    break;
                case ExecutorType.Reporting:
                    id = ++_ReportCounter;
                    break;
                default:
                    id = ++_maintenanceCounter;
                    break;
            }

            ThreadID = id;
            CallerService = caller;
            ExecutorType = type;

            string logName = this.GetType().GetDescription() + "(" + type.GetDescription() + "): Thread #" + id;
            LogName = $"{type}_{id}";
            _Manager = manager.Clone(true, logName);

            Info = new ThreadInfo(logName, type.ToString(), id);
            Status = new ThreadStatus(Info) { MyStatus = StatusType.Unknown };
            caller.MyStatus.Add(Status);
            SetThreadName(null);
        }
        public string Description => Info.Name;
        public void SetThreadName(string newName)
        {
            string mgrName = this.GetType().GetDescription() + "[" + ExecutorType.GetDescription() + "]: Thread #" + ThreadID;
            if (!string.IsNullOrWhiteSpace(newName))
            {
                mgrName += " - " + newName;
            }
            ThreadName = newName ?? GetType().GetDescription() + "(" + ThreadID + ")";
            _Manager.ProgramName = mgrName;
        }
        public volatile bool IsWorking;
        public virtual bool CheckForLongRun(ref int executionTimeSeconds)
        {
            executionTimeSeconds = 0;
            return false;
        }

        public abstract int Workload { get; }
        protected abstract void Work();
        [Obsolete("Should not be aborting the thread.", true)]
        protected virtual string HandleAbort() { return null;}
        public void SetStatus(string message, StatusType status = StatusType.General)
        {
            lock (Status) //Not best practice, but it is a status for 'this'
            {
                Status.SetStatus(message, status);
            }
        }
        public bool IsAlive
        {
            get
            {
                if (worker == null)
                    return false;
                return worker.IsAlive;
            }
        }
        public ThreadState WorkerState
        {
            get
            {
                if (worker != null)
                    return worker.ThreadState;
                return ThreadState.Unstarted;
            }
        }
        Thread worker;
        public void Call()
        {
            if(worker == null)
            {
                worker = new Thread(internalCall)
                {
                    IsBackground = true, 
                    //Note that background threads stop if a service doesn't have any foreground threads.
                    //Full threads because they're for long running processes.
                    Name = LogName
                };
            }
            int count = 20;
            while(worker.ThreadState.In(ThreadState.AbortRequested, ThreadState.SuspendRequested) && count > 0)
            {
                Wait(FAILURE_SLEEPTIME, "Waiting for thread to finish Abort/Suspend request...");
                count--;
            }
            lock (WorkLock)
            {
                if (worker.ThreadState.In(ThreadState.Running, ThreadState.WaitSleepJoin,
                    ThreadState.AbortRequested, ThreadState.SuspendRequested, ThreadState.Background))
                {
                    SetStatus("Executor.Call() - Thread Status is still: " + worker.ThreadState + " after waiting. Return.", 
                        StatusType.Unknown);
                    return;
                }
                IsWorking = false;
                worker.Start();
            }
        }
        public virtual bool Stop()
        {            
            return false;            
        }
        void internalCall()
        {            
            while (CallerService.ServiceAlive)
            {                
                try
                {
                    CallerService.PauseEvent.WaitOne();
                    SetStatus("Check Workload", StatusType.Start);
                    CheckWorkLoad();
                    if(Workload == 0)
                    {
                        SetStatus("No Work - sleep", StatusType.Sleep);
                        if (!Thread.Yield())
                            Thread.Sleep(FAILURE_SLEEPTIME * 1000);
                        //No Work, see if yielding will let another thread start some work in the meantime.                         
                        continue;
                    }
                    lock (WorkLock)
                    {
                        IsWorking = true;
                    }
                    Work();
                    SetStatus("Finish Work", StatusType.Finish);
                }/*
                catch(ThreadAbortException)//shouldn't happen anymore.
                {                                        
                    var m = HandleAbort();
                    if (!string.IsNullOrWhiteSpace(m))
                        SetStatus(m, ThreadStatus.StatusType.Unknown);                    
                }*/
                catch (System.Data.SqlClient.SqlException ex)
                {
                    SQLExceptionLog(ex);
                    SetStatus("SQL Exception: " + ex.Message, StatusType.Error);
                    if (ExecutorType.NotIn(ExecutorType.Job, ExecutorType.Queue))
                        Thread.Sleep(FAILURE_SLEEPTIME * 4000); //Extra delay for non Queue/Job if SQL error.
                }
                catch (Exception ex)
                {
                    ExceptionLog(ex);
                    //Note: ToString includes stack trace already
                    SetStatus("Error:" + ex.Message, StatusType.Error);
                    Thread.Sleep(FAILURE_SLEEPTIME * 1000);
                }
                finally
                {
                    lock (WorkLock)
                    {
                        IsWorking = false;
                    }
                }
            }
        }
        protected virtual void ExceptionLog(Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
            if (Environment.UserInteractive)
                return; //Not running as service.
#endif
            CallerService.LogToDB(this, ex.ToString() + Environment.NewLine, false, MessageType: "ERROR");
        }
        protected virtual void SQLExceptionLog(System.Data.SqlClient.SqlException ex)
        {
            const string SQLEXCEPTION_FORMAT = "{0}:{3}:{7}{6}LineNumber:{4}{6}Procedure:{5}{6}Data:{1}{6}StackTrace:{2}";

            string msg = string.Format(
                    SQLEXCEPTION_FORMAT
                    , ex.ErrorCode           //0
                    , ex.Data                //1
                    , ex.StackTrace          //2
                    , ex.Number              //3
                    , ex.LineNumber          //4
                    , ex.Procedure           //5
                    , Environment.NewLine    //6
                    , ex.State               //7
            );
            CallerService.LogToDB(this, msg, false, MessageType: "ERROR");
            foreach (SqlError err in ex.Errors)
            {
                msg = $@"Error Number:{err.Number}
Line Number: {err.LineNumber}
Procedure: {err.Procedure}
Server: {err.Server}
State: {err.State}
Severity: {err.Class}
Message: {err.Message}";
                CallerService.LogToDB(this, msg, false, "ERROR");
            }

        }
        protected const int FAILURE_SLEEPTIME = 15;
        public virtual void Wait(int sleepSeconds, string logReason)
        {
            if (string.IsNullOrWhiteSpace(logReason))
                logReason = "(UNSPECIFIED)";
            CallerService.LogToDB(this, "Sleep Requested: " + logReason, false);
            SetStatus("Sleep requested:" + logReason, StatusType.Sleep_JobRequest);
            Thread.Sleep(sleepSeconds * 1000);
            SetStatus("Wake from Job Sleep Request");
        }

        internal virtual void LogInfo(string message, JobExecutionShell shell, bool shared = false)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("DEBUG INFO: " + message);
            if (Environment.UserInteractive)
                return; //Not running as service.
#endif
            int count = 10;
            while (!CallerService.LogToDB(this, shell, message, shared) && count > 0)
            {
                count--;
                Thread.Sleep(FAILURE_SLEEPTIME * 1000);
            }
        }
        public virtual void LogInfo(string message, bool shared = false)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("DEBUG INFO: " + message);
            if (Environment.UserInteractive)
                return; //Not running as service.
#endif
            LogInfo(message, null, shared);
        }        
    }
    public enum ExecutorType
    {
        Maintenance,
        Job,
        Queue,
        Reporting
    }
}
