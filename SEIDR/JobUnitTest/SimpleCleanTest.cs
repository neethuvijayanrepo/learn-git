using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;
using SEIDR.FileSystem.SimpleClean;

namespace JobUnitTest
{
    [TestClass]
    public class SimpleCleanTest : ContextJobTestBase<SimpleCleanJob, FileSystemContext>
    {

        SEIDR.JobBase.ExecutionStatus DoCall(string file, 
            SimpleCleanConfiguration config = null, 
            int? minLineLength = null, int? MaxLineLength = null,
            int? codePage = null, 
            bool DoTrailer = false, 
            string extension = "CLN", 
            int? BlockSize = null)
        {
            var cs = new SimpleCleanJob();
            if(config == null)
            {
                config = new SimpleCleanConfiguration
                {
                    Line_MinLength = minLineLength,
                    Line_MaxLength = MaxLineLength,
                    CodePage = codePage,
                    AddTrailer = DoTrailer,
                    Extension = extension
                };
            }
            if (BlockSize != null)
                config.BlockSize = BlockSize;
            else if (config.BlockSize == null)
                config.BlockSize = SEIDR.Doc.DocMetaData.DEFAULT_PAGE_SIZE;
            
            cs.DoClean(file, config, MyContext);
            _JOB.FinalizeWorkingFile(MyContext);
            return MyContext.ResultStatus;
        }
        //ToDo: Dummy files that are in source control/unit test project.

        [TestMethod]
        public void SimpleCleanRaggedRight_FailLength()
        {
            var stat = DoCall(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\MemorialHermann\Notes\NE\NEMEMO.D181203", minLineLength: 1700);
            Assert.AreEqual("LL", stat.ExecutionStatusCode);
            Assert.IsTrue(stat.IsError);
        }



        [TestMethod]
        public void SimpleCleanRaggedRight_FailLength_Exceed()
        {            
            var stat = DoCall(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\MemorialHermann\Notes\NE\NEMEMO.D181203", MaxLineLength: 10);
            Assert.AreEqual("HL", stat.ExecutionStatusCode);
            Assert.IsTrue(stat.IsError);
                
        }



        [TestMethod]
        public void SimpleCleanRaggedRight()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\MemorialHermann\Notes\NE\NEMEMO.D181203", minLineLength: 16, codePage: 1252);
        }


        [TestMethod]
        public void SimpleCleanDelimited()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\WellstarWestGeorgia\Cymetrix_Cancellation_File\Cymetrix_Cancellation_File_2018_07_27_030310.txt");
        }
        [TestMethod]
        public void SimpleCleanRaggedRight2()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\MemorialHermann\Transaction\NEPAY.D181207", codePage: 1252);
        }
        [TestMethod]
        public void SimpleCleanRaggedRightWithTrailer()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\MemorialHermann\Transaction\NEPAY.D181207", codePage: 1252, DoTrailer: true, extension: "TRLR_CLN");
        }

        //Test when the block size ends evenly on a line ending
        [TestMethod]
        public void SimpleCleanRaggedRightWithTrailer_LineLengthBlock()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\MemorialHermann\Transaction\NEPAY.D181207", 
                codePage: 1252, DoTrailer: true, extension: "TRLR_CLN_LNLBLCK", BlockSize: 200);
        }
        //Test when the block size ends before a line ends
        [TestMethod]
        public void SimpleCleanRaggedRightWithTrailer_SmallBlock()
        {
            DoCall(@"\\sdsrv031.cymetrix.com\IS\DATA\_SourceFiles\MemorialHermann\Transaction\NEPAY.D181207",
                codePage: 1252, DoTrailer: true, extension: "TRLR_CLN_SMLBLCK", BlockSize: 150);
        }
        [TestMethod]
        public void EmptyBlockTest()
        {
            var test = @"C:\SEIDR\TestEmptyBlockTest.txt";
            using(var sw = new System.IO.StreamWriter(test, false))
            {
                sw.WriteLine("Header Line bla bla");
                for (int i = 0; i < 500; i++)
                    sw.WriteLine(Environment.NewLine);
                sw.WriteLine("NextLine");
                sw.WriteLine("ThirdLine");
                sw.WriteLine("END");
            }
            DoCall(test, DoTrailer: true, BlockSize: 210);
            Assert.AreEqual("Header Line bla bla\r\nNextLine\r\nThirdLine\r\nEND\r\nTRAILER:TestEmptyBlockTest.txt.CLN    LineCount:4", System.IO.File.ReadAllText(test + ".CLN"));
        }

        [TestMethod]
        public void EmptyBlockStart_Test()
        {
            var test = @"C:\SEIDR\TestEmptyBlockTest.txt";
            using (var sw = new System.IO.StreamWriter(test, false))
            {
                for (int i = 0; i < 500; i++)
                    sw.WriteLine(Environment.NewLine);
                sw.WriteLine("Header Line bla bla");
                sw.WriteLine("NextLine");
                sw.WriteLine("ThirdLine");
                sw.WriteLine("END");
            }
            DoCall(test, DoTrailer: true, BlockSize: 210);
            Assert.AreEqual("Header Line bla bla\r\nNextLine\r\nThirdLine\r\nEND\r\nTRAILER:TestEmptyBlockTest.txt.CLN    LineCount:4", System.IO.File.ReadAllText(test + ".CLN"));
        }


        [TestMethod]
        public void MissingNewLinePostClean()
        {
            _TestExecution.SetFileInfo(@"C:\temp\HMC_itemizedcharges_20200211.txt");
            ExecuteTest();
        }

    }
}
