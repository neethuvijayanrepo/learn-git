using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.FileSystem.FileSort;

namespace JobUnitTest
{
    /// <summary>
    /// Summary description for FileSortJobTest
    /// </summary>
    [TestClass]
    public class FileSortJobTest : JobTestBase<FileSortJob>
    {
        

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        const string FOLDER = "FileSorting";
       
        /*
         ToDo: FileSorting Folder, Test File + Expected output for comparison. Small + Large versions. 
         Perhaps split the logic of the sort job into another method so that it can be called from a test in a more straightforward fashion. 
         Right now this would fail, but it would have before anyway, since the assets were not available to anyone other than Darshan and only on that specific computer, pointing to that specific DB.
             */
        [TestMethod]
        public void TestFileSort()
        {
            //JobExecution job = new FileSortJobTest() { JobProfile_JobID = 52 };
            //TestExecutor test = new TestExecutor();
            //job.FilePath = @"D:\Darshan\Test\FileSortJob\CleanMoneField5.TXT";
            SetExecutionTestFile("testUnsortedFile.txt", FOLDER);            
            var expected = GetTestFile("ExpectedSort.txt", FOLDER);
            AssertFileContent(expected, true);
            //Assert.IsTrue(fsJob.Execute(test, job, ref Status));
        }
    }
}
