using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.OperationServiceModels;
using SEIDR.DataBase;
using SEIDR.JobBase;

namespace SEIDR.JobExecutor
{
    [System.ComponentModel.Description("File Registration Queue")]
    public class Queue : Executor
    {
        static Queue()
        {
            string s = System.Configuration.ConfigurationManager.AppSettings["QueueCutoverTime"];
            if (!string.IsNullOrWhiteSpace(s))
                CUTOVER = Convert.ToInt32(s);
        }
        static int _CUTOVER = -5;
        public static int CUTOVER
        {
            get
            {
                return _CUTOVER;
            }
            set
            {
                if (value > 0)
                    _CUTOVER = -value; //Ensure negative value. (Should be X minutes in the past
                else
                    _CUTOVER = value;
            }
        }
        public const string REGISTERED_FOLDER_NAME = "_Registered";
        public const string REJECTED_FOLDER_NAME = "_Rejected";
        public const string DUPLICATE_FOLDER_NAME = "_Duplicate";
        const string GRAB_WORK = "SEIDR.usp_JobProfile_sl_FileWatch";
        const string INVALID = "SEIDR.usp_JobProfile_u_InvalidPath";
        object map;        
        List<JobProfile> work = new List<JobProfile>();
        public Queue( JobExecutorService caller, DatabaseManager db)
            : base(db, caller, ExecutorType.Queue)
        {
            
            map = new
            {
                ThreadID,
                ThreadCount = caller.QueueThreadCount
            };            
        }

        public override int Workload => work.Count;
        protected override void ExceptionLog(Exception ex)
        {
            string Message = ex.ToString();
            if (LogShell?.ExtraMessage != null)
                Message = LogShell.ExtraMessage + Environment.NewLine + Message;
            if (LogShell?.SourceFile != null)
                Message = LogShell.SourceFile + ": " + Message;
            if (LogShell?.Directory != null)
                Message = "DIRECTORY: " + LogShell.Directory + Environment.NewLine + Message; //If we get an error, should know what directory we're looking at. 
            CallerService.LogToDB(this, LogShell, Message, false, MessageType: "ERROR");

            profile = null;
            LogShell = null;
        }

        protected override string HandleAbort()
        {
            work.Clear();
            return null;
        }

        void CheckFolders(params string[] directories)
        {
            string dir = LogShell.Directory;
            foreach (var f in directories)
            {                
                if (Directory.Exists(f)) //Shouldn't be necessary, but to be safe.
                    continue;
                LogShell.Directory = f; // If we run into an exception while trying to create directory, then this should be available for logging.
                Directory.CreateDirectory(f); //Also checks if it already exists first.
            }
            LogShell.Directory = dir; //Revert to original directory.
        }
        protected override void CheckWorkLoad()
        {
            if (Workload == 0)
            {
                using (var h = _Manager.GetBasicHelper(map))
                {
                    h.QualifiedProcedure = GRAB_WORK;
                    h.RetryOnDeadlock = true;
                    h.ExpectedReturnValue = 0;
                    work = _Manager.Execute(h).ToContentList<JobProfile>();
                }
            }
        }
        public JobProfile profile { get; private set; } = null;
        internal JobExecutionShell LogShell { get; private set; } = null;
        protected override void Work()
        {
            profile = work[0];
            LogShell = new JobExecutionShell(profile.JobProfileID.Value);
            LogShell.Directory = profile.RegistrationFolder;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Start Work: JobProfileID " + profile.JobProfileID);
#endif
            work.RemoveAt(0);
            bool invalid = false;
            DirectoryInfo di = new DirectoryInfo(profile.RegistrationFolder);
            if (!di.Root.Exists)
            {
                string root = di.Root.Name;
                if (root.Like("%:%") && !root.Like(@"\\%"))
                {
                    invalid = true;
                    //Should be a drive, on the local machine.
                }
                else
                {
                    LogInfo("Unable to access network path:" + profile.RegistrationFolder,
                            LogShell, 
                            true);
                    System.Threading.Thread.Sleep(5000);
                    return; //Root doesn't exist, but it's (probably) a UNC path. May just be connection issues. Skip
                }
            }
            
            if (invalid || !di.Exists)
            {
                if (!invalid)
                {
                    try
                    {
                        di.Create();
                        LogInfo("Created directory: " + profile.RegistrationFolder, LogShell);
                    }
                    catch(Exception e)
                    {
                        if (di.Exists) // Network hiccup?
                            return;
                        if(e.Message.Contains("device is not ready"))
                        {
                            LogInfo("Device is not ready for access....", LogShell, true);
                            System.Threading.Thread.Sleep(5000);
                            return;
                        }
                        LogShell.ExtraMessage = "Unable to create Directory:";
                        ExceptionLog(e);                        
                        invalid = true;
                    }
                }
                if (invalid)
                {
                    using (var h = _Manager.GetBasicHelper(map))
                    {
                        h.AddKey("@JobProfileID", profile.JobProfileID);
                        h.QualifiedProcedure = INVALID;
                        h.RetryOnDeadlock = true;
                        h.ExpectedReturnValue = 0;
                        _Manager.ExecuteNonQuery(h);
                    }                    
                    LogInfo("Invalid Directory Found: " + profile.RegistrationFolder, LogShell);
                }
                return; //Profile didn't exist or marked as invalid. No work, move on.
            }
            LogShell.Directory = null; //Finished validating Registration folder.

            string Registered = Path.Combine(profile.RegistrationFolder, REGISTERED_FOLDER_NAME); //successful registrartion
            if (!string.IsNullOrWhiteSpace(profile.RegistrationDestinationFolder))
                Registered = profile.RegistrationDestinationFolder;

            string Rejected = Path.Combine(profile.RegistrationFolder, REJECTED_FOLDER_NAME); //Hash in use for profile or something maybe?
            string Duplicate = Path.Combine(profile.RegistrationFolder, DUPLICATE_FOLDER_NAME); //Name match

            CheckFolders(Rejected, Duplicate);
            bool masked = true;
            if (!Registered.Contains("<"))
            {
                CheckFolders(Registered);
                masked = false;
            }

            string[] filterSet = profile.FileFilter.Split(';');
            string[] exclusionFilterSet = profile.FileExclusionFilter?.Split(';');

            foreach(string filter in filterSet)
            {
                ProcessFilter(di, filter, exclusionFilterSet, masked, Registered, Duplicate, Rejected);
            }                       
        }
        public void ProcessFilter(DirectoryInfo di, string FileFilter, string[] ExclusionFilterSet, bool masked, string Registered, string Duplicate, string Rejected)
        {
            DateTime cutover = DateTime.Now.AddMinutes(CUTOVER);
            var fileList = di.GetFiles(FileFilter);
            List<RegistrationFile> regList = new List<RegistrationFile>();
            fileList.ForEach(fi =>
            {
                if (ExclusionFilterSet != null 
                && ExclusionFilterSet.Exists(ef =>
                 {
                     string check = ef.Replace('*', '%');
                     return fi.Name.Like(check);
                 }))
                {
                    return; //Essentially a continue.
                }
                fi.Refresh();

                if (fi.LastWriteTime > cutover) //LastModified
                {                    
                    System.Diagnostics.Debug.WriteLine($"File '{fi.Name}' was written to at {fi.LastWriteTime}. Skip");
                    return;
                }
                if (fi.CreationTime > cutover) //Some FTP clients may set the CreationTime instead of the Modified time when they finish pushing
                    return;

                regList.Add(new RegistrationFile(profile, fi, true));

            });

            int delay = Workload > 0 ? 5000 : 15000; //if this is the only profile, wait at least 15 seconds before trying again. Otherwise, wait at least 5 seconds

            if (fileList.HasMinimumCount(2))
                delay /= fileList.Length.MinOfComparison(5); //Divide the delay so that it's a spread out across the files, up to 5. (That is, if there are 6 files, we would still divide 5000 into 5: 1 s delay per *file that cannot be accessed due to already being in use*)

            regList.OrderBy(reg => reg.FileDate).ForEach((reg) =>
            {
                LogShell.SourceFile = reg.FileName;
                string localRegistered = Registered;
                if (masked)
                {
                    localRegistered = localRegistered.ToUpper()
                        .Replace("<YYYY>", reg.FileDate.Year.ToString())
                        .Replace("<YY>", reg.FileDate.Year.ToString().Substring(2, 2))
                        .Replace("<CCYY>", reg.FileDate.Year.ToString())
                        .Replace("<CC>", reg.FileDate.Year.ToString().Substring(0, 2))
                        .Replace("<MM>", reg.FileDate.Month.ToString().PadLeft(2, '0'))
                        .Replace("<M>", reg.FileDate.Month.ToString())
                        .Replace("<DD>", reg.FileDate.Day.ToString().PadLeft(2, '0'))
                        .Replace("<D>", reg.FileDate.Day.ToString()); //Simple date masking.
                    CheckFolders(localRegistered);
                }
                reg.FileAlreadyInUse_SleepTime = delay;

                if (File.Exists(reg.FilePath)) //Need to make sure File still exists first.
                    ProcessRegistrationFile(reg, localRegistered, Duplicate, Rejected, profile);
            });
        }

        public void ProcessRegistrationFile(RegistrationFile reg, string Registered, string Duplicate, string Rejected, JobProfile profile)
        {
            string success = Path.Combine(Registered, reg.FileName);                        
            if (File.Exists(success))
            {
                var dupe = Path.Combine(Duplicate, reg.FileName);
                if(File.Exists(dupe))
                    dupe += DateTime.Now.ToString("_yyyyMMdd_hhmmss"); //Match Rejected folder logic. Only timestamp if file already exists at destination. Easier to cleanup this way as well, since we don't have to rename the first time if it shouldn't have been marked as duplicate

                reg.QueueLoggingDataRow(_Manager, Registered, dupe, false, true); //If proc errors, do it before moving the file so that we don't miss logging.
                File.Move(reg.FilePath, dupe);
                //Loging the duplicate file details to [SEIDR].[QueueRejection].
                LogInfo($"Job Profile {profile.JobProfileID}, '{reg.FileName}' - moved to duplicate (" + Duplicate + ").",
                    LogShell, 
                    true);
                return;
            }
            string fail = Path.Combine(Rejected, reg.FileName);
            if (File.Exists(fail))
            {
                fail += DateTime.Now.ToString("_yyyyMMdd_hhmmss");
            }
            LogInfo($"Job Profile {profile.JobProfileID}, '{reg.FileName}' - Attempt Register",
                    LogShell, 
                    true);
            
            
            var j = reg.RegisterDataRow(_Manager, success, fail).ToContentRecord<JobExecutionDetail>();
            if (j != null)
            {
                LogShell = new JobExecutionShell(profile.JobProfileID.Value, j.JobProfile_JobID, j.JobExecutionID);
                CallerService.QueueExecution(j);
                LogInfo($"Job Profile {profile.JobProfileID}, '{reg.FileName}' - Queued for execution. JobExecutionID: {j.JobExecutionID}.",
                    LogShell, 
                    true);
            }
            else if (reg.Rejected.HasValue) //Note: if Rejected is null, then the file has been left alone so that it can try registering again later
            {
                //Loging the rejected file details to [SEIDR].[QueueRejection].
                if (reg.Rejected.Value)
                    reg.QueueLoggingDataRow(_Manager, Registered, fail, true, false);
                
                LogInfo($"Job Profile {profile.JobProfileID}, '{reg.FileName}' - Registration Result: "
                    + (reg.Rejected.Value ? "REJECTED" : "REGISTERED"),
                    LogShell, 
                    true);
            }
        }


#if DEBUG
        #region Testing

        public void TestWork(JobProfile v)
        {
            if (v != null)
                work.Insert(0, v);
            else
                CheckWorkLoad();           
            while (this.Workload > 0)
            {
                try
                {
                    Work();
                }
                catch (Exception ex)
                {
                    ExceptionLog(ex);
                }
            }
        }
        #endregion
#endif
    }
}
