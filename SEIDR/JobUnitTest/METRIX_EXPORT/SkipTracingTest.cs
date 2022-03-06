using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest.METRIX_EXPORT
{
    [TestClass]
    public class SkipTracingTest : JobTestBase<SEIDR.METRIX_EXPORT.SkipTracing.SkipTracingImportJob>
    {
        [TestMethod]
        public void SkipTracingImportTest()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug", "");
            var setting = new SEIDR.METRIX_EXPORT.ExportSetting(filePath,
                                                                vendorName: "LexisNexis",
                                                                metrixDatabaseLookupID: 7,
                                                                exportType: "SkipTracing Export",
                                                                importType: "SkipTracing Import"
                                                               );
            var m = MockManager.NewMockModel("usp_ExportSetting_ss", "METRIX");
            m.MapToNewRow(setting);

            _TestExecution.FilePath = System.IO.Path.Combine(setting.ArchiveLocation, "FileSplitting\\LexisNexisBatchFile_994_20190801_030114__out.csv");
            _TestExecution.SetProjectID(188);
            _TestExecution.METRIX_ExportBatchID = 528805;
            Assert.IsTrue(ExecuteTest());
        }
    }
}
