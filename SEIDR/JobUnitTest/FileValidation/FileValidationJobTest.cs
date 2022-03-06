using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.FileSystem;
using SEIDR.FileSystem.FileValidation;
using SEIDR.JobBase;
using JobUnitTest;
using JobUnitTest.MockData;
using SEIDR.Doc;

namespace JobUnitTest
{
    [TestClass]
    public class FileValidationJobTest : JobTestBase<FileValidationJob>
    {
        const string FOLDER = "FileValidation";
        /*
         ToDo: 
         * Create dummy sample files in a folder for File Validation.
         * Have separate tests per file instead of loading all these samples for every single test
         * Inherit JobTestBase instead of extending JobExecution just to be able to call the 'GetSample' method directly ( Get Sample(...) instead of JobExecution.GetSample(...) - not really a good reason to inherit)
             */
        JobExecution job = JobExecution.GetSample(1, 123, 1, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Input\TextDocument.txt", null, null);
        JobExecution jobNS = JobExecution.GetSample(1, 123, 2, 1, 1, 0, null, null, null, null, "SC", "SEIDER", null, null, null);
        JobExecution jobCN = JobExecution.GetSample(1, 124, 2, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Input\TextDocumentCN.txt", null, null);
        JobExecution jobCC = JobExecution.GetSample(1, 125, 2, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Input\TextDocumentCC.txt", null, null);
        JobExecution jobColumnNoGreaterThanHeader = JobExecution.GetSample(1, 125, 2, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Input\TextDocumentColumnNoGreaterThanHeader.txt", null, null);
        JobExecution jobLastRecordWorngNoOfColumns = JobExecution.GetSample(1, 125, 2, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Input\TextDocumentLastRecordWorngNoOfColumns.txt", null, null);
        ExecutionStatus Status = new ExecutionStatus();
        TestExecutor test;
        FileValidationJob FV = new FileValidationJob();

        //1:Test connection
        [TestMethod]
        public void FileValidationJobMetadataTestConnection()
        {
            FileValidationJobConfiguration validate = FileValidationJobConfiguration.GetFileValidationJobConfiguration(_Manager, 1);
        }

        //2:File present in sourec and all columns are matched
        [TestMethod]
        public void FileValidationCRTest()
        { 
            Assert.IsTrue(FV.Execute(test, job, ref Status));
        }

        //3:File not available in source
        [TestMethod]
        public void FileValidationCRTestNS()
        {
            Assert.IsTrue(FV.Execute(test, jobNS, ref Status));
        }

        //4:Column Name not match
        [TestMethod]
        public void FileValidationCRTestCN()
        {
            Assert.IsTrue(FV.Execute(test, jobCN, ref Status));
        }

        //5:Column Count not match
        [TestMethod]
        public void FileValidationCRTestCC()
        {
            Assert.IsTrue(FV.Execute(test, jobCC, ref Status));
        }

        //6:Column No. Greater than Header columns
        [TestMethod]
        public void FileValidationCRTestNoGreaterThanHeader()
        {
            Assert.IsTrue(FV.Execute(test, jobColumnNoGreaterThanHeader, ref Status));
        }

        //7:Last Column has wrong no. of columns
        [TestMethod]
        public void FileLastRecordWorngNoOfColumns()
        {
            Assert.IsTrue(FV.Execute(test, jobLastRecordWorngNoOfColumns, ref Status));
        }

        [TestMethod]
        public void TextQualifierAdd_MET_12660()
        {
                 
            JobExecution job = JobExecution.GetSample(12143, 12, 25, 3, 1, 0, null, null, null, null, "SC", "SEIDR", @"\\ncihctstsql07.nciwin.local\SEIDR_QA\_Registered\Test5.txt", null, null);
            Assert.IsTrue(FV.Execute(test, job, ref Status));
        }
        [TestMethod]
        public void TextQualifierAdd_MLK()
        {
            JobExecution job = JobExecution.GetSample(12143, 30, 108, 3, 2, ProcessingDate: DateTime.Today, FilePath: @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\MLK\NT ENCOUNTER\mlkch_intellimetrix_enc_notes_20181017.dat");
            try
            {
                bool b = FV.Execute(test, job, ref Status);
                Assert.IsTrue(b);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TextQualified_MLK_FBNE()
        {
            JobExecution job = JobExecution.GetSample(12143, 39, 113, 3, 2, ProcessingDate: DateTime.Today, FilePath: @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\MLK\FBNE\Test\MLKHELDFREQ_20181105.csv");
            try
            {
                bool b = FV.Execute(test, job, ref Status);
                Assert.IsTrue(b);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }


        [TestMethod]
        public void TextQualified_UAB_Note()
        {
            JobExecution job = JobExecution.GetSample(5405274, 61, 200, 3, 2, ProcessingDate: DateTime.Today, FilePath: @"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\UAB\HQ_Hosp\Notes_Zbal\Test\notes_Zbal_20181109.TXT");
            try
            {
                bool b = FV.Execute(test, job, ref Status);
                Assert.IsTrue(b);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }
        [TestMethod]
        public void AHC_IC()
        {
            JobExecution job = JobExecution.GetSample(5406412, 110, 288, 3, 2, FilePath: @"\\Sdsrv015.cymetrix.com\andromedafiles\Adventist\Daily_Loads\Preprocessing\ICFilter\SEIDR\advn_intellimetrix_chrgs_20181120.dat");
            try
            {
                bool b = FV.Execute(test, job, ref Status);
                Assert.IsTrue(b);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }
        [TestMethod]
        public void AHC_CR()
        {
            JobExecution job = JobExecution.GetSample(-1, 129, 331, 3, 2, FilePath: @"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\AdventistCerner\advn_intellimetrix_bill_doc\Cleaned\advn_intellimetrix_bill_doc_20181126.dat");
            try
            {
                bool b = FV.Execute(test, job, ref Status);
                Assert.IsTrue(b);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }
        /*
        [TestMethod]
        public void Obfuscate_BCH()
        {
            //Turn off base.Init if calling this.
            var file = new SEIDR.Doc.DocMetaData(@"C:\SEIDR\JOBS\FileValidationJob\BCH_CYMET_SSC_RE-20190606");
            file.SetMultiLineEndDelimiters("\r", "\n", "\r\n")
                .SetDelimiter('|')
                .SetHasHeader(false);            
            for(int i = 0; i < 182; i++)
            {
                file.AddColumn("Column # " + (i + 1));
            }
            TestObfuscateFileVersion(file, 0);
        }*/
        [TestMethod]
        public void BCH_RECALL_BAD_HEADER_ROW()
        {            
            var fv = GetTestFile("BCH_Format_BadFirstRow.txt", FOLDER);
            _TestExecution = JobExecution.GetSample(-1, 5, 24, 3, 1, 
                FilePath: fv.FullName, 
                ProcessingDate: new DateTime(2019, 06, 04)); //Note: Need to use a prod connection here for checking meta data
                                                             //(Testing specific profile to make sure it works... Need the meta data already configured to be able to test this with the incomplete first line, though)
            ExecuteTest();
            var expected = GetTestFile("BCH_Format_BadFirstRow.EXPECTED", FOLDER);
            AssertFileContent(expected, true);
        }
        [TestMethod]
        public void BOMTest()
        {
            var expected = GetTestFile("BOMTest.CLN", FOLDER);
            SetExecutionTestFile("BOMTest.txt", FOLDER);
            ExecuteTest();
            AssertFileContent(expected, true);
        }
        protected override void Init()
        {
            base.Init();
            int rc = _Manager.ExecuteTextNonQuery("DELETE FROM SEIDR.FileValidationJob WHERE JobProfile_JobID = -1"); //-1 record should not have a stored configuration kept.
        }
    }
}
