using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;

namespace SEIDR.JobBase
{
    public abstract class BaseContext
    {
        private static readonly object ContextSyncLock = new object();
        private static readonly Dictionary<string, object> ContextSync = new Dictionary<string, object>();

        /// <summary>
        /// Get an object intended to use for Locking/Syncing.
        /// <para>Intended use: lock folder access for moving files in/out of the working directory to avoid stepping
        /// on other threads working in the same directory with similar file names.</para>
        /// </summary>
        /// <returns></returns>
        public object GetSyncObject()
        {
            string context = GetType().Name;
            return GetSyncObject(context);
        }
        public object GetSyncObject(string context)
        {

            lock (ContextSyncLock)
            {
                if (ContextSync.ContainsKey(context))
                    return ContextSync[context];
                var obj = new object();
                ContextSync.Add(context, obj);
                return obj;
            }
        }

        /// <summary>
        /// Allow initializing another type of context. E.g, for calling a file system job from an export job.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InitializeOtherContext<T>()where T: BaseContext, new()
        {
            T other = new T();
            other.Init(Executor, Execution);
            return other;
        }

        /// <summary>
        /// Allow forcing a current JobExecution to go to Complete = 1.
        /// <para>May be useful if creating Child Executions to take over from the following step?</para>
        /// </summary>
        /// <param name="success"></param>
        public virtual void Complete(bool success = true)
        {
            ResultStatus = new ExecutionStatus
            {
                ExecutionStatusCode = success
                                          ? ExecutionStatus.COMPLETE
                                          : ExecutionStatus.FAILURE_STOP,
                NameSpace = nameof(SEIDR),
                IsError = !success,
                IsComplete = true
            };
            // Consideration - throw an exception to force immediate exit? 
            // Probably not, because the job could still need to do clean up
            // Counter Point - best practice would probably be to set the status at the end anyway.
            // Once we decide that we're totally done, there shouldn't be any more to do.
        }
        /// <summary>
        /// Default output directory for any classes inheriting from <see cref="BasicLocalFileHelper"/> and that initialize with this Context.
        /// </summary>
        public string DefaultOutputDirectory { get; set; }
        public static implicit operator JobExecution(BaseContext context) => context.Execution;
        public static implicit operator JobProfile(BaseContext context) => context.Executor.job;
        public static implicit operator DateTime(BaseContext context) => context.Execution.ProcessingDate;
        public static implicit operator DatabaseManager(BaseContext context) => context.Manager;
        public DateTime ProcessingDate => Execution.ProcessingDate;
        /// <summary>
        /// Sets the default completion status for success, using <see cref="ExecutionStatus.STEP_COMPLETE"/>
        /// or <see cref="ExecutionStatus.FAILURE"/>, depending on <paramref name="success"/>
        /// <para>Note: suggested to also have another implementation of this method but with an enum input parameter.</para>
        /// </summary>
        public virtual void SetStatus(bool success)
        {
            ResultStatus = new ExecutionStatus
            {
                ExecutionStatusCode = success
                                        ? ExecutionStatus.STEP_COMPLETE
                                        : ExecutionStatus.FAILURE,
                NameSpace = nameof(SEIDR),
                IsError = !success,
                IsComplete = false
            };
        }
        bool _Success = true;
        /// <summary>
        /// Success Property of Context.
        /// <para>If <see cref="ResultStatus"/> is null, will be used to call <see cref="SetStatus(bool)"/> at the end of processing.</para>
        /// </summary>
        public bool Success
        {
            get
            {
                if (ResultStatus != null)
                    return !ResultStatus.IsError;
                return _Success;
            }
            set { _Success = value; }
        }
        /// <summary>
        /// Check if the Result Status of current executing process is an error.
        /// <para>Opposite of <see cref="Success"/></para>
        /// </summary>
        public bool Failure
        {
            get
            {
                if (ResultStatus != null)
                    return ResultStatus.IsError;
                return !_Success;
            }
        }
        /// <summary>
        /// Result status to return values to the Executor.
        /// <para>Suggested use: add a method to the implementation which allows converting an Enum to an ExecutionStatus.</para>
        /// </summary>
        public ExecutionStatus ResultStatus { get; set; }
        public IJobExecutor Executor { get; private set; }
        public JobExecution Execution { get; private set; }
        public JobProfile Profile => Executor.job;
        public DatabaseManager Manager => Executor.Manager;
        public bool RequeueRequested { get; private set; } = false;

        public void Requeue(int delayMinutes)
        {
            Executor.Requeue(delayMinutes);
            RequeueRequested = true;
        }

        public void Init(IJobExecutor caller, JobExecution execution)
        {
            Executor = caller;
            Execution = execution;
            ExecutionWorkDirectory = BasicLocalFileHelper.GetWorkDirectory(this, true);
        }
        /// <summary>
        /// A work directory specific to this JobExecution/job context.
        /// <para>Initialized during <see cref="Init(IJobExecutor, JobExecution)"/> </para>
        /// </summary>
        public string ExecutionWorkDirectory { get; private set; } = null;

        public long JobExecutionID => Execution.JobExecutionID ?? 0; //Should never be null, but default to 0 (e.g., unit test)
        public int JobProfile_JobID => Execution.JobProfile_JobID;
        /// <summary>
        /// Job Execution's branch.
        /// <para>NOTE: The default branch for new job executions is 'MAIN'</para>
        /// </summary>
        public string Branch => Execution.Branch;
        private readonly object logLock = new object();

        public void LogInfo(string message)
        {
            lock(logLock)
                Executor.LogInfo(message);
        }

        public void LogError(string message, Exception ex = null, int? ExtraID = null)
        {
            lock(logLock)
                Executor.LogError(message, ex, ExtraID);
        }
        public string FileName => Execution.FileName;
        /// <summary>
        /// FilePath associated with the job execution
        /// </summary>
        public string FilePath => Execution.FilePath;
        /// <summary>
        /// Hash for the file associated with the JobExecution
        /// </summary>
        public string FileHash => Execution.FileHash;
        /// <summary>
        /// Size of the file associated with the JobExecution
        /// </summary>
        public long? FileSize => Execution.FileSize;
        /// <summary>
        /// Indicates whether or not the JobExecution is being called from an error handling context.
        /// <para>E.g., if we tried to download a file X times, then failed, gave up, and decided to generate a dummy file for in the meantime.</para>
        /// </summary>
        public bool IsError => Execution.IsError;

        /// <summary>
        /// FilePath associated with the current context.
        /// <para>Can either be the JobExecution's associated file, or the working file's path (if that has been set).</para>
        /// </summary>
        public string CurrentFilePath => WorkingFile ?? Execution.FilePath;

        public string CurrentFileName
        {
            get
            {
                if (WorkingFile != null)
                {
                    return Path.GetFileName(WorkingFile);
                }

                return Execution.FileName;
            }
        }

        public BasicLocalFileHelper WorkingFile { get; set; }
        /// <summary>
        /// If the working file is set, but the file does not exist, clear the working file property.
        /// <para>NOTE: If the WorkingFile is already marked as Finished, will skip checking whether the file exists.</para>
        /// </summary>
        public void ClearUnusedWorkingFile()
        {
            if (WorkingFile == null || WorkingFile.Finished)
                return;
            if (System.IO.File.Exists(WorkingFile))
                return;
            System.Diagnostics.Debug.WriteLine($"Clearing Context WorkingFile due to not being used. ( File Doesn't exist at Path '{WorkingFile.WorkingFilePath}')");
            WorkingFile = null;
        }
        /// <summary>
        /// Gets a LocalFile Helper and sets it as the WorkingFile.
        /// <para>It is not set based on an existing file.</para>
        /// </summary>
        /// <param name="subFolder"></param>
        public BasicLocalFileHelper SetWorkingFile(bool subFolder = DEFAULT_SUBFOLDER_MODE)
        {
            if (WorkingFile != null)
                throw new InvalidOperationException("Working File Has already been set.");
            WorkingFile = GetLocalFile(subFolder);
            return WorkingFile;
        }

        /// <summary>
        /// Updates the FileInformation on the underlying JobExecution
        /// </summary>
        /// <param name="filePath"></param>
        public void UpdateFilePath(string filePath)
        {
            Execution.SetFileInfo(filePath);
        }

        public void SetCurrentFilePath(string filePath)
        {
            if (WorkingFile != null)
            {
                if (!filePath.StartsWith(BasicLocalFileHelper.DefaultWorkingDirectory))
                {
                    throw new InvalidOperationException("Must set filePath to a file in the working directory when using a local file.");
                }
                string fileName = Path.GetFileName(filePath);
                if (WorkingFile.Finished)
                {
                    WorkingFile = ReserveBasicLocalFile(fileName);
                }
                WorkingFile.SetWorkingFileName(fileName);
                return;
            }
            UpdateFilePath(filePath);
        }
        const bool DEFAULT_SUBFOLDER_MODE = true;
        /// <summary>
        /// Gets a Local File Helper based on the file path associated with the JobExecution
        /// </summary>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public BasicLocalFileHelper GetExecutionLocalFile(bool subFolder = DEFAULT_SUBFOLDER_MODE)
        {
            var loc = GetLocalFile<BasicLocalFileHelper>(Execution.FilePath, subFolder);
            loc.OutputDirectory = Path.GetDirectoryName(Execution.FilePath); //Default output directory for execution, regardless of context Default.
            return loc;
        }

        /// <summary>
        /// Gets a basic local file helper, using the specified file as the source.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public BasicLocalFileHelper GetLocalFile(string sourceFile, bool subFolder = DEFAULT_SUBFOLDER_MODE)
        {
            return GetLocalFile<BasicLocalFileHelper>(sourceFile, subFolder);
        }
        /// <summary>
        /// Gets a Basic Local File Helper
        /// </summary>
        /// <returns></returns>
        public BasicLocalFileHelper GetLocalFile(bool subFolder = DEFAULT_SUBFOLDER_MODE)
        {
            return GetLocalFile<BasicLocalFileHelper>(subFolder);
        }
        /// <summary>
        /// Gets a basic local file helper using the specified file path as the source file.
        /// <para>The source file will be copied to the working directory for working with.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceFile"></param>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public T GetLocalFile<T>(string sourceFile, bool subFolder = DEFAULT_SUBFOLDER_MODE) where T : BasicLocalFileHelper, new()
        {
            if (String.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
                throw new ArgumentException("Must provide a valid source.", nameof(sourceFile));
            T local = new T();
            local.Init(this, sourceFile, subFolder);
            return local;
        }
        /// <summary>
        /// Gets a basic local file helper with a name reserved, but does not actually copy any file or put anything into
        /// <para>the working directory. Useful for creating an output file in the working directory before moving it to a Network location.</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public BasicLocalFileHelper ReserveBasicLocalFile(string fileName, bool subFolder = DEFAULT_SUBFOLDER_MODE) 
            => ReserveLocalFile<BasicLocalFileHelper>(fileName, subFolder);
     
        /// <summary>
        /// Gets a basic local file helper with a name reserved, but does not actually copy any file or put anything into
        /// <para>the working directory. Useful for creating an output file in the working directory before moving it to a Network location.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public T ReserveLocalFile<T>(string fileName, bool subFolder = DEFAULT_SUBFOLDER_MODE) where T : BasicLocalFileHelper, new()
        {
            string dir = Path.GetDirectoryName(fileName);
            if (String.IsNullOrEmpty(dir))
            {
                dir = null;
            }
            else
                fileName = Path.GetFileName(fileName); //Allow reserving full filepath.

            T local = new T();
            local.Init(this, subFolder);
            local.SetWorkingFileName(fileName);
            if(dir != null)
                local.OutputDirectory = dir;

            local.OutputFileName = fileName; //Default. Does not have directory anymore, if passed a full path

            return local;
        }
        /// <summary>
        /// Gets a local file helper using the specified working directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetLocalFile<T>(bool subFolder = DEFAULT_SUBFOLDER_MODE) where T : BasicLocalFileHelper, new()
        {
            T local = new T();
            local.Init(this, subFolder);
            return local;
        }
        /// <summary>
        /// Clears the FilePath associated with the underlying JobExecution.
        /// </summary>
        public void ClearFilePath()
        {
            Execution.SetFileInfo(null as string);
        }
        /// <summary>
        /// Updates the file Information on the underlying JobExecution
        /// </summary>
        /// <param name="file"></param>
        public void UpdateFilePath(FileInfo file)
        {
            Execution.SetFileInfo(file);
        }
        /// <summary>
        /// Gets the last checkpoint that was logged by this JobExecution, or null.
        /// </summary>
        /// <returns></returns>
        public JobExecutionCheckPoint GetLastCheckPoint() => Executor.GetLastCheckPoint();
        /// <summary>
        /// Calls the Checkpoint logic on the underlying JobExecutor.
        /// </summary>
        /// <param name="checkPointNumber"></param>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public JobExecutionCheckPoint LogCheckPoint(int checkPointNumber, string message = null, string key = null)
            => Executor.LogCheckPoint(checkPointNumber, message, key);
    }
}
