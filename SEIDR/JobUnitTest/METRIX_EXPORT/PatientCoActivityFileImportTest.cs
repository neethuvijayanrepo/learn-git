using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest.METRIX_EXPORT
{
    [TestClass]
    public class PatientCoActivityFileImportTest : JobTestBase<SEIDR.METRIX_EXPORT.Statements.PatientCoStatementActivityFileImportJob>
    {
        [TestMethod]
        public void ActivityFileImportTest()
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

            _TestExecution.FilePath = System.IO.Path.Combine(setting.ArchiveLocation, "METRIX_EXPORT\\StatementActivityFileSample.csv");
            _TestExecution.SetProjectID(138);
            _TestExecution.METRIX_ExportBatchID = 3655;
            Assert.IsTrue(ExecuteTest());
        }
    }
}
