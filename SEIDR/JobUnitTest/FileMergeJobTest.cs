using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;
using SEIDR.FileSystem.FileConcatenation;

namespace JobUnitTest
{
    [TestClass]
    public class FileMergeJobTest : ContextJobTestBase<FileMergeJob, FileSystemContext>
    {
        [TestMethod]
        public void TestMergeInnerJoin()
        {
            FileMergeJobSettings settings = new FileMergeJobSettings
            {

            };
            doMerge(settings);
        }
        public void doMerge(FileMergeJobSettings settings)
        {
            var mock = MockManager.NewMockModel<FileMergeJobSettings>();
            mock.MapToNewRow(settings);
            Assert.IsTrue(ExecuteTest());
        }
        protected override void Init()
        {
            base.Init();
            //get a file from the FileSystem folder in JobUnit Test project, named MergeTestLeft.txt
            SetExecutionTestFile("MergeTestLeft.txt", "FileSystem"); 
        }
    }
}
