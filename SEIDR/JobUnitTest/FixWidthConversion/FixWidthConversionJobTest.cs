using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;
using SEIDR;

namespace JobUnitTest
{
    [TestClass]
    public class FixWidthConversionJobTest: JobTestBase<SEIDR.FileSystem.FileConversion.FixWidthConversionJob>
    {
        
        int[] pipeIndexes = new int[] { 4, 18, 13, 14 }; //Values for the basic test content.
        
        const string SETTINGS_FILE = "settings.fwcs";                
        const string FOLDER = "FixWidthConversion"; //Unit test folder (resources)


        [TestMethod]
        public void TestSettingLoad()
        {
            const string DERIVE_COL_NAME = "Derived - MORE";
            var converterSettings = new FixWidthConverter
            {
                NewHeader = "ID|Info|SecondaryInfo|Note",
                LineEnding_CR = false,
            };            
            converterSettings.filterIn.Add(@"[\s0-9]{4}?.{10}?Info%");
            converterSettings.filterOut.Add("%End%");            
            DerivedColumnInfo c = new DerivedColumnInfo("%More%", DERIVE_COL_NAME, 5, 4);
            converterSettings.InsertDerived(c);
            
            converterSettings.fieldWidths.AddRange(pipeIndexes);

            SetExecutionTestFile("Basic.txt", FOLDER);
            var Expected = GetTestFile("TestSettingLoad.txt", FOLDER);

            var settings = CreateSettingsFile(SETTINGS_FILE, converterSettings.ToString());

            _JOB.Process(settings, _TestExecution, _Executor);
            using (var reader = new SEIDR.Doc.DocReader("r", TestPath, converterSettings.Delimiter, converterSettings.LineEnding))
            {
                Assert.AreEqual(2, reader.RecordCount);
                var p = reader.GetPage(0);
                Assert.AreEqual("1234", p[0]["ID"]);
                Assert.AreEqual("1236", p[1]["ID"]);
                Assert.AreEqual("more", p[0][DERIVE_COL_NAME]); //From the line before loading. ("More more info")
                Assert.AreEqual("te", p[1][DERIVE_COL_NAME]);   //from same line.               
            }
            AssertFileContent(Expected, true);
        }
        [TestMethod]
        public void TestSettingsWithNoFilterOut()
        {
            SetExecutionTestFile("Basic.txt", FOLDER);
            var Expected = GetTestFile("TestSettingsWithNoFilterOut.txt", FOLDER);     

            var converterSettings = new FixWidthConverter {
                NewHeader = "ID|Info|SecondaryInfo|Note".Replace('|', '\t')
            };
            converterSettings.filterIn.Add(@"[0-9]{4}?%Info%");
            converterSettings.SetDelimiter(FixWidthConverter.DELIMITER.TAB); //Tab, comma, or Pipe
            converterSettings.fieldWidths.AddRange(pipeIndexes);
            var settings = CreateSettingsFile(SETTINGS_FILE, converterSettings.ToString());

            _JOB.Process(settings, _TestExecution, _Executor);
            using (var reader = new SEIDR.Doc.DocReader("R", TestPath, converterSettings.Delimiter, converterSettings.LineEnding))
            {
                Assert.AreEqual(3, reader.RecordCount); //No Filter out, so we get all three lines.
                var p = reader.GetPage(0);
                Assert.AreEqual("1234", p[0]["ID"]);
                Assert.AreEqual("1235", p[1][0]);
                Assert.AreEqual("1236", p[2]["ID"]);
            }

            AssertFileContent(Expected);
        }

        [TestMethod]
        public void AdvancedDeriveTest()
        {            
            SetExecutionTestFile("Advanced.txt", FOLDER);
            var Expected = GetTestFile("AdvancedDerive.csv", FOLDER);

            var convert = new FixWidthConverter
            {
                NewHeader = "HCPCS,Quantity,ChargeAmount"
            };
            //convert.filterIn.Add(@"[a-zA-Z0-9]{2}?[\sa-zA-Z0-9]{4}?   [\s0-9]{2}?[0-9]{1}?[\s]{30}?[\s0-9]{14}?%[0-9]{2}?");
            convert.filterIn.Add(@"[\sa-zA-Z]{1}?[0-9a-zA-Z]{1}?.{4}?   [\s0-9]{3}?\s{23}?[\s0-9]{9}?.[\s0-9]{2}?");
            convert.SetDelimiter(FixWidthConverter.DELIMITER.COMMA);
            var facility = new DerivedColumnInfo(".{50}?%", "FACILITY", 0, 25);
            var ID = new DerivedColumnInfo(".{50}?%", "PatientID", 50, 0); //To End of line
            convert.InsertDerived(facility, ID);
            convert.fieldWidths.AddRange(new int[] {6, 6, 35});
            var setting = CreateSettingsFile(SETTINGS_FILE, convert.ToString());
            _JOB.Process(setting, _TestExecution, _Executor);
            using (var reader = new SEIDR.Doc.DocReader("r", TestPath, convert.Delimiter, convert.LineEnding))
            {
                var p = reader.GetPage(0);
                Assert.AreEqual(5, reader.RecordCount);                
                Assert.AreEqual("1234", p[0]["PatientID"]);
                Assert.AreEqual("1234", p[1]["PatientID"]);

                Assert.AreEqual("1235", p[2]["PatientID"]);
                Assert.AreEqual("A1234", p[2]["HCPCS"]);

                Assert.AreEqual("12367", p[3]["PatientID"]);
                Assert.AreEqual("50", p[3]["Quantity"]);
                Assert.AreEqual("12367", p[4]["PatientID"]);
                Assert.AreEqual("Hospital Namen", p[4]["FACILITY"]);
                Assert.AreEqual("NP", p[4]["HCPCS"]);
            }
            AssertFileContent(Expected, true); //Ensure output matches against Test file from resource folder
        }

    }
}
