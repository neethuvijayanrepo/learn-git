using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.METRIX_EXPORT;

namespace JobUnitTest.METRIX_EXPORT
{
    [TestClass]
    public class StatusUpdateJobTest:JobTestBase<MetrixExportStatusUpdateJob>
    {

        [TestMethod]
        public void ExportStatusUpdateJob_Test()
        {
            const int EXPORT_BATCH_ID = 34954;
            _TestExecution.SetJobProfileID(68);
            _TestExecution.SetJobExecutionID(1335);
            _TestExecution.SetJobProfile_JobID(529);
            _TestExecution.METRIX_ExportBatchID = EXPORT_BATCH_ID;
            _TestExecution.IsError = true;
            Assert.IsTrue(ExecuteTest());
            _JOB.UpdateExportBatchStatus(_Executor, _TestExecution);
            var met = _JOB.GetMetrixDatabaseManager(_Executor, true);
            string cmd = "SELECT ExportBatchStatusCode FROM EXPORT.ExportBatch WITH (NOLOCK) WHERE ExportBatchID = " +
                         EXPORT_BATCH_ID;
            var row = met.ExecuteText(cmd).Tables[0].Rows[0];
            Assert.AreEqual(MetrixExportStatusUpdateJob.METRIX_EXPORT_FAILURE, row[0]); //1 column only
            //Assert.IsTrue(lsuj.Execute(_Executor, _TestExecution, ref Status));
        }
    }
}
