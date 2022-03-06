using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.METRIX_EXPORT;
using SEIDR.METRIX_EXPORT.ARxChangeExport;

namespace JobUnitTest
{
    [TestClass]
    public class ARxChangeExportTest : JobExecution
    {
        DatabaseConnection c;
        DatabaseManager mgr;
        ExecutionStatus Status = new ExecutionStatus();
        TestExecutor test;
        MetrixExportStatusUpdateJob lsuj = new MetrixExportStatusUpdateJob();
        ARxChangeExportFileGenerationJob fg = new ARxChangeExportFileGenerationJob();

        public ARxChangeExportTest()
        {
            c = new DatabaseConnection(@"metaldev.cymetrix.com\SQL2014", "MIMIR");
            mgr = new DatabaseManager(c);

            test = new TestExecutor(c);
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
        public void ARxChangeExportExecute_Connection_Test()
        {
            bool result = false;
            string c1 = mgr.GetConnection().ToString();
            ARxChangeExportService _service = new ARxChangeExportService(mgr);
            string connectionString = _service.GetConnectionString();
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                result = true;
                conn.Close();
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ARxChangeExportFilegenerationJob_Test()
        {

            JobExecution job = new ARxChangeExportTest() { METRIX_ExportBatchID = null };
            //JobExecution job = new ARxChangeExportTest() { METRIX_ExportBatchID = 460284 };
            //JobExecution job = new ARxChangeExportTest() { METRIX_ExportBatchID = 34916 };
            try
            {
                Assert.IsTrue(fg.Execute(test, job, ref Status));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        //[TestMethod]
        //public void ARxChangeStatusUpdateJob_Test()
        //{
        //    JobExecution job = new ARxChangeExportTest()
        //    {
        //        JobID = 37,
        //        JobProfileID = 68,
        //        JobExecutionID = 1335,
        //        JobProfile_JobID = 529,
        //        ExecutionStatusCode = "SC",
        //        ExecutionStatusNameSpace = "SEIDR",
        //        METRIX_ExportBatchID = 34954,
        //        IsError = true
        //    };
        //    Assert.IsTrue(lsuj.Execute(test, job, ref Status));
        //}
    }
}
