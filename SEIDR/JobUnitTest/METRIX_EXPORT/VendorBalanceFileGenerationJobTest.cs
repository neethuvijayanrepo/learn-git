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
    public class VendorBalanceFileGenerationJobTest: JobTestBase<VendorBalanceFileGenerationJob>
    {
        [TestMethod]
        public void TestGeneration()
        {
            var setting = new SEIDR.METRIX_EXPORT.ExportSetting(@"C:\SEIDR\TestVendorFile\",
                                                                vendorName: "PatientCo",
                                                                metrixDatabaseLookupID: 7,
                                                                exportType: "VendorBalance Export",
                                                                importType: null
                                                               );
            var m = MockManager.NewMockModel("usp_ExportSetting_ss", "METRIX");
            m.MapToNewRow(setting);

            _TestExecution.METRIX_ExportBatchID = 132889;
         

            Assert.IsTrue(ExecuteTest());
            
        }
    }
}
