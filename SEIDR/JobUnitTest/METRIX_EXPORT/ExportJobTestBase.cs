using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT;

namespace JobUnitTest.METRIX_EXPORT
{
    public abstract class ExportJobTestBase<T> : JobTestBase<T> where T: ExportJobBase
    {
        // As we get new examples of Export jobs, may find ways to make common methods for the different settings...

        protected void AssertStage(ExportJobBase.ExportBatchStage Expected)
        {
            var actual = _JOB.CheckStage(Context);
            Assert.AreEqual(Expected, actual);
        }
        /// <summary>
        /// Uses <see cref="WorkingFile"/> to get the current FilePath location, and compares it against a file from the Test Project (<see cref="TestBase.AssertFileContent(string, System.IO.FileInfo, bool, string[])"/> )
        /// <para>NOTE: Assumes that the test file is in the METRIX_EXPORT folder</para>
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="ignoreNewLines"></param>
        public void AssertWorkingFileContent(string FileName, bool ignoreNewLines = true)
        {
            var actual = new System.IO.FileInfo(WorkingFile);
            AssertFileContent(FileName, actual, ignoreNewLines, "METRIX_EXPORT");
        }
        public void AssertWorkingFileContent(string FileName, params string[] Folders)
        {
            AssertWorkingFileContent(FileName, true, Folders);
        }
        /// <summary>
        /// Uses <see cref="WorkingFile"/> to get the current FilePath location, and compares it against a file from the Test Project (<see cref="TestBase.AssertFileContent(string, System.IO.FileInfo, bool, string[])"/> )
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="ignoreNewLines"></param>
        /// <param name="Folders"></param>
        public void AssertWorkingFileContent(string FileName, bool ignoreNewLines, params string[] Folders)
        {
            var actual = new System.IO.FileInfo(WorkingFile);
            AssertFileContent(FileName, actual, ignoreNewLines, Folders);
        }

        protected new ExecutionStatus _TestExecutionStatus { get; private set; }
        public new virtual bool ExecuteTest()
        {
            return ExecuteTest(WorkingFile);
        }

        public new virtual bool ExecuteTest(ref ExecutionStatus status)
        {
            var result = ExecuteTest(WorkingFile);
            status = _TestExecutionStatus;
            return result;
        }

        public bool ExecuteTest(LocalFileHelper work)
        {
            LinkExecution();
            if (_JOB == null)
                throw new InvalidOperationException("_JOB is null. Improper initialization.");
            var ret = _JOB.ProcessJobExecution(Context, WorkingFile);
            if (Context.ReturnStatus != null)
            {
                _TestExecutionStatus = Context.ReturnStatus;
                return !_TestExecutionStatus.IsError && ret >= ExportJobBase.SUCCESS_BOUNDARY;
            }
            _TestExecutionStatus = _JOB.GetStatus(ret);
            return !_TestExecutionStatus.IsError;
        }

        private ExportSetting _set = new ExportSetting();
        /// <summary>
        /// Get or Set the ExportSettings object. NOTE: Setting the value to a new object will also change <see cref="Context"/> to point to a new Context object that has the new settings
        /// </summary>
        protected ExportSetting Settings
        {
            get
            { 
                return   _set;
            }
            set
            {
                _set = value;
                Context = new ExportContextHelper(_JOB, _Executor, Context.Execution, value);
            }
        }
        protected ExportContextHelper Context;
        protected LocalFileHelper WorkingFile;
        /// <summary>
        /// Gets a new LocalFile Helper using <see cref="Context"/> and <see cref="TestBase.MyRoot"/> as the working directory.
        /// <para>Also sets the value of <see cref="WorkingFile"/> </para>
        /// </summary>
        /// <returns></returns>
        public LocalFileHelper GetLocalFileHelper()
        {
            return WorkingFile = new LocalFileHelper(_JOB, Context, MyRoot.FullName);
        }

        /// <summary>
        /// Gets a new LocalFile Helper using <see cref="Context"/> and <paramref name="workDirectory"/> as the working directory.
        /// <para>Also sets the value of <see cref="WorkingFile"/> </para>
        /// </summary>
        /// <param name="workDirectory">Working directory for the temporary file to be placed in.</param>
        /// <returns></returns>
        public LocalFileHelper GetLocalFileHelper(string workDirectory)
        {
            return WorkingFile = new LocalFileHelper(_JOB, Context, workDirectory);
        }
        public void ResetContext(SEIDR.JobBase.JobExecution executionForReset)
        {
            _TestExecution = executionForReset;
            Context = new ExportContextHelper(_JOB, _Executor, executionForReset, Settings);
        }
        /// <summary>
        /// Initializes the helper properties <see cref="Settings"/> and <see cref="Context"/>.
        /// <para>Set _TestExecution before calling the base method if you want the Context to point to a different JobExecution (or call <see cref="ResetContext(SEIDR.JobBase.JobExecution)"/> )</para>
        /// <para>Similarly, you can update </para>
        /// </summary>
        protected override void Init()
        {
            base.Init();
            Context = new ExportContextHelper(_JOB, _Executor, _TestExecution, Settings);
        }
    }
}
