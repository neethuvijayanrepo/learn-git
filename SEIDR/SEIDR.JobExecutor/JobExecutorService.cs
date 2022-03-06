using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SEIDR.Doc;
using SEIDR;
using SEIDR.OperationServiceModels;
using System.Threading;
using System.ServiceProcess;
using System.ComponentModel.Composition;
using System.Configuration;
using SEIDR.JobBase;
using SEIDR.JobBase.Status;

namespace SEIDR.JobExecutor
{    
    public class JobExecutorService : ServiceBase
    {       
        /// <summary>
        /// Called if job meta data indicates single threaded. Makes sure there isn't another thread running the job.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="checkID"></param>
        /// <returns></returns>
        public bool CheckSingleThreadedJobThread(JobExecutionDetail job, int checkID)
        {            
            return(_jobList.UnderMaximumCount(executor =>
            {
                if (executor.ThreadID == checkID)
                    return false;
                if (executor.ThreadName == job.JobThreadName)
                    return true;
                return false;
            }, 1)); //True if none of the other executors have the threadName.            
        }        
        public void QueueExecution(JobExecutionDetail newJob)
        {
            if (newJob == null)
                return;
            JobExecutor.Queue(newJob);
        }
        List<JobExecutor> _jobList;
        List<Executor> _executorList;        
        public JobExecutorService()
        {
            DataBase.DatabaseManagerHelperModel.DefaultRetryOnDeadlock = true;
            ServiceName = "SEIDR.JobExecutor";
            AutoLog = true;
            CanStop = true;
            CanPauseAndContinue = true;
            CanShutdown = false;
            CanHandleSessionChangeEvent = false;
            CanHandlePowerEvent = false;
        }        
        public JobExecutorService(DataBase.DatabaseManager manager, string logDirectory = null)
            :this()
        {
            this._MGR = manager;
            LogDirectory = logDirectory ?? ConfigurationManager.AppSettings["LogRootDirectory"] ?? @"C:\SEIDR\Logs\";
        }
        public byte QueueThreadCount = 2;
        public byte ExecutionThreadCount = 4; //Default        
        public int BatchSize = 5;
        string dbServerSetting; //For status.
        public ServiceStatus MyStatus { get; private set; } = new ServiceStatus();

        public DatabaseLookupSet DatabaseLookups { get; private set; }

        void SetupFromConfig()
        {

            var appSettings = ConfigurationManager.AppSettings;
            int tempInt;
            string temp = appSettings["Timeout"];
            if (!int.TryParse(temp, out tempInt) || tempInt < 60)
                tempInt = 60;

            DataBase.DatabaseConnection db = new DataBase.DatabaseConnection(
                appSettings["DatabaseServer"],
                appSettings["DatabaseCatalog"]
                )
            {
                Timeout = tempInt,
                CommandTimeout = tempInt * 3
            };
            _MGR = new DataBase.DatabaseManager(db, "SEIDR") { RethrowException = false, ProgramName = "SEIDR.JobExecutor"};
            dbServerSetting = db.Server;
            
            //OperationExecutor.ExecutionManager = _MGR.Clone(reThrowException: true, programName: "SEIDR.JobExecutor Query");

            temp = appSettings["BatchSize"];
            if (!int.TryParse(appSettings["BatchSize"], out BatchSize) || BatchSize < 5)
                BatchSize = 5;

            LogDirectory = appSettings["LogRootDirectory"] ?? @"C:\Logs\SEIDR.JobExecutor\";
            if (!Directory.Exists(LogDirectory))
            {
                try
                {
                    Directory.CreateDirectory(LogDirectory);
                }
                catch (Exception ex)
                {
                    LogDirectory = @"C:\Logs\SEIDR.JobExecutor\";
                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);
                    LogFileMessage("Error Checking LogDirectory: " + ex.Message);
                }
            }
            
            try
            {
                JobExecutor.ConfigureLibrary(appSettings["JobLibrary"]);
                //OperationExecutor.SetLibrary(appSettings["JobLibrary"]);
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                LogFileMessage("Library Set up Error: " + sb.ToString());
                throw;
            }
            catch (Exception ex)
            {
                LogFileMessage("Library set up error - " + ex.Message);
                throw;
            }

            System.Net.Mail.MailAddress sender = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(appSettings["MailSender"]))
                    sender = new System.Net.Mail.MailAddress(appSettings["MailSender"], appSettings["SenderDisplayName"]);

                _Mailer = new Mailer(sender, SendTo: appSettings["StatusMailTo"]);
                _Mailer.SMTPServer = appSettings["SmtpServer"];
                Mailer.Domain = appSettings["MailDomain"];
                if (string.IsNullOrEmpty(_Mailer.SMTPServer))
                    _Mailer = null;
            }
            catch(Exception ex)
            {
                _Mailer = null;
                LogFileMessage("Unable to properly configure mailer: " + ex.Message);
            }
            temp = appSettings["ThreadCount"];
            const int MAX_EXECUTOR_COUNT = 45;
            const int MAX_QUEUE_COUNT = 16;

            if (!int.TryParse(temp, out tempInt))
                tempInt = 4;
            else if (tempInt > MAX_EXECUTOR_COUNT)
                tempInt = MAX_EXECUTOR_COUNT;

            byte ThreadCount;            
            ThreadCount = (byte)tempInt;
            ExecutionThreadCount = ThreadCount;
            
            temp = appSettings["QueueThreadCount"];
            if (!byte.TryParse(temp, out ThreadCount) || ThreadCount < 1)
                ThreadCount = 1;
            else if (ThreadCount > MAX_QUEUE_COUNT)
                ThreadCount = MAX_QUEUE_COUNT;
            QueueThreadCount = ThreadCount;
            var d = _MGR.ExecuteText("SELECT 1", false);
            if (d == null)
            {
                LogFileMessage("Unable to connect to database: " + db.Server + "." + db.DefaultCatalog);
                throw new ArgumentException("Server");
            }
            LogFileMessage("Configuration done. Server: " + db.Server + " Catalog: " + db.DefaultCatalog);
            /*
            temp = appSettings["BatchSize"];
            if (!byte.TryParse(temp, out _BatchSize) || _BatchSize < 1)
                _BatchSize = 1;
            else if (_BatchSize > 10)
                _BatchSize = 10;
            */
        }
        

        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                JobExecutorService om = new JobExecutorService();
                om.Run();
                while (om.ServiceAlive)
                {
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Run(new JobExecutorService());
                //Don't use the array version, 
                //because we don't have multiple implementations of ServiceBase in the executable
            }
        }
        /// <summary>
        /// Static to set from static constructor time (before instance constructor)
        /// </summary>
        static readonly DateTime STARTUP = DateTime.Now;
        public void Run()
        {            
            SetupFromConfig();
            LogFileMessage("STARTING UP");
            JobExecutor.CheckLibrary(DataManager);
            JobExecutor.PopulateStatusList(DataManager);
            LogFileMessage("Job Library configured");

            BasicLocalFileHelper.ClearRootWorkingDirectory();
            LogFileMessage("Root Local File Helper directory cleared.");
            #region Executor Set up            
            _executorList = new List<Executor>();
            _jobList = new List<JobExecutor>();
            for (byte i = 1; i <= ExecutionThreadCount; i++)
            {
                var je = new JobExecutor(this, DataManager);
                _executorList.Add(je);
                _jobList.Add(je);                
                //MyOperators.Add(new OperationExecutor(this, i));
            }            
            for (byte i = 1; i <= QueueThreadCount; i++)
            {
                _executorList.Add(new Queue(this, DataManager));
                //MyOperators.Add(new Queue(this, i));
            }
            //executorList.Add(new ReDistributor(DataManager, this, jobList));
            _executorList.Add(new CancellationExecutor(this, DataManager, _jobList));
            _executorList.Add(new ScheduleChecker(this, DataManager));
            _executorList.Add(new ResetDelayExecutor(this, DataManager));
            //MyOperators.Add(new Queue(this, QUEUE_ID));



            MyStatus.BatchSize = BatchSize;
            MyStatus.JobExecutorCount = Executor.JobExecutorCount;
            MyStatus.QueueExecutorCount = QueueThreadCount;
            MyStatus.MaintenanceCount = Executor.MaintenanceCount;

            DataManager.ExecuteNonQuery("SEIDR.usp_JobExecution_CleanWorking");            

            _ServiceAlive = true;
            if (!Environment.UserInteractive && !string.IsNullOrEmpty(_Mailer?.SendTo))
            {
                try
                {
                    _Mailer.SendMail("Service Startup - " + Environment.MachineName, GetOverallStatus(true));
                }
                catch(Exception ex)
                {
                    LogToDB(null, ex.Message, true, "_MAIL");
                }
                
            }

            _executorList.ForEach(e => e.Call());
            LogFileMessage("START UP DONE, EXECUTORS STARTED");
            #endregion
            DateTime lastStatus = DateTime.MinValue;
            DateTime lastLog = DateTime.MinValue;
            DateTime lastLongRunCheck = DateTime.MinValue;

            string interval = ConfigurationManager.AppSettings["LongRunCheckInterval"];

            DatabaseLookups = new DatabaseLookupSet(_MGR);


            int longRunCheckIntervalSeconds;
            if (!int.TryParse(interval, out longRunCheckIntervalSeconds))
            {
                longRunCheckIntervalSeconds = 150;
            }

            while (ServiceAlive)
            {
                _mre.WaitOne();
                try
                {
                    JobExecutor.CheckLibrary(DataManager);
                    DateTime n = DateTime.Now;

                    int minute = n.Minute;

                    if (n.Subtract(lastStatus).TotalSeconds > 45)
                    {
                        try
                        {
                            MyStatus.WriteToFile(LogDirectory);
                            lastStatus = n;
                        }
                        catch
                        {
                            Thread.Yield();
                            continue;
                        }
                    }                  

                    if (!string.IsNullOrEmpty(_Mailer?.SendTo) 
                        && n.Subtract(lastLongRunCheck).TotalSeconds > longRunCheckIntervalSeconds)
                    {
                        
                        try
                        {
                            lastLongRunCheck = n;
                            int executionTimeSec = 0;
                            if (_executorList.Exists(e => e.CheckForLongRun(ref executionTimeSec)))
                            //If current execution is long run
                            {
                                string warn = "Thread Long Running Execution Info";
                                foreach (Executor e in _executorList)
                                {
                                    executionTimeSec = 0;                                    
                                    if (e.CheckForLongRun(ref executionTimeSec))
                                    {
                                        warn += "<br />" + e.InfoDescription + ": "
                                                + "<br />" + "Execution time: " + executionTimeSec.ToString() + " seconds."
                                                + $" ({executionTimeSec / 60} minutes)"
                                                + "<br />";
                                    }
                                }
                                //warn += "<br />" + "<br />" + GetOverallStatus(true);

                                _Mailer.SendMail("Service Long Running Job Alert - " + Environment.MachineName, warn); 
                            }
                        }
                        catch
                        {
                            Thread.Yield();
                            continue;
                        }
                    }

                    if (Environment.UserInteractive)
                    {
                        continue;
                    }

                    if (n.Subtract(lastLog).TotalMinutes >= 5)                    
                    {
                        try
                        {
                            DatabaseLookups.Refresh(_MGR);
                            LogFileMessage(GetOverallStatus(false));

                            if (!string.IsNullOrEmpty(_Mailer?.SendTo)
                                && minute % 60 == 0 // on the hour
                                && _executorList.Exists(e => e.Status.MyStatus == StatusType.Error)) 
                                //If Current status is error.
                            {
                                string Error = "Thread Error Info";
                                _executorList.Where(e => e.Status.MyStatus == StatusType.Error).ForEach((e) =>
                                {                                       
                                    Error += Environment.NewLine + e.LogName + "(" + e.ThreadName + "): "
                                            + Environment.NewLine + e.Status.LastErrorMessage + " - " + e.Status.LastError.ToString()
                                            + Environment.NewLine;
                                });
                                Error += Environment.NewLine + Environment.NewLine + GetOverallStatus(true);


                                _Mailer.SendMail("Service Error Alert - " + Environment.MachineName, Error); //Send a mail for listing unhandled exceptions once an hour,.
                            }
                            lastLog = n;
                        }
                        catch
                        {
                            Thread.Yield();
                            continue;
                        }                        
                    }
                    //Don't use the daily folder for current status XML file.   
                    //Maybe to do: Delete old log directories? Not something I'd consider especially important, though, so should be okay to do manually
                }
                finally
                {
                    if(ServiceAlive)
                        Thread.Sleep(SLEEP_TIME);
                }
            }            
        }       


        const int SECONDS = 1000;
        const int SLEEP_TIME = 15 * SECONDS;
   
        #region Service control logic
        ManualResetEvent _mre = new ManualResetEvent(true);
        volatile bool _ServiceAlive;
        public bool ServiceAlive
        {
            get
            {
                return _ServiceAlive;
            }
        }
        public ManualResetEvent PauseEvent
        {
            get
            {
                return _mre;
            }
        }

        bool _Paused = false;
        public bool Paused { get { return _Paused; } }
        protected override void OnPause()
        {
            _Mailer.SendMail("Service PAUSED - " + Environment.MachineName, GetOverallStatus(true));
            LogFileMessage("PAUSED");
            _Paused = true;
            _mre.Reset();            
        }
        protected override void OnContinue()
        {
            _Mailer.SendMail("Service CONTINUE - " + Environment.MachineName, MailBody: GetOverallStatus(true));
            LogFileMessage("CONTINUE");
            _Paused = false;            
            _mre.Set();
        }
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Thread Main = new Thread(() =>
            {
                Run();
            })
            {
                IsBackground = false,
                Name = "SEIDR.JobExecutorService"
            };
            Main.Start();
        }
        protected override void OnStop()
        {
            _Mailer.SendMail("Service STOPPING - " + Environment.MachineName, GetOverallStatus(true));
            base.OnStop();
            int MaxWaitLoops = 15;
            int WaitLoops = 0;
            _ServiceAlive = false;
            while(WorkingCount > 0 && WaitLoops < MaxWaitLoops)
            {
                RequestAdditionalTime(14 * 1000);
                Thread.Sleep(11 * 1000); //JobExecutors work with Background threads, so will be forced to stop once foreground thread is done.
                WaitLoops++;
            }
        }
        public int WorkingCount
        {
            get
            {
                return _executorList.Count(j => j.IsWorking);
            }
        }
        #endregion

        DataBase.DatabaseManager _MGR;
        public DataBase.DatabaseManager DataManager { get { return _MGR; } }
        
        #region Logging

        //ToDo: move to logging helper class

        Mailer _Mailer; //Send success/failure notifications for Executors.
        public static string FormatExceptionMessage(Exception ex, string prefix = null)
        {
            int level = 0;
            StringBuilder sb = new StringBuilder(prefix);            
            while(ex != null)
            {
                sb.AppendFormat("[{0}]<{3}>: {1}{2}", ex.GetType().ToString(), ex.Message, Environment.NewLine, level++);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
        public bool TrySendMail(Executor e, string To, string Subject, string Message, bool HTML = false)
        {
            if (_Mailer == null)
            {
                LogToDB(e, "_Mailer not configured properly.", true, "_MAIL");
                return false;
            }
            try
            {
                _Mailer.SendMail(Subject, Message, RecipientList: To, isHtml: HTML);
                return true;
            }
            catch (Exception ex)
            {
                LogToDB(e, FormatExceptionMessage(ex, "MAIL FAILURE:\r\n"), true, MessageType: "_MAIL");
                return false;
            }
        }
        public bool TrySendMail(string To, string Subject, string Message, bool HTML = false)
        {
            if (_Mailer == null)
            {
                LogFileMessage("_Mailer not configured properly.");
                return false;
            }
            try
            {
                _Mailer.SendMail(Subject, Message, RecipientList: To, isHtml: HTML);
                return true;
            }
            catch(Exception ex)
            {
                LogFileMessage("MAIL FAILURE: " + ex.Message);
                return false;
            }
        }
        public bool TrySendMail(Executor e, System.Net.Mail.MailMessage message)
        {
            if (_Mailer == null)
            {
                LogToDB(e, "_Mailer not configured properly.", true, "_MAIL");
                return false;
            }

            _mailSync.Wait();
            try
            {
                _Mailer.SendMail(message);
                return true;
            }
            catch (Exception ex)
            {
                LogToDB(e, FormatExceptionMessage(ex, "MAIL FAILURE:\r\n"), true, MessageType: "_MAIL");
                return false;
            }
            finally
            {
                _mailSync.Release();
            }
        }

        public bool LogExecutionStartFinish(Executor caller, JobExecutionDetail je, ExecutionStatus status, bool start)
            => LogToDB(caller, je, start? $"START - Status={status.NameSpace}.{status.ExecutionStatusCode}" : $"FINISH - Status={status.NameSpace}.{status.ExecutionStatusCode}. Duration: {je.ExecutionTimeSeconds.ToString()} seconds.", true);
        
        public bool LogExecutionError(IJobExecution execution, string Message, int? ExtraID, int? CallerThread = null)
        {
            if (execution == null || execution.JobExecutionID == null)
                return true; //Nothing to log, just return true immediately.
            if (string.IsNullOrWhiteSpace(Message))
                return true;
            const string SPROC = "SEIDR.usp_JobExecutionError_i";            
            var m = new
            {
                execution.JobExecutionID,
                ErrorDescription = Message,
                ExtraID,                
                ThreadID = CallerThread
            };
            try
            {
                _MGR.ExecuteNonQuery(SPROC, m, RetryDeadlock: true);
            }
            catch
            {
                return false;
            }
            return true;
        }        
        public bool LogExecutionError(Executor caller, IJobExecution errBatch, string Message, int? ExtraID)
        {
            if (string.IsNullOrWhiteSpace(Message))
                return true;
            caller.SetStatus(Message, StatusType.Error);
            bool a = LogExecutionError(errBatch, Message, ExtraID, caller.ThreadID);
            bool b = LogToDB(caller, errBatch, (ExtraID.HasValue? "*" + ExtraID.Value + "*" + Environment.NewLine : string.Empty) + Message, shared: false, MessageType: "ERROR");
            return a && b;
        }

        private readonly SemaphoreSlim _mailSync = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim _dbSyncLog = new SemaphoreSlim(3, 3);
        public bool LogToDB(Executor callingOperator, IJobExecution e, string Message, bool shared, string MessageType = "INFO")
        {
            //Potential ToDo: change MessageType to enum in JobBase. Allow a config setting for minimum logging severity.
            if (string.IsNullOrWhiteSpace(Message))
                return true;
            //Note: the way this is called, a callingOperator is always going to be provided.
            //File logging should really only be the service itself.
            using (var h = DataManager.GetBasicHelper())
            {
                h.QualifiedProcedure = "SEIDR.usp_Log_i";
                h.RetryOnDeadlock = true;
                h.DeadlockRetryLimit = 3;
                h["ThreadID"] = callingOperator.ThreadID;
                h["ThreadType"] = callingOperator.ExecutorType.ToString();
                h["ThreadName"] = callingOperator.ThreadName;
                h["LogMessage"] = Message;
                h["MessageType"] = MessageType;
                var j = callingOperator as JobExecutor;
                if(j?.CurrentExecution != null)
                {
                    var exec = j.CurrentExecution;
                    h["JobProfileID"] = exec.JobProfileID;
                    h["JobExecutionID"] = exec.JobExecutionID;
                    h["JobProfile_JobID"] = exec.JobProfile_JobID;
                }
                else if(e != null)
                {
                    h["JobProfileID"] = e.JobProfileID;
                    if (e.JobProfile_JobID != 0)
                        h["JobProfile_JobID"] = e.JobProfile_JobID;
                    h["JobExecutionID"] = e.JobExecutionID;
                }

                _dbSyncLog.Wait();
                try
                {
                    //Note: SQL error here may not be handled safely by caller method, so release semaphore and ignore.
                    DataManager.ExecuteNonQuery(h);
                }
                finally
                {                   
                    _dbSyncLog.Release();
                }
            }
            return true;
        }
        string CurrentTimeMessage
        {
            get
            {
                return "[" + DateTime.Now.ToString("MMM dd, yyyy hh:mm:ss") + "] ";
            }
        }
        readonly string _startupTimeMessage = "[SERVICE STARTUP TIME: " + STARTUP.ToString("MMM dd, yyyy hh:mm:ss") + "] ";
        bool LogFileMessage(string Message)
        {
            if (string.IsNullOrWhiteSpace(Message))
                return true;
            string tempMessage = CurrentTimeMessage + Environment.NewLine;
            tempMessage += Message + Environment.NewLine + Environment.NewLine;
            string directory = GetDailyLogDirectory();
            string file = Path.Combine(directory, SHARED_LOG_FILE_FORMAT);
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                System.IO.File.AppendAllText(file, tempMessage);
                return true;
            }
            catch(IOException)
            {
                return false;
            }
        }
        public bool LogToDB(Executor caller, string Message, bool shared, string MessageType = "INFO")
        {
            return LogToDB(caller, null, Message, shared, MessageType: MessageType);
        }
        string LogDirectory;
        string GetDailyLogDirectory()
        {            
            if (string.IsNullOrWhiteSpace(LogDirectory))
                LogDirectory = @"C:\SEIDR.JobExecutor\Logs\";
            return Path.Combine(LogDirectory, DateTime.Now.ToString("yyyy_MM_dd"));         
        }
        const string LOG_FILE_FORMAT = "SEIDR.{0}.txt";
        const string SHARED_LOG_FILE_FORMAT = "SEIDR.JobExecutor.txt";

        string GetOverallStatus(bool HTML)
        {
            string
                PARA = "<p>",
                PARA_END = "</p>",
                BREAK = "<br />";
            if (!HTML) { PARA = ""; PARA_END = Environment.NewLine; BREAK = Environment.NewLine; }

            string Message =
                PARA +
                _startupTimeMessage +
                PARA +
                CurrentTimeMessage +
                PARA_END + PARA +
                "Database Server: " + dbServerSetting +
                PARA_END + PARA +
                "JobExecutor Count: " + ExecutionThreadCount +
                PARA_END + PARA +
                "Queue Count: " + QueueThreadCount +
                PARA_END + PARA +
                "Overall Thread(Executor) Count: " + _executorList.Count +
                PARA_END + BREAK 

                ;
            //var orderedOperators = MyOperators.OrderBy(o => ((int)o.MyType * 1000) + o.ID);
            //foreach(Operator o in orderedOperators)
            var orderedJobThreads = _executorList.OrderBy(e => (int)e.ExecutorType * 1000 + e.ThreadID);
            foreach (var ex in orderedJobThreads)
            {
                lock (ex.Status)
                {
                    Message += PARA + ex.ThreadName + $"({ex.LogName})" + PARA_END + PARA
                        + "Worker State: " + ex.WorkerState.ToString() + "( Live? " + ex.IsAlive + ")" + PARA_END + PARA
                        + ex.Status.LastStatusMessage
                        + (ex.Status.LastStatus.HasValue
                            ? (" - " + ex.Status.LastStatus.Value.ToLocalTime().ToString("MMM dd, yyyy hh:mm"))
                            : string.Empty)
                        + PARA_END + PARA + "Working ? " + ex.IsWorking.ToString()
                        + "     Work Load Size: " + ex.Workload
                        + PARA_END + BREAK;
                }
            }
            return Message + BREAK + BREAK;
        }

        #endregion

    }
}
