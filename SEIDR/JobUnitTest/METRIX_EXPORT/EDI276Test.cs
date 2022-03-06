using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest.METRIX_EXPORT
{
    [TestClass]
    public class EDI276Test : JobTestBase<SEIDR.METRIX_EXPORT.EDI.EDI276FileGenerationJob>
    {
        [TestMethod]
        public void TestFileCreation()
        {
            var setting = new SEIDR.METRIX_EXPORT.ExportSetting(@"C:\SEIDR\Test276\",
                                                                vendorName: "TransUnion",
                                                                metrixDatabaseLookupID: 7,
                                                                exportType: "276 EDI Export",
                                                                importType: "277 EDI Import"
                                                               );
            var m = MockManager.NewMockModel("usp_ExportSetting_ss", "METRIX");
            m.MapToNewRow(setting);

            _TestExecution.FilePath = System.IO.Path.Combine(setting.ArchiveLocation, "EDI276_Test.276");
            _TestExecution.SetProjectID(148);
            _TestExecution.METRIX_ExportBatchID = 527588;
            ExecuteTest();
        }
        [TestMethod]
        public void TestFileNaming()
        {

            var setting = new SEIDR.METRIX_EXPORT.ExportSetting(@"C:\SEIDR\Test276\",
                                                                vendorName: "TransUnion",
                                                                metrixDatabaseLookupID: 7,
                                                                exportType: "276 EDI Export"
                                                               );
            var m = MockManager.NewMockModel("usp_ExportSetting_ss", "METRIX");
            m.MapToNewRow(setting);

            _TestExecution.METRIX_ExportBatchID = 0;
            ExecuteTest();
        }
    }
}
