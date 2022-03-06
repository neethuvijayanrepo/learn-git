using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobExecutor;
using SEIDR.DataBase;
using System.Configuration;
using SEIDR.JobBase;
using SEIDR;
using System.IO;
using JobUnitTest.MockData;
using SEIDR.Doc;

namespace JobUnitTest
{       
    public abstract class TestBase
    {
        public FileInfo TestObfuscateFileVersion(SEIDR.Doc.DocMetaData FileIn)
        {
            return TestObfuscateFileVersion(FileIn, new int[] { });
        }
        public FileInfo TestObfuscateFileVersion(SEIDR.Doc.DocMetaData FileIn, params int[] columnsIgnore)
        {
            FileIn.CanWrite = true;
            FileIn.Columns.AllowMissingColumns = true;
            string FilePath = FileIn.FilePath;
            Random r = new Random();
            using (var reader = new SEIDR.Doc.DocReader(FileIn))
            {
                var outMetaData = reader.MetaData.CloneForNewFile(FilePath + ".OBFUSC", true);
                var colSet = reader.MetaData.Columns.Where(c => !columnsIgnore.Contains(c.Position));
                using (var writer = new SEIDR.Doc.DocWriter(outMetaData))
                {
                    foreach(var record in reader)
                    {
                        foreach(var cv in colSet)
                        {
                            string work = record[cv];
                            if (string.IsNullOrWhiteSpace(work))
                                continue;
                            if (work.Like("[^a-zA-Z]+", false))
                            {
                                if (work.Like(@"[0-9]{4}\/[0-9]{1,2}\/[0-9]{1,2}", false))
                                    work = DateTime.Now.ToString("yyyy/MM/dd");
                                else if (work.Like(@"[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}", false))
                                    work = DateTime.Now.ToString("yyyy-MM-dd");
                                else if (work.Like(@"20[0-9]{6}", false))
                                    work = DateTime.Now.ToString("yyyyMMdd");
                                else
                                    work = r.Next().ToString();
                            }
                            else
                                work = Guid.NewGuid().ToString().Substring(0, 15);

                            record[cv] = work;
                        }
                        writer.AddRecord(record);
                    }                                        
                }
                return new FileInfo(outMetaData.FilePath);
            }
        }

        /// <summary>
        /// Use if you want to log to a file. A call to <see cref="PrepRootDirectory(bool)"/> will attempt to set this to the directoy specified by <see cref="MyRoot"/> by default.
        /// </summary>
        protected string LogFilePath
        {
            get
            {
                return _Executor.LogFilePath;
            }
            set
            {
                _Executor.LogFilePath = value;
            }
        }
        protected DatabaseConnection _Connection;
        protected DatabaseManager _Manager;
        protected JobExecutorService _Service;
        protected TestExecutor _Executor;
        /// <summary>
        /// Wrapper for TestExecution FilePath
        /// </summary>
        protected string TestPath
        {
            get { return _TestExecution.FilePath; }
            set { _TestExecution.SetFileInfo(value); }
        }
        protected bool IgnoreJobMetaData = false;
        /// <summary>
        /// For testing with a JobExecution. Initializes to null, but can be populated during <see cref="Init"/> 
        /// </summary>
        protected JobExecution _TestExecution;
        public TestBase()
        {
            string catalog = ConfigurationManager.AppSettings["TEST_CATALOG"] ?? "MIMIR";
            _Connection = new DatabaseConnection(ConfigurationManager.AppSettings["TEST_DATABASE"], catalog);
            _Manager = new DatabaseManager(_Connection, DefaultSchema: nameof(SEIDR));
            _Service = new JobExecutorService(_Manager, ConfigurationManager.AppSettings["LogRootDirectory"])
            {
                QueueThreadCount = 1,
                ExecutionThreadCount = 1,                
            };
            _Executor = new TestExecutor(_Connection);            
            _TestExecution = null;
            Init();
        }
        protected const string BASE_ROOT = @"C:\SEIDR";
     
        /// <summary>
        /// Set by calling <see cref="PrepRootDirectory(bool)"/> 
        /// <para>Note: <see cref="JobTestBase{T}.PrepRootDirectory(bool)"/> will set this to a folder specific to the Job of type T</para>
        /// </summary>
        protected DirectoryInfo MyRoot { get; set; }
        public virtual DirectoryInfo PrepSubfolder(string FolderName, bool Clean = true)
        {
            if (MyRoot == null)
                throw new NullReferenceException($"'{nameof(MyRoot)}' has not been populated by calling {nameof(PrepRootDirectory)} yet.");
            DirectoryInfo di = new DirectoryInfo(Path.Combine(MyRoot.FullName, FolderName));
            if(di.Exists)
            {
                if (Clean)
                {
                    di.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete()); //Clean out files
                    di.EnumerateDirectories().ForEach(d => d.Delete(true)); //Clean subdirectories.
                }                
            }
            else
            di.Create();
            return di;
        }
        public void CheckUserKey(string UserKey, string Description = null, string priority = null)
        {
            using (var h = _Manager.GetBasicHelper())
            {
                h.QualifiedProcedure = "TEST.usp_CheckUserKey";
                h[nameof(UserKey)] = UserKey;
                h[nameof(Description)] = Description;
                h[nameof(priority)] = priority;
                _Manager.Execute(h);
            }
        }
        /// <summary>
        /// Creates a profile in the database specified by <see cref="_Connection"/> and returns a JobProfile object. 
        /// <para>If the JobProfile already exists, then the method returns the first record.</para>
        /// <para>Internally calls <see cref="CreateProfile(string, string, DatabaseManagerHelperModel)"/>, but the Helper Model is created and closed within this method, and cannot be rolled back. </para>
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="UserKey"></param>
        /// <returns></returns>
        public JobProfile CreateProfile(string Description, string UserKey)
        {
            using (var h = _Manager.GetBasicHelper())
            {
                return CreateProfile(Description, UserKey, h);
            }
        }
        public JobProfile CreateProfile(string Description, string UserKey, DatabaseManagerHelperModel h)
        {
            bool hasTran = h.HasOpenTran;
            h.QualifiedProcedure = "SEIDR.usp_JobProfile_i";
            h.ExpectedReturnValue = 0;
            h["Description"] = Description;
            h["OrganizationID"] = 0;
            h["UserKey"] = UserKey;
            h["ProjectID"] = DBNull.Value;
            h["LoadProfileID"] = DBNull.Value;
            h["SafetyMode"] = true;            
            try
            {
                return _Manager.SelectSingle<JobProfile>(h);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (hasTran && !h.HasOpenTran) //exception rolled back.
                    h.BeginTran();
                h.QualifiedProcedure = "SEIDR.usp_JobProfile_sl";
                h.RemoveKey("UserKey");
                h["UserKey1"] = UserKey;
                return _Manager.SelectList<JobProfile>(h).First();
            }
        }

        public JobExecution RegisterBasicExecution(JobProfile jp, string FilePath = null, DateTime? FileDate = null)
        {
            string FileHash = null;
            long? FileSize = null;
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                //May need to figure out an automated procedure for cleaning anything up from the profile for this filepath
                // in unit tests...
                var i = new FileInfo(FilePath);
                if (i.Exists)
                {
                    FileHash = i.GetFileHash();
                    FileSize = i.Length;
                }
            }
            using (var h = _Manager.GetBasicHelper())
            {
                h.Procedure = "usp_Job_RegisterFile";
                h[nameof(JobProfile.JobProfileID)] = jp.JobProfileID;
                h[nameof(FilePath)] = FilePath as object ?? DBNull.Value;
                h[nameof(FileSize)] = FileSize as object ?? DBNull.Value;
                h[nameof(FileHash)] = FileSize as object ?? DBNull.Value;
                h[nameof(FileDate)] = FileDate ?? DateTime.Today;
                h[nameof(JobExecution.StepNumber)] = 1;
                h["QueueAfterRegister"] = false;

                return _Manager.SelectSingle<JobExecution>(h);
            }
        }

        public void CheckJobMetaData<J>() where J : IJob
        {
            var tInfo = typeof(J);
            var metadataInfo = tInfo.GetCustomAttributes(typeof(IJobMetaDataAttribute), true);
            var md = metadataInfo[0] as IJobMetaDataAttribute;

            var dt = new System.Data.DataTable("udt_JobMetaData");
            dt.AddColumns<IJobMetaData>(
                nameof(IJobMetaData.SafeCancel),
                nameof(IJobMetaData.RerunThreadCheck)); //Fields we don't need in the database. 
            dt.AddRow(md);
            using (var h = _Manager.GetBasicHelper())
            {
                h.QualifiedProcedure = "TEST.usp_CheckJob";
                h["JobList"] = dt;
                _Manager.Execute(h);
            }
        }
        public void CheckJobMetaData(params IJob[] jobList)
        {
            var metaDataList = new List<IJobMetaDataAttribute>();
            foreach (var j in jobList)
            {
                var tInfo = j.GetType();
                var metadataInfo = tInfo.GetCustomAttributes(typeof(IJobMetaDataAttribute), true);
                if (metadataInfo.UnderMaximumCount(1))
                    throw new Exception("Job " + tInfo.Name + " Does not have meta data decoration.");
                var md = metadataInfo[0] as IJobMetaDataAttribute;
                metaDataList.Add(md);
            }

            var dt = new System.Data.DataTable("udt_JobMetaData");
            dt.AddColumns<IJobMetaData>(
                nameof(IJobMetaData.SafeCancel),
                nameof(IJobMetaData.RerunThreadCheck)); //Fields we don't need in the database. 
            metaDataList.ForEach(md => dt.AddRow(md));
            using (var h = _Manager.GetBasicHelper())
            {
                h.QualifiedProcedure = "TEST.usp_CheckJob";
                h["JobList"] = dt;
                _Manager.Execute(h);
            }
        }
        /// <summary>
        /// Creates or updates a JobProfile_Job record for a given profile, and returns the Identity to use as a key.
        /// </summary>
        /// <typeparam name="J"></typeparam>
        /// <param name="jp"></param>
        /// <param name="h"></param>
        /// <param name="StepNumber"></param>
        /// <param name="Description"></param>
        /// <param name="trigger"></param>
        /// <param name="ThreadID"></param>
        /// <param name="SequenceSchedule"></param>
        /// <returns></returns>
        public int SetStep<J>(JobProfile jp, DatabaseManagerHelperModel h, short StepNumber, string Description, ExecutionStatus trigger = null, int? ThreadID = null, string SequenceSchedule = null) where J : IJob
        {
            const string IDENT = "JobProfile_JobID";
            var tInfo = typeof(J);
            var metadataInfo = tInfo.GetCustomAttributes(typeof(IJobMetaDataAttribute), true);
            var md = metadataInfo[0] as IJobMetaDataAttribute;
            var dr = _Manager.SelectRowWithKey("JobName", md.JobName, "SEIDR.Job");
            h.QualifiedProcedure = "SEIDR.usp_JobProfile_Job_iu";
            h[nameof(JobProfile.JobProfileID)] = jp.JobProfileID;
            h[nameof(StepNumber)] = StepNumber;
            h[nameof(Description)] = Description;
            h["TriggerExecutionStatus"] = trigger?.ExecutionStatusCode as object ?? DBNull.Value;
            h["TriggerExecutionNameSpace"] = trigger?.NameSpace as object ?? DBNull.Value;
            h["CanRetry"] = false;
            h["RetryLimit"] = 0;
            h["RetryDelay"] = 5;
            h["JobID"] = dr["JobID"];
            h[nameof(ThreadID)] = ThreadID as object ?? DBNull.Value;
            h[IDENT] = -1; //For output
            h[nameof(SequenceSchedule)] = SequenceSchedule as object ?? DBNull.Value;
            h["FailureNotificationMail"] = DBNull.Value;

            _Manager.Execute(h);
            return (int)h[IDENT];

        }
        /// <summary>
        /// Creates or updates a JobProfile_Job record for the given profile.
        /// <para>Calls <see cref="SetStep{J}(JobProfile, DatabaseManagerHelperModel, short, string, ExecutionStatus, int?, string)"/> but creates and closes the model within this method call, so it cannot be rolled back in this call. </para>
        /// </summary>
        /// <typeparam name="J"></typeparam>
        /// <param name="jp"></param>
        /// <param name="StepNumber"></param>
        /// <param name="Description"></param>
        /// <param name="trigger"></param>
        /// <param name="ThreadID"></param>
        /// <param name="SequenceSchedule"></param>
        /// <returns></returns>
        public int SetStep<J>(JobProfile jp, short StepNumber, string Description, ExecutionStatus trigger = null, int? ThreadID = null, string SequenceSchedule = null) where J : IJob
        {
            using (var h = _Manager.GetBasicHelper())
            {
                return SetStep<J>(jp, h, StepNumber, Description, 
                        trigger:trigger, 
                        ThreadID:ThreadID, 
                        SequenceSchedule: SequenceSchedule);
            }
        }
        /// <summary>
        /// Prepares a root directory in <see cref="BASE_ROOT"/>
        /// </summary>
        /// <param name="CleanDirectories">Deletes any files and subdirectories. Note: If cleaning, make sure that you do not have any files open in programs that hold onto handles, or it will cause an error.</param>
        /// <returns></returns>
        public virtual DirectoryInfo PrepRootDirectory(bool CleanDirectories)
        {
            string Dir = BASE_ROOT;
            DirectoryInfo di = new DirectoryInfo(Dir);
            //JobFolder = di.FullName;

            if (!di.Exists)
                di.Create();
            else if (CleanDirectories)
            {
                di.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete()); //Clean out files
                di.EnumerateDirectories().ForEach(d => d.Delete(true)); //Clean subdirectories.
            }            
            MyRoot = di;
            if (LogFilePath == null)
                LogFilePath = Path.Combine(MyRoot.FullName, "LOG.txt");
            return di;
        }
    
        /// <summary>
        /// Gets a file resource from test project, and make a temporary local copy within the Job's working directory ( <see cref="BASE_JOB_ROOT"/> / <see cref="MyRoot"/>  ) 
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public FileInfo GetTestFile(string FileName) => GetTestFile(FileName, false, null);
        /// <summary>
        /// Gets a file resource from test project, and make a temporary local copy within the Job's working directory ( <see cref="BASE_JOB_ROOT"/> / <see cref="MyRoot"/>  ). Does NOT the FilePath of TestExecution.
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FolderList"></param>
        /// <returns></returns>
        public FileInfo GetTestFile(string FileName, params string[] FolderList) => GetTestFile(FileName, false, FolderList);
        /// <summary>
        /// Copies a file resource from the test project, makes a temporary local copy within the Job's working directory, and sets it as the FilePath of the <see cref="_TestExecution"/>. Also ensures that the _TestExecution has been initialized. 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FolderList"></param>
        /// <returns></returns>
        public FileInfo SetExecutionTestFile(string FileName, params string[] FolderList) => GetTestFile(FileName, true, FolderList);      
        /// <summary>
        /// Gets a file resource from test project, and make a temporary local copy within the Job's working directory ( <see cref="BASE_JOB_ROOT"/> / <see cref="MyRoot"/>  ) 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="SetExecutionPath">If true, updates the FilePath of JobExecution to point to this file.</param>
        /// <param name="FolderList"></param>
        /// <returns></returns>
        private FileInfo GetTestFile(string FileName, bool SetExecutionPath, params string[] FolderList)
        {
            if (MyRoot == null)
                throw new Exception("Must prepare root directory before getting Test File.");
            if(_TestExecution == null)
                _TestExecution = JobExecution.GetSample(-1, -1, -1, -1, -1, UserKey1: "TEST");
            string path = "..\\..\\";
            foreach (var Folder in FolderList)
            {
                if (!string.IsNullOrWhiteSpace(Folder))
                {
                    path = path + Folder + "\\";
                }
            }
            var info = new FileInfo(path + FileName);
                     
            string dest = Path.Combine(MyRoot.FullName, FileName);
            if (info.Exists)
            {
                System.Diagnostics.Debug.WriteIf(File.Exists(dest), "Overwriting '" + dest + "' with Unit Test Resource.");
                File.Copy(info.FullName, dest, true);
                var result = new FileInfo(dest);
                if(SetExecutionPath)
                    _TestExecution.SetFileInfo(result);                
                return result;
            }
            else
                throw new FileNotFoundException(FileName);
        }
        /// <summary>
        /// Compares the Content of the Test Execution's FilePath against the Content of the file specified by <paramref name="Expected"/>, and asserts that they match. Uses <see cref="File.ReadAllText(string)"/> 
        /// </summary>
        /// <param name="Expected"></param>
        /// <param name="IgnoreNewLines"></param>
        public void AssertFileContent(FileInfo Expected, bool IgnoreNewLines = true) => AssertFileContent(Expected, new FileInfo(TestPath), IgnoreNewLines);
        /// <summary>
        /// Compares the content of the files specified by the two FileInfo objects, <paramref name="Expected"/> and <paramref name="Actual"/>, and asserts that the content matches. Uses <see cref="File.ReadAllText(string)"/> 
        /// </summary>
        /// <param name="Expected"></param>
        /// <param name="Actual"></param>
        /// <param name="IgnoreNewLines"></param>
        public void AssertFileContent(FileInfo Expected, FileInfo Actual, bool IgnoreNewLines = true)
        {
            Assert.IsTrue(CompareTestContent(Expected, Actual, IgnoreNewLines));
        }
        public void AssertFileContent(string ExpectedFileName, FileInfo Actual, bool ignoreNewLines, params string[] FolderList)
        {
            var Expected = GetTestFile(ExpectedFileName, FolderList);
            AssertFileContent(Expected, Actual, ignoreNewLines);
        }
        public void AssertFileContent(string ExpectedFileName, FileInfo Actual, bool ignoreNewLines)
        {
            var Expected = GetTestFile(ExpectedFileName);
            AssertFileContent(Expected, Actual, ignoreNewLines);
        }
        /// <summary>
        /// Compares content of the files specified by the two FileInfo objects, <paramref name="Expected"/> and <paramref name="Actual"/>, and returns the result of comparison.  Uses <see cref="File.ReadAllText(string)"/> 
        /// </summary>
        /// <param name="Expected"></param>
        /// <param name="Actual"></param>
        /// <param name="ignoreNewlines"></param>
        /// <returns></returns>
        public bool CompareTestContent(FileInfo Expected, FileInfo Actual, bool ignoreNewlines = true)
        {
            Expected.Refresh();
            if (!Expected.Exists)
                throw new FileNotFoundException(nameof(Expected));
            Actual.Refresh();
            if (!Actual.Exists)
                throw new FileNotFoundException(nameof(Actual));
            if (Expected.Length == 0 && Actual.Length == 0)
                return true;
            string ExpectedContent, ActualContent;
            if (ignoreNewlines)
            {
                ExpectedContent = File.ReadAllText(Expected.FullName).Replace("\r", "").Replace("\n", "");
                ActualContent = File.ReadAllText(Actual.FullName).Replace("\r", "").Replace("\n", "");
            }
            else
            {
                if (Expected.Length != Actual.Length)
                    return false;
                ExpectedContent = File.ReadAllText(Expected.FullName);
                ActualContent = File.ReadAllText(Actual.FullName);                
            }
            return ExpectedContent == ActualContent;
        }


        /// <summary>
        /// Creates a random file in the JobFolder. May have multiple extensions.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public FileInfo CreateRandomFile(string extension = "TXT")
        {
            if (extension[0] != '.')
                extension = '.' + extension;
            string p = Path.Combine(MyRoot.FullName, Path.GetRandomFileName());
            if (!p.EndsWith(extension))
                p = p + extension;
            File.WriteAllText(p, p);
            return new FileInfo(p);            
        }                
        /// <summary>
        /// Creates a file in the job folder with the given name, and the given content
        /// </summary>
        /// <param name="FileNameWithExtension"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public FileInfo CreateFile(string FileNameWithExtension, string Text = null) => CreateFile(FileNameWithExtension, MyRoot, Text);
        public FileInfo CreateFile(string FileNameWithExtension, DirectoryInfo dir, string Text = null)
        {
            string p = Path.Combine(dir.FullName, FileNameWithExtension);
            File.WriteAllText(p, Text ?? string.Empty);
            return new FileInfo(p);
        }
        public FileInfo CreateSubFolderFile(string FileNameWithExtension, string subFolder, string Text = null)
        {
            string p = Path.Combine(MyRoot.FullName, subFolder, FileNameWithExtension);
            File.WriteAllText(p, Text ?? string.Empty);
            return new FileInfo(p);
        }
        /// <summary>
        /// Returns a Settings File for the Test JobExecution using the specicfied File Information
        /// </summary>
        /// <param name="FileNameWithExtension"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public JobProfile_Job_SettingsFile CreateSettingsFile(string FileNameWithExtension, string Text = null)
        {
            var f = new JobProfile_Job_SettingsFile();
            var info = CreateFile(FileNameWithExtension, Text);
            if (_TestExecution != null)
                f.JobProfile_JobID = _TestExecution.JobProfile_JobID;
            f.SettingsFilePath = info.FullName;
            return f;
        }
        /// <summary>
        /// Gets a Settings file for the Test Job Execution usin a file that is stored in a resource folder of the Unit Test project
        /// </summary>
        /// <param name="FileNameWithExtension"></param>
        /// <param name="FolderList"></param>
        /// <returns></returns>
        public JobProfile_Job_SettingsFile CreateSettingsFile(string FileNameWithExtension, params string[] FolderList)
        {

            var f = new JobProfile_Job_SettingsFile();
            var info = GetTestFile(FileNameWithExtension, FolderList);
            if (_TestExecution != null)
                f.JobProfile_JobID = _TestExecution.JobProfile_JobID;
            f.SettingsFilePath = info.FullName;
            return f;
        }        
        /// <summary>
        /// Returns a settings file obejct for the test execution using the FileInfo provided
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public JobProfile_Job_SettingsFile CreateSettingsFile(FileInfo source)
        {
            var f = new JobProfile_Job_SettingsFile();            
            if (_TestExecution != null)
                f.JobProfile_JobID = _TestExecution.JobProfile_JobID;
            f.SettingsFilePath = source.FullName;
            return f;
        }
        /// <summary>
        /// Method called at end of base constructor, with database connection and dummy service available.
        /// <para>Should simplify calling common setup</para>
        /// <para>NOTE: base version of Init simply creates _TestExecution as an empty jobExecution shell, ready to have properties set.</para>
        /// </summary>
        protected virtual void Init()
        {
            if (_TestExecution == null)
            {
                _TestExecution = JobExecution.GetSample(-1, -1, -1, -1, -1, UserKey1: "TEST");              
                _Executor.SetExecution(_TestExecution);
            }
        }

        protected void LinkExecution()
        {
            _Executor.SetExecution(_TestExecution);
        }
        /// <summary>
        /// Sets the Processing Date of <see cref="_TestExecution"/>. Ensures that Test Execution Exists.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="Day"></param>
        protected void SetProcessingDate(int year, int month, int Day)
        {
            DateTime procDate = new DateTime(year, month, Day);
            SetProcessingDate(procDate);
        }
        /// <summary>
        /// Sets the Processing Date of <see cref="_TestExecution"/>. Ensures that Test Execution Exists.
        /// </summary>
        /// <param name="procDate">Processing Date</param>
        protected void SetProcessingDate(DateTime ProcDate)
        {
            if(_TestExecution == null)
                _TestExecution = JobExecution.GetSample(-1, -1, -1, -1, -1, UserKey1: "TEST");
            _TestExecution.SetProcessingDateTime(ProcDate);
        }
        
    }
}
