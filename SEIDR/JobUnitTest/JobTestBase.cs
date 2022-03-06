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

namespace JobUnitTest
{    
    public class JobTestBase<T>: TestBase where T:IJob
    {
        /// <summary>
        /// NOTE: This is for the service database.
        /// </summary>
        /// <param name="ProcedureName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public MockQueryModel NewMockModel(string ProcedureName, string schema = null) => _Executor.MockManager.NewMockModel(ProcedureName, schema);
             
        /// <summary>
        /// Creates a MockModel with a stored procedure name such as usp_{typeof(<typeparamref name="Rt"/>)}_{suffix}
        /// <para>E.g., Rt: PayerMaster_MapInfo, suffix: sl -> usp_PayerMaster_MapInfo_sl</para>
        /// <para>NOTE: This is for the service database.</para>
        /// </summary>
        /// <typeparam name="Rt"></typeparam>
        /// <param name="suffix">Suffix for procedure - default is sl for select list.</param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public MockQueryModel NewMockModel<Rt>(string suffix = "sl", string schema = null) => _Executor.MockManager.NewMockModel<Rt>(suffix, schema);
        /// <summary>
        /// NOTE: This is for the service database.
        /// </summary>
        /// <param name="QualifiedProcedureName"></param>
        /// <returns></returns>
        public MockQueryModel NewMockModelQualified(string QualifiedProcedureName) => _Executor.MockManager.NewMockModelQualified(QualifiedProcedureName);
        public MockData.MockDatabaseManager MockManager => this._Executor.MockManager;        
        protected const string BASE_JOB_ROOT = BASE_ROOT + @"\JOBS";
        /// <summary>
        /// Folder for job work - initializes to <see cref="BASE_JOB_ROOT"/>, but will update to the directory returned in <see cref="PrepRootDirectory{T}(bool)"/> after being called.  
        /// </summary>
        public string JobFolder { get; private set; } = BASE_JOB_ROOT;

        public override DirectoryInfo PrepSubfolder(string FolderName, bool Clean = true)
        {
            if (MyRoot == null)
                throw new NullReferenceException("MyRoot has not been populated by calling PrepRootDirectory yet.");
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
        public DirectoryInfo PrepSubFolder(string FolderName, bool CleanDirectories = true)
        {
            if (MyRoot == null)
            	PrepRootDirectory(CleanDirectories);
            return PrepSubfolder(FolderName, CleanDirectories);
        }
        public IJobMetaDataAttribute GetJobMetaData()
        {
            if (_JOB == null)
                throw new Exception("Job has not been initialized via Prep yet.");

            var tInfo = typeof(T);
            var metadataInfo = tInfo.GetCustomAttributes(typeof(IJobMetaDataAttribute), true);
            return metadataInfo[0] as IJobMetaDataAttribute;
            
        }

        public DateTime ProcessingDate => _TestExecution.ProcessingDate;
        public void CheckJobMetaData()
        {            
            var md = GetJobMetaData();

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
        public int SetStep(JobProfile jp, DatabaseManagerHelperModel h, short StepNumber, string Description, ExecutionStatus trigger = null, int? ThreadID = null, string SequenceSchedule = null) 
        {
            return base.SetStep<T>(jp, h, StepNumber, Description, trigger, ThreadID, SequenceSchedule);
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
        public int SetStep(JobProfile jp, short StepNumber, string Description, ExecutionStatus trigger = null, int? ThreadID = null, string SequenceSchedule = null)            
        {
            return base.SetStep<T>(jp, StepNumber, Description, trigger, ThreadID, SequenceSchedule);
        }
       
        /// <summary>
        /// Created during PrepRootDirectory with parameterless Constructor.
        /// <para>NOTE: for windows service to work, the Job MUST have a public parameterless constructor available.</para>
        /// </summary>
        protected T _JOB { get; private set; } = default(T);        
        public override DirectoryInfo PrepRootDirectory(bool CleanDirectories = true)
        {
            var tInfo = typeof(T);
            var metadataInfo = tInfo.GetCustomAttributes(typeof(IJobMetaDataAttribute), true);
            string Dir = BASE_JOB_ROOT;
            if (!IgnoreJobMetaData)
            {
                if (metadataInfo.Length > 0)
                {
                    var md = metadataInfo[0] as IJobMetaDataAttribute;
                    if (md == null || string.IsNullOrWhiteSpace(md.JobName))
                        throw new Exception("Meta Data is not properly configured for job.");
                    if (md.JobName.Exists(c => c.In(Path.GetInvalidPathChars())))
                    {
                        string temp = md.JobName;
                        foreach(var c in Path.GetInvalidPathChars())
                        {
                            temp = temp.Replace(c, '_');
                        }
                        Dir = Path.Combine(BASE_JOB_ROOT, temp);
                    }
                    else
                        Dir = Path.Combine(BASE_JOB_ROOT, md.JobName);
                }
                else
                    throw new Exception("Job must be decorated with IJobMetaDataAttribute");
            }
            _JOB = Activator.CreateInstance<T>();
            DirectoryInfo di = new DirectoryInfo(Dir);
            JobFolder = di.FullName;
                                
            if (!di.Exists)
                di.Create();
            else if (CleanDirectories)
            {
                di.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete()); //Clean out files
                try
                {
                    di.EnumerateDirectories().ForEach(d => d.Delete(true)); //Clean subdirectories.
                }catch
                {
                    //Not a big deal if we can't delete the directories, as long as the files are cleaned up.
                }
            }
            MyRoot = di;
            if(LogFilePath == null)
                LogFilePath = Path.Combine(MyRoot.FullName, "LOG.txt");
            return di;
        }
        
        ExecutionStatus _MyStatus = null;
        /// <summary>
        /// Execution status of calling <see cref="ExecuteTest"/> 
        /// </summary>
        protected ExecutionStatus _TestExecutionStatus
        {
            get
            {
                return _MyStatus;
            }
        }
        /// <summary>
        /// Calls the Execute method on <see cref="_JOB"/>, passing the test execution and test executor as parameters. 
        /// </summary>
        /// <returns>Execution status result of job call.</returns>
        public bool ExecuteTest()
        {
            return ExecuteTest(ref _MyStatus);
        }
        /// <summary>
        /// Calls the Execute method on <see cref="_JOB"/>, passing the test execution and test executor as parameters. 
        /// <para>Also calls Refresh on <see cref="TestBase.MyRoot"/>  </para>
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Success versus failure status of job call</returns>
        public bool ExecuteTest(ref ExecutionStatus status)
        {
            if (_JOB == null)
                throw new Exception("Must call PrepRootDirectory first before ExecuteTest can be used.");
            LinkExecution();
            try
            {
                var md = GetJobMetaData();
                if(md.NeedsFilePath && _TestExecution.FilePath == null)
                {
                    if (status == null)
                        status = new ExecutionStatus();
                    //Service Call: SetExecutionStatus(false, ExecutionStatus.INVALID);
                    status.ExecutionStatusCode = ExecutionStatus.INVALID;
                    status.Description = nameof(ExecutionStatus.INVALID);
                    status.NameSpace = nameof(SEIDR);
                    status.IsError = true;
                    _Executor.LogError("FilePath required, not provided", null, null);
                    return false;         
                }
                bool r = _JOB.Execute(_Executor, _TestExecution, ref status);
                if (status != null)
                {
                    if (r && status.IsError)
                    {
                        throw new Exception("Status Indicates error, but Job Result is success.");
                    }
                    else if (!r && !status.IsError)
                    {
                        throw new Exception("Status indicates success, but Job Result is failure.");
                    }
                }
                MyRoot.Refresh();
                return r;
            }
            catch(Exception ex)
            {
                _Executor.LogError("Test Execution call", ex, null);
                return false;
            }
        }
        /// <summary>
        /// Method called at end of base constructor, with database connection and dummy service available.
        /// <para>Should simplify calling common setup</para>
        /// <para>NOTE: the Job Base version of this will initialize the <see cref="TestBase._TestExecution"/>, and then also call <see cref="PrepRootDirectory(true)"/>  </para>
        /// </summary>
        protected override void Init()
        {
            base.Init();
            PrepRootDirectory(true);
        }
    }
}
