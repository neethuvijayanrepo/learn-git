using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.DemoMap;
using SEIDR.DemoMap.HEALTHQUEST;
using DemoMapJob = SEIDR.DemoMap.BaseImplementation.DemoMapJob;

namespace JobUnitTest
{
    [TestClass]
    public class DemoMapPreprocessTest :JobTestBase<DemoMapJob>
    {
       
        public DemoMapPreprocessTest()
        {
            c = new DatabaseConnection(@"NCIHCTSTSQL07\sql2014", "MIMIR");
            //c = new DatabaseConnection(@"NCIHCTSTSqL05", "DataServices");
            mgr = new DatabaseManager(c);
        }
        DatabaseConnection c;
        DatabaseManager mgr;


        JobExecution job_CleanMoneField = JobExecution.GetSample(1, 123, 44, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\CleanMoneField.TXT", null, null);
        JobExecution job_CheckDates = JobExecution.GetSample(1, 123, 44, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\CheckDates.TXT", null, null);
        JobExecution job_BadBucketTransform = JobExecution.GetSample(1, 123, 44, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\BadBucketTransform.TXT", null, null); 
        JobExecution job_IsSelfPay = JobExecution.GetSample(1, 123, 44, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\IsSelfPay.TXT", null, null); // use facility ID 33
        JobExecution job_OOOBacket = JobExecution.GetSample(1, 123, 44, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\OOOBucket.TXT", null, null);
        JobExecution mlktest = JobExecution.GetSample(1, 123, 51, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\MLK\CYM_MLK_DEMO_20180627.CYM", null, null);
        JobExecution UAB_Test = JobExecution.GetSample(1, 123, 51, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Demographics\Input\demo_all_20180627.TXT.CYM", null, null);

        ExecutionStatus Status = new ExecutionStatus();
        TestExecutor test => _Executor;
        DemoMapJob dm = new DemoMapJob();
        MLK_DEMO mlk = new MLK_DEMO();
        UAB_DMAP uab = new UAB_DMAP();

        [TestMethod]
        public void MlkDmapTest()
        {  
            Assert.IsTrue(mlk.Execute(test, mlktest, ref Status));
        }

        [TestMethod]
        public void UabDmapTest()
        {
            Assert.IsTrue(uab.Execute(test, UAB_Test, ref Status));
        }

        [TestMethod]
        public void DemoMapFileProcess_CleanMoneyTest()
        {
            Assert.IsTrue(dm.Execute(test, job_CleanMoneField, ref Status));
        }

        [TestMethod]
        public void DemoMapFileProcess_CheckDateTest()
        {
            Assert.IsTrue(dm.Execute(test, job_CheckDates, ref Status));
        }

        [TestMethod]
        public void DemoMapFileProcess_BadBucketTransformTest()
        {
            Assert.IsTrue(dm.Execute(test, job_BadBucketTransform, ref Status));
        }

        [TestMethod]
        public void DemoMapFileProcess_IsSelfPayTest()
        {
            Assert.IsTrue(dm.Execute(test, job_IsSelfPay, ref Status));
        }

        [TestMethod]
        public void DemoMapFileProcess_OOOBucketTest()
        {
            Assert.IsTrue(dm.Execute(test, job_OOOBacket, ref Status));
        }


    }

}
