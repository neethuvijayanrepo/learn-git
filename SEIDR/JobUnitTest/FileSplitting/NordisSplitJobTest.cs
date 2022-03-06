using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest.FileSplitting
{
    [TestClass]
    public class NordisSplitJobTest:JobTestBase<SEIDR.FileSystem.FileSplitting.NordisSplitJob>
    {                        
        /// <summary>
        /// Resource folder name in unit test project
        /// </summary>
        const string FOLDER_NAME = nameof(FileSplitting);
        [TestMethod]
        public void TestSplit()
        {
            const string SAMPLE_FILE_NAME = "Nav_Confirmation_20190603_1.txt"; //Note: Be sure to check encoding of file in resource folder.

            SetProcessingDate(2019, 06, 3);
            SetExecutionTestFile(SAMPLE_FILE_NAME, FOLDER_NAME);

            var Expected = GetTestFile("Nav_Confirmation_20190603_Project115.Expected", FOLDER_NAME); //Took the content from sample file for project 115, sorted based on source file (first column)

            bool result = ExecuteTest();
            Assert.IsTrue(result);
            Assert.IsNull(_TestExecutionStatus);
            MyRoot.Refresh();

            //We'll have a file output for each Nordis file indicated by the first X lines, but they'll only have content if they're actually used.
            var outputInfo = OutputDir.EnumerateFiles("*.*").Where(f => f.Length > 0).ToArray();
            Assert.AreEqual(3, outputInfo.Length);

            var proj115 = outputInfo.First(f => f.Name.Contains("Project115"));
            AssertFileContent(Expected, proj115, true);
        }
        [TestMethod]
        public void TestLargeSplit()
        {
            const string SAMPLE_FILE_NAME = "Nav_Confirmation_20190604_1.txt";
            SetProcessingDate(2019, 06, 04);
            SetExecutionTestFile(SAMPLE_FILE_NAME, FOLDER_NAME);

            bool result = ExecuteTest();
            Assert.IsTrue(result);
            Assert.IsNull(_TestExecutionStatus);

            var outputInfo = OutputDir.EnumerateFiles("*.*").Where(f => f.Length > 0).ToArray();
            Assert.AreEqual(6, outputInfo.Length);
            
            foreach(var outFile in outputInfo)
            {
                //Go through expected files in FileSplitting unit test resource folder, compare against output
                AssertFileContent(outFile.Name.Replace(".CYM", ".EXPECTED"), outFile, true, FOLDER_NAME);
            }

        }
        [TestMethod]
        public void TestMissingFile()
        {
            SetProcessingDate(2019, 06, 4);

            string DummyPath = System.IO.Path.Combine(MyRoot.FullName, "MISSING FILE.txt");
            var info = new System.IO.FileInfo(DummyPath);
            Assert.IsFalse(info.Exists);
            _TestExecution.SetFileInfo(info);
            
            bool result = ExecuteTest();
            Assert.IsFalse(result);
            Assert.IsNotNull(_TestExecutionStatus);
            Assert.AreEqual("NS", _TestExecutionStatus.ExecutionStatusCode); 
            Assert.AreEqual(nameof(SEIDR.FileSystem), _TestExecutionStatus.NameSpace); 
        }
        /*
        [TestMethod]
        public void RemoveIDData()
        {
            const string SAMPLE_FILE_NAME = "Nav_Confirmation_20190604_1.txt";            
            var info = GetTestFile(SAMPLE_FILE_NAME, FOLDER_NAME);
            var MetaData = new SEIDR.Doc.DocMetaData(info.FullName);
            for(int i = 1; i <= 20; i++)
            {
                MetaData.AddColumn("COLUMN # " + i);
            }
            MetaData.SetDelimiter('|').SetHasHeader(false);
            MetaData.Columns.AllowMissingColumns = true;
            MetaData.CanWrite = true;
            MetaData.SetEmptyIsNull(true);

            var output = new SEIDR.Doc.DocMetaData(MyRoot.FullName, "Nav_Confirmation_Cleaned.txt", "o")
                .SetDelimiter('|')
                .SetHasHeader(false)
                .AddDetailedColumnCollection(MetaData);
            using (var read = new SEIDR.Doc.DocReader(MetaData))
            using(var o = new SEIDR.Doc.DocWriter(output))
            {
                const int N_ADDRESS1 = 14 - 1;
                const int N_ADDRESS2 = 15 - 1;
                const int N_CITY = 16 - 1;
                const int N_STATE = 17 - 1;
                const int N_ZIP = 18 - 1;
                const int O_ADDRESS1 = 8 - 1;
                const int O_ADDRESS2 = 9 - 1;
                const int O_CITY = 10 - 1;
                const int O_STATE = 11 - 1;
                const int O_ZIP = 12 - 1;
                const int LETTERID = 2 - 1;

                foreach (var record in read)
                {
                    if (record[0] != read.FileName) //Need to manually cleanup the initial records (too many column delimiters in output because it doesn't account for multiple record types)
                    {
                        if (!string.IsNullOrWhiteSpace(record[N_ADDRESS1]))
                        {
                            record[N_ADDRESS1] = "Address1 Information";
                            if (!string.IsNullOrEmpty(record[N_ADDRESS2]))
                                record[N_ADDRESS2] = "Address2";
                            record[N_CITY] = "CITY";
                            record[N_STATE] = "ST";
                        }
                        if (!string.IsNullOrWhiteSpace(record[N_ZIP]))
                            record[N_ZIP] = "123456789";
                        if (!string.IsNullOrEmpty(record[O_ADDRESS1]))
                        {
                            record[O_ADDRESS1] = "Address 1";
                            if (!string.IsNullOrEmpty(record[O_ADDRESS2]))
                                record[O_ADDRESS2] = "Address 2";
                            record[O_CITY] = "Bad City"; //Not really sure about this..
                            record[O_STATE] = "BT";
                        }
                        if (!string.IsNullOrEmpty(record[O_ZIP]))
                            record[O_ZIP] = "987654321";
                        string acct = record[LETTERID];
                        StringBuilder sb = new StringBuilder();                        
                        foreach(var c in acct.Reverse())
                        {                            
                            sb.Append(c);
                            if (sb.Length % 4 == 0)
                                sb.Append((c + sb.Length) % 9);
                        }
                        record[LETTERID] = sb.ToString();
                    }
                    o.AddRecord(record);
                }
            }
        }
        */

        System.IO.DirectoryInfo OutputDir;
        protected override void Init()
        {
            base.Init();
            OutputDir = PrepSubfolder("_Parsed");       
        }
    }
}
