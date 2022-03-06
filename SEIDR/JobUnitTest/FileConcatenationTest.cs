using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.FileSystem.FileConcatenation;
using SEIDR.JobBase;

namespace JobUnitTest
{
    [TestClass]
    public class FileConcatenationTest : JobTestBase<FileConcatenationJob>
    {
        ExecutionStatus Status = new ExecutionStatus();
        private TestExecutor test => _Executor;

        [TestMethod]
        public void DoConcatenation1()
        {
            JobExecution je = new JobExecution { FilePath = @"C:\SEIDR\Concat1.txt" };
            FileConcatenationSettings cs = new FileConcatenationSettings
            {
                SecondaryFileHasHeader = true,
                HasHeader = true,
                SecondaryFilePath = @"C:\SEIDR\Concat2.txt",
                OutputPath = @"C:\SEIDR\Output1<YYYY><MM><DD>.txt"
            };
            FileConcatenationJob j = new FileConcatenationJob();
            j.DoConcatenation(je, cs);
        }
        [TestMethod]
        public void DoConcatenation1Noheader()
        {
            JobExecution je = new JobExecution { FilePath = @"C:\SEIDR\Concat1Noheader.txt" };
            FileConcatenationSettings cs = new FileConcatenationSettings
            {
                SecondaryFileHasHeader = true,
                HasHeader = false,
                SecondaryFilePath = @"C:\SEIDR\Concat2.txt",
                OutputPath = @"C:\SEIDR\Output2<YYYY><MM><DD>.txt"
            };
            FileConcatenationJob j = new FileConcatenationJob();
            j.DoConcatenation(je, cs);
        }
        [TestMethod]
        public void DoConcatenationNoHeader()
        {

            JobExecution je = new JobExecution { FilePath = @"C:\SEIDR\Concat1Noheader.txt" };
            FileConcatenationSettings cs = new FileConcatenationSettings
            {
                SecondaryFileHasHeader = false,
                HasHeader = false,
                SecondaryFilePath = @"C:\SEIDR\Concat2NoHeader.txt",
                OutputPath = @"C:\SEIDR\OutputNoHeader<YYYY><MM><DD>.txt"
            };
            FileConcatenationJob j = new FileConcatenationJob();
            j.DoConcatenation(je, cs);
        }
    }
}
