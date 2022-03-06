using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.METRIX_EXPORT.ARxChangeExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobUnitTest
{
    [TestClass]
    public class ARxChangeExportTest : JobTestBase<ARxChangeReconciliationJob>
    {
        [TestMethod]
        public void ARxChangeExportFileGenerationJob_Test()
        {
            const int EXPORT_BATCH_ID = 35454;
            _TestExecution.SetJobProfile_JobID(581);
            _TestExecution.SetJobProfileID(93);
            //_TestExecution.METRIX_ExportBatchID = null;
            _TestExecution.METRIX_ExportBatchID = EXPORT_BATCH_ID;
            _TestExecution.SetJobExecutionID(3585);   

            Assert.IsTrue(ExecuteTest());

        }
    }
}
