using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR;
using SEIDR.Doc;
using SEIDR.DataBase;

namespace SEIDR.JobBase
{
    public class RegistrationFile
    {
        public static bool NO_REGISTER_MODE = false;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void SetNoRegisterMode()
        {
            NO_REGISTER_MODE = true;
        }
        /// <summary>
        /// Number of miliseconds to make the thread sleep if a file cannot be registered due to an IO Exception when the file is being used by another process
        /// </summary>
        public int FileAlreadyInUse_SleepTime = 5000;


        const string REGISTER_SPROC = "SEIDR.usp_Job_RegisterFile";
        const string QUEUE_LOGGING_SPROC = "SEIDR.usp_Job_QueueLogging";
        public int JobProfileID { get; private set; }
        public string FilePath { get; private set; }
        public string FileName => System.IO.Path.GetFileName(FilePath);
        public string FileHash { get; private set; }
        public long FileSize { get; private set; }
        DateTime _FileDate;
        public DateTime FileDate => _FileDate;
        int _StepNumber = 1;
        public int StepNumber
        {
            get { return _StepNumber; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(StepNumber), "Value must be > 0");
                _StepNumber = value;
            }
        }

        public string RegistrationPath { get; private set; }

        public bool IsRejected { get; private set; }

        public bool IsDuplicate { get; private set; }

        public bool? Rejected { get; private set; } = null;
        public bool QueueAfterRegister { get; private set; } = false;
        public RegistrationFile(JobProfile profile, System.IO.FileInfo file, bool queueAfterRegister = false)
        {
            JobProfileID = profile.JobProfileID.Value;
            FilePath = file.FullName;
            FileSize = file.Length;
            _FileDate = file.CreationTime.Date;
            FileHash = file.GetFileHash();
            QueueAfterRegister = queueAfterRegister;
            bool parsable = !string.IsNullOrWhiteSpace(profile.FileDateMask);

            if(parsable)
                parsable = file.Name.ParseDateRegex(profile.FileDateMask, ref _FileDate);

            if (!parsable
                || !CheckSQLDateValid(_FileDate)
                || _FileDate > DateTime.Today.AddMonths(3) //Parsing issue... Should not be getting data that far into the future. Ex: Client has mutliple files that are <MM><DD><YY><HH><mm> format, but some that are also <MM><DD><Y><HH><mm> format...we don't care about hours/minutes, so the latter is garbage and leads to the first part of the hour being treated as part of the <YY>. No good way to treat, so just use file date if we don't get a good parse.
                )
            {
                _FileDate = file.CreationTime.Date;
            }
        }
        /// <summary>
        /// Registers the file without moving or copying it.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public JobExecution Register(DatabaseManager manager)
            => Register(manager, null, null, true);
        public JobExecution CopyRegister(DatabaseManager manager, string SuccessFilePath, string FailureFilePath)
            => Register(manager, SuccessFilePath, FailureFilePath, true);
        /// <summary>
        /// Registers this file as a new JobExecution under it's JobProfile
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="SuccessFilePath"></param>
        /// <param name="FailureFilePath"></param>
        /// <returns></returns>
        public JobExecution MoveRegister(DatabaseManager manager, string SuccessFilePath, string FailureFilePath)
            => Register(manager, SuccessFilePath, FailureFilePath, false);
        private JobExecution Register(DatabaseManager manager,
            string SuccessFilePath, string FailureFilePath, bool copyMode)
            => RegisterDataRow(manager, SuccessFilePath, FailureFilePath, copyMode).ToContentRecord<JobExecution>();
        public System.Data.DataRow RegisterDataRowCopy(DatabaseManager manager, string sucessFilePath, string FailureFilePath)
            => RegisterDataRow(manager, SuccessFilePath: sucessFilePath, FailureFilePath: FailureFilePath, copyMode: true);
        public System.Data.DataRow RegisterDataRow(DatabaseManager manager, string sucessFilePath, string FailureFilePath)
            => RegisterDataRow(manager, SuccessFilePath: sucessFilePath, FailureFilePath: FailureFilePath, copyMode: false);

        private void FileMove(string PathCheck, bool CopyMode)
        {
            if(FilePath !=PathCheck && !string.IsNullOrWhiteSpace(PathCheck))
            {
                if (CopyMode)
                    System.IO.File.Copy(FilePath, PathCheck, true);
                else
                    System.IO.File.Move(FilePath, PathCheck);
            }
        }
        private System.Data.DataRow RegisterDataRow(DatabaseManager manager,
            string SuccessFilePath, string FailureFilePath, bool copyMode)
        {
            if (NO_REGISTER_MODE)
            {
                System.Diagnostics.Debug.WriteLine("SKIPPING REGISTER CALL - NO REGISTER MODE");
                FileMove(SuccessFilePath, copyMode);
                return null;
            }

            using (var help = manager.GetBasicHelper(this, includeConnection: true))
            {
                help.QualifiedProcedure = REGISTER_SPROC;
                help.DoMapUpdate = false;
                if (!string.IsNullOrWhiteSpace(SuccessFilePath))
                    help[nameof(FilePath)] = SuccessFilePath;
                help.RetryOnDeadlock = true;

                help.BeginTran();
                var job = manager.Execute(help).GetFirstRowOrNull();
                bool Success = job == null ? help.ReturnValue == 0 : true;
                ReturnCode = help.ReturnValue;
                Rejected = !Success;
                try
                {
                    if (Success)
                    {
                        FileMove(SuccessFilePath, copyMode);
                    }
                    else
                    {
                        FileMove(FailureFilePath, copyMode);
                    }
                    help.CommitTran();
                }
                catch(Exception ex)
                {                    
                    Rejected = null;
                    ReturnCode = null;
                    help.RollbackTran();
                    if (ex is System.IO.IOException && ex.Message.Contains("because it is being used by another process"))
                    {
                        System.Threading.Thread.Sleep(FileAlreadyInUse_SleepTime); //Useful to log, but it can get spammed if a file is going to be in use for a while. So wait a bit of time to prevent spamming attempts on the file
                    }
                    throw;
                }
                return job;
            }
        }
        public int? ReturnCode { get; private set; } = null;
        public bool CheckSQLDateValid(DateTime check)
        {
            if (check.CompareTo(new DateTime(1770, 1, 1, 0, 0, 0, 0)) <= 0 || check.CompareTo(new DateTime(9000, 12, 30)) >= 0)
                return false;
            return true;
        }
        public static string CheckFileHash(string FilePath) => FilePath.GetFileHash();
        public static string CheckFileHash(System.IO.FileInfo file) => file.GetFileHash();

        public void QueueLoggingDataRow(DatabaseManager manager, string registeredPath, string filePath, bool isRejected, bool isDuplicate)
        {
            using (var help = manager.GetBasicHelper(this, includeConnection: true))
            {
                help.QualifiedProcedure = QUEUE_LOGGING_SPROC;
                help.DoMapUpdate = false;
                help[nameof(RegistrationPath)] = registeredPath;
                if (!string.IsNullOrWhiteSpace(filePath))
                    help[nameof(FilePath)] = filePath;

                help[nameof(IsRejected)] = isRejected;
                help[nameof(IsDuplicate)] = isDuplicate;
                help.RetryOnDeadlock = true;
                manager.Execute(help);                
            }
        }
    }
}
