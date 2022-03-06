using System;
using System.Text;
using System.Collections.Generic;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.PreProcess;

namespace JobUnitTest
{
    [TestClass]
    public class PreProcessJobTest: JobTestBase<PreProcessJob>
    {
        private DatabaseManager mgr => MockManager;
        ExecutionStatus Status = new ExecutionStatus();
        TestExecutor test => _Executor;
        PreProcessJob ppj => _JOB;

        public PreProcessJobTest()
        {
        }

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void PreProcessJobCheckThreadTest()
        {
            int trd = _JOB.CheckThread(null, 1, _Executor);
            Assert.AreEqual(1, trd);
        }

        [TestMethod]
        public void PreProcessJobExecuteTest()
        { /*
            JobExecution job = new PreProcessJobTest() { JobID = 36, JobProfileID = 19, JobExecutionID = 21320579, JobProfile_JobID = 44, FilePath= @"C:\SEIDR\RegistrationPreProcess\_Registered\TestFile1_11_07_18_New.txt" };
            Assert.IsTrue(ppj.Execute(test, job, ref Status));
            */
        }
    }
}
