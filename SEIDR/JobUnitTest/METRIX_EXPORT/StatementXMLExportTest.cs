using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.METRIX_EXPORT.Statements;

namespace JobUnitTest.METRIX_EXPORT
{
    [TestClass]
    public class StatementXMLExportTest: JobTestBase<StatementXMLGenerationJob>
    {
        [TestMethod]
        public void TestGeneration()
        {
            var setting = new SEIDR.METRIX_EXPORT.ExportSetting(@"C:\SEIDR\TestLetter\",
                                                                vendorName: "Nordis",
                                                                metrixDatabaseLookupID: 7,
                                                                exportType: "Statement Export",
                                                                importType: null
                                                               );
            var m = MockManager.NewMockModel("usp_ExportSetting_ss", "METRIX");
            m.MapToNewRow(setting);

            _TestExecution.METRIX_ExportBatchID = 527311; //projectID and other information driven by ExportBatchID and settings from above.


            Assert.IsTrue(ExecuteTest());

            
        }
    }
}
