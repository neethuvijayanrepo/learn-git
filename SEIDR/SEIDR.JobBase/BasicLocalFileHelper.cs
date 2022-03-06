using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    public class BasicLocalFileHelper
    {

        /// <summary>
        /// Indicates whether or not <see cref="Finish"/> has been called.
        /// </summary>
        public bool Finished { get; private set; } = false;
        /// <summary>
        /// Indicates that the Working File has been started and is not finished.
        /// </summary>
        public bool Working => !Finished && File.Exists(WorkingFilePath);

        /// <summary>
        /// The path for the file being processed.
        /// </summary>
        public string WorkingFilePath { get; private set; }

        /// <summary>
        /// Implicitly treats the Object as it's ToString representation, since the LocalFileHelper is about managing the File's Path, as well as its state.
        /// </summary>
        /// <param name="file"></param>
        public static implicit operator string(BasicLocalFileHelper file) => file.ToString();

        /// <summary>
        /// Depending on the state of the LocalFileHelper object, returns either <see cref="WorkingFilePath"/> (<see cref="Finished"/> = False), or <see cref="OutputFilePath"/> (<see cref="Finished"/> = True)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Finished ? OutputFilePath : WorkingFilePath;
        }
        /// <summary>
        /// Output Directory for file.
        /// </summary>
        public string OutputDirectory { get; set; }
        /// <summary>
        /// Final file path
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// Combines OutputDirectory/OutputFileName.
        /// </summary>
        public string OutputFilePath
        {
            get { return Path.Combine(OutputDirectory, OutputFileName); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Attempted to pass empty or null string to OutputFilePath.");
                OutputDirectory = Path.GetDirectoryName(value);
                OutputFileName = Path.GetFileName(value);
            }
        }
        /// <summary>
        /// The directory of the Working File Path. Will use <see cref="DefaultWorkingDirectory"/>, unless overwritten in an inheriting class.
        /// </summary>
        public string WorkDirectory { get; private set; }
        /// <summary>
        /// Default directory to use for working.
        /// <para>Will either use a subdirectory of the job folder, or a directory specified by a config setting for 'WorkFolder' </para>
        /// </summary>
        public static readonly string DefaultWorkingDirectory;

        static BasicLocalFileHelper()
        {
            //Should never be null, because this is the same key used by the service as the location to pick up the DLL that we're running from.
            string jobFolder = ConfigurationManager.AppSettings["JobLibrary"];
            string workFolder = ConfigurationManager.AppSettings["WorkFolder"];
            if (workFolder != null)
                DefaultWorkingDirectory = workFolder;
            else
                DefaultWorkingDirectory = Path.Combine(jobFolder,"__WORKING__");
            //ClearRootWorkingDirectory();
        }

        protected BaseContext Context { get; private set; }
        private JobExecution Execution => Context.Execution;
        /// <summary>
        /// Base Constructor with parameter - used for <see cref="ContextJobBase{T}"/> 
        /// </summary>
        /// <param name="context"></param>
        public BasicLocalFileHelper(BaseContext context)
        {
            Context = context;

            _prefix = "_" + context.JobExecutionID + "_";
            WorkingFilePath = Path.Combine(WorkDirectory, _prefix + ".TEMP");

            var folderSync = Context.GetSyncObject();
            lock (folderSync)
            {
	            if (!Directory.Exists(WorkDirectory))
	                Directory.CreateDirectory(WorkDirectory);
	            if (File.Exists(WorkingFilePath))
	                File.Delete(WorkingFilePath);
	        }
	        
        }
        
        public BasicLocalFileHelper()
        {
            WorkDirectory = DefaultWorkingDirectory;
        }

        public static string GetWorkDirectory(BaseContext context, bool subFolder = true)
        {
            string contextType = context.GetType().Name.Replace("Context", string.Empty);
            string workDir;
            if (subFolder)
                workDir = Path.Combine(DefaultWorkingDirectory,
                                       contextType,
                                       string.Format(SUBFOLDER_FORMAT, context.JobExecutionID));
            else
                workDir = Path.Combine(DefaultWorkingDirectory, contextType);
            return workDir;
        }
        public virtual void Setup(BaseContext context) { }
        /// <summary>
        /// Placeholder method that could potentially be used to override working directory or other settings.
        /// </summary>
        public void Init(BaseContext context, bool subFolder = false)
        {
            OutputDirectory = context.DefaultOutputDirectory;
            Setup(context); //Allow setup to override output Directory if appropriate. (since that's just a default)
            Context = context;

            if(_prefix == null)
                _prefix = "_" + context.JobExecutionID + "_";
            WorkDirectory = GetWorkDirectory(context, subFolder);

            WorkingFilePath = Path.Combine(WorkDirectory, _prefix + ".TEMP");

            var folderSync = Context.GetSyncObject();
            lock (folderSync) 
            { 
	            if (!Directory.Exists(WorkDirectory))
	                Directory.CreateDirectory(WorkDirectory);
	            if (File.Exists(WorkingFilePath))
                	File.Delete(WorkingFilePath);
            }

            SubFolderMode = subFolder;
        }

        private string _prefix;
        public bool SubFolderMode { get; private set; }

        public void SwitchSubFolderMode()
        {
            string newFilePath = null;
            if (SubFolderMode)
            {
                DirectoryInfo di = new DirectoryInfo(WorkDirectory);
                WorkDirectory = di.Parent.FullName;
            }
            else
            {
                WorkDirectory = Path.Combine(WorkDirectory, $"__{Context.JobExecutionID}__");
            }
            if (WorkingFilePath == null) //NOTE: Shouldn't happen.
                return;
            newFilePath = Path.Combine(WorkDirectory, Path.GetFileName(WorkingFilePath));
            if (File.Exists(WorkingFilePath))
            {
                var folderSync = Context.GetSyncObject();
                lock (folderSync)
	                File.Move(WorkingFilePath, newFilePath);
            }
            WorkingFilePath = newFilePath;
        }

        /// <summary>
        /// Stamps the working file path with the current timestamp at the end of its name, to prevent issues with processing
        /// <para>Two different files with the same name.</para>
        /// </summary>
        public void TimeStamp()
        {
            string workingNew = this.WorkingFilePath + DateTime.Now.ToString("_yyyyMMdd_HHmmss_ffff");
            File.Move(WorkingFilePath, workingNew);
            WorkingFilePath = workingNew;
        }

        public void SetFromJobExecution()
        {
            if (string.IsNullOrEmpty(Context.FilePath) || !File.Exists(Context.FilePath))
            {
                throw new InvalidOperationException("No file found for JobExecution's Current FilePath");
            }
            File.Copy(Context.FilePath, WorkingFilePath);
            //new files going into directory should not be a sync issue, since they're prepended with JobExecutionID info..
        }
        /// <summary>
        /// Allow pointing the object to a different file Name in the same directory. (E.g., a cleaned/scrubbed version of a file being worked with)
        /// <para>NOTE: does not validate that the target file exists yet.</para>
        /// <para>Also, the provided file name may be changed before assigning to the working path.</para>
        /// </summary>
        /// <param name="newFileName"></param>
        public void SetWorkingFileName(string newFileName)
        {
            if (!newFileName.StartsWith(_prefix))
                newFileName = _prefix + newFileName;
            string temp = Path.Combine(WorkDirectory, newFileName);

            WorkingFilePath = temp;
            Finished = false;
        }

        public void Rename(string newFileName, bool overwrite = false)
        {

            _prefix = _prefix ?? "_" + Context.JobExecutionID + "_";
            if (!newFileName.StartsWith(_prefix))
                newFileName = _prefix + newFileName;
            string newPath = Path.Combine(WorkDirectory, newFileName);


            var folderSync = Context.GetSyncObject();
            lock (folderSync)
            {
	            if (overwrite && File.Exists(newPath))
	                File.Delete(newPath);
            	File.Move(WorkingFilePath, newPath);
            }

            WorkingFilePath = newPath;
        }

        private const string SUBFOLDER_FORMAT = "__{0}__";
        public void Init(BaseContext context, string sourceFile, bool subFolder = false)
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
                throw new ArgumentException("Invalid Source", nameof(sourceFile));
            OutputDirectory = context.DefaultOutputDirectory;
            Setup(context);
            Context = context;

            //Default

            if (_prefix == null)
                _prefix = "_" + context.JobExecutionID + "_";
            OutputFileName = Path.GetFileName(sourceFile);

            WorkDirectory = GetWorkDirectory(context, subFolder);

            string tempName = _prefix
                              + Path.GetFileNameWithoutExtension(sourceFile)
                              + ".TEMP";
            WorkingFilePath = Path.Combine(WorkDirectory, tempName);

            var folderSync = Context.GetSyncObject();
            lock (folderSync)
            {
	            if (!Directory.Exists(WorkDirectory))
	                Directory.CreateDirectory(WorkDirectory);
	            if (File.Exists(WorkingFilePath))
	                File.Delete(WorkingFilePath);
            	File.Copy(sourceFile, WorkingFilePath);
            }
            SubFolderMode = subFolder;
            
        }
        
        public static void ClearRootWorkingDirectory()
        {
            if (!Directory.Exists(DefaultWorkingDirectory))
                return;
            Directory.Delete(DefaultWorkingDirectory, true);
        }
        /// <summary>
        /// Delete any files associated with the current JobExecution that are in the working folder(s) (Main for context, or specific to JobExecution).
        /// </summary>
        public static void ClearWorkingDirectory(BaseContext Context)
        {
            if (!Directory.Exists(DefaultWorkingDirectory))
                return;

            string contextType = Context.GetType().Name.Replace("Context", string.Empty);
            string contextFolder = Path.Combine(DefaultWorkingDirectory, contextType);
            if (!Directory.Exists(contextFolder))
                return;
            string executionFolderName = string.Format(SUBFOLDER_FORMAT, Context.JobExecutionID);
            string subFolder = Path.Combine(contextFolder, executionFolderName);

            var folderSync = Context.GetSyncObject();
            int counter = 0;
            lock (folderSync)
            {
	            if (Directory.Exists(subFolder))
	            {
	                Directory.Delete(subFolder, true);
	            }
	            string pattern = $"_{Context.JobExecutionID}_*.*"; //Working files generated by processes may have other extensions.
	            var files = Directory.GetFiles(contextFolder, pattern);
	            foreach (string path in files)
	            {
	                if (File.Exists(path))
	                {
	                    File.Delete(path);
	                    counter++;
	                }
	            }
            }
            if (counter > 0)
                Context.LogInfo($"Deleted {counter} temp files from Working directory.");
            
        }

        /// <summary>
        /// Initializes and returns a <see cref="Doc.DocMetaData"/> instance that can be used with a <see cref="Doc.DocWriter"/> to create content in the working file.
        /// </summary>
        /// <returns></returns>
        public Doc.DocMetaData GetDocMetaData()
        {
            if (!Working)
                return null;
            var md= new Doc.DocMetaData(WorkingFilePath)
                .SetMultiLineEndDelimiters("\r", "\n", "\r\n");
            md.Columns.TextQualifier = "\""; //default to having quote as text qualifier.
            return md;
        }
        /// <summary>
        /// Initializes and returns a <see cref="Doc.DocMetaData"/> instance that can be used with a <see cref="Doc.DocWriter"/> to create content in the working file.
        /// <para>Specifies the delimiter and any columns.</para>
        /// </summary>
        /// <returns></returns>
        public Doc.DocMetaData GetDocMetaData(char delimiter, params string[] columns)
        {
            if (!Working)
                return null;
            var md = new Doc.DocMetaData(WorkingFilePath);
            md.SetDelimiter(delimiter)
              .SetMultiLineEndDelimiters("\r", "\n", "\r\n") //Allow ease of working with any of the main delimiters, or a combination.
              .AddDelimitedColumns(columns)
              .Columns.TextQualifier = "\"";
            return md;
        }


        /// <summary>
        /// Moves the working file from the temporary location to the Output location, and sets information in the job execution originally passed.
        /// <para>If inheriting, can call additional logic before/after this logic. (e.g., Checkpoint after file has been moved)</para>
        /// </summary>
        public virtual void Finish(bool OverWrite = false)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BasicLocalFileHelper));
            if (Finished)
                throw new InvalidOperationException($"{nameof(Finish)}() has already been called. ('{nameof(Finished)}' = True)");
            if (!Working)
                throw new InvalidOperationException("Have not started working on Working File - does not exist. Cannot finish.");
            if (string.IsNullOrWhiteSpace(OutputDirectory))
                throw new InvalidOperationException($"{nameof(OutputDirectory)} has not been set.");
            if (string.IsNullOrWhiteSpace(OutputFileName))
                throw new InvalidOperationException($"{nameof(OutputFileName)} has not been set.");

            DirectoryInfo di = new DirectoryInfo(OutputDirectory);
            if (!di.Exists)
                di.Create();
            if (OverWrite && File.Exists(OutputFilePath))
                File.Delete(OutputFilePath);
            File.Move(WorkingFilePath, OutputFilePath);
            const string SET_PATH_MESSAGE = " - Setting as JobExecution FilePath.";
            Context.LogInfo($"Move working file from '{WorkingFilePath}' to '{OutputFilePath}' {(SetExecutionFileInfo ? SET_PATH_MESSAGE : string.Empty)}");
            //this should (almost) always apply to Export jobs but not necessarily to all jobs
            if (SetExecutionFileInfo)
	            Execution.SetFileInfo(OutputFilePath);
            Finished = true;
        }

        public bool Disposed { get; private set; }

        /// <summary>
        /// Dispose of the working file. Marks the 
        /// </summary>
        public virtual void Dispose()
        {
            if (Finished || Disposed)
                return;
            if (File.Exists(WorkingFilePath))
                File.Delete(WorkingFilePath);

            Disposed = true;
        }
        /// <summary>
        /// If true, sets the JobExecution file path information to match the output file upon successful call to <see cref="Finish(bool)"/>
        /// <para>Note that the JobExecution filepath may still be updated to match if <see cref="BaseContext.WorkingFile"/> is set.</para>
        /// </summary>
        public bool SetExecutionFileInfo { get; set; } = false;
    }
}
