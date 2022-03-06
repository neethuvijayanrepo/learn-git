using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR;
using SEIDR.JobBase;
using SEIDR.DataBase;
using System.IO;


namespace JobUnitTest
{
    [TestClass()]
    public partial class FileSystemJobTests: ContextJobTestBase<FileSystemJob, FileSystemContext>
    {
        //ToDo: Recreate - do at least one test per operation (can skip the renames, e.g. GRAB_ALL is functionally the same as MOVE_ALL)
        JobExecution batch = new JobExecution();
        
        FS Dummy = new FS
        {
            Operation = FileOperation.CREATE_DUMMY,
            OutputPath = @"C:\SEIDR\Dummy.txt",
            Overwrite = true,
            UpdateExecutionPath = true
        };
        #region constants
        const string DUMMY_FOLDER_TEST = @"C:\SEIDR\<YYYY><MM>_<DD>\Test\";
        const string DUMMY_DESTINATION = @"C:\SEIDR\<YYYY><MM>_<DD>\";
        const string DESTINATION_TEST_FOLDER = @"C:\SEIDR\DestinationTest\";
        #endregion
        [TestMethod]
        public void LocalFileHelperTest()
        {
            var dir = ClearLocalWorkingDirectory();
            var originalID = MyContext.JobExecutionID;
            var test = base.CreateRandomFile();
            var file = MyContext.GetLocalFile(test.FullName);
            var test2 = CreateRandomFile();
            var file2 = MyContext.GetLocalFile(test2.FullName); //Pass a file path to have a file in the working directory.
            _TestExecution.SetJobExecutionID(3);
            var file3 = MyContext.GetLocalFile(test.FullName);
            _TestExecution.SetJobExecutionID(originalID);
            BasicLocalFileHelper.ClearWorkingDirectory(MyContext);

            Assert.AreEqual(1, dir.GetFiles().Count());
            File.Delete(file3);
        }

        /// <summary>
        /// Just testing ignore attribute, not actual test, although it should be run at the start of tests..
        /// </summary>
        [TestMethod, Ignore]
        public void cleanup()
        {
            if (File.Exists(DESTINATION_TEST_FOLDER + "Dummy.txt"))
                File.Delete(DESTINATION_TEST_FOLDER + "Dummy.txt");            
        }

        [TestMethod]
        public void WildCardTest()
        {
            var fi = CreateFile($"ear_{ProcessingDate:yyyyMMdd}_PBS_21332.A.txt");
            string path = FS.Combine(JobFolder, "EAR_<YYYY><MM><DD>_PBS*.A.TXT");
            FS f = new FS
            {
                Source = path,
                Operation = FileOperation.CHECK,
                UpdateExecutionPath = true,
            };
            Assert.IsTrue(Test(f));
        }

        [TestMethod]
        public void WildCardTest2()
        {
            var fi = CreateFile($"EOS_{ProcessingDate:yyyyMMdd}_21325asf_bla bla bla.A.TXT.IC.CYM");
            string path = FS.Combine(JobFolder, "EOS_<YYYY><MM><DD>_*.A.TXT.IC.CYM");
            var dir = PrepSubFolder("COPY_OTHER_FOLDER", false);
            FS f = new FS
            {
                Source = path,
                Operation = FileOperation.COPY,
                UpdateExecutionPath = true,
                OutputPath = dir.FullName
            };
            Assert.IsTrue(Test(f));
        }

        public bool Test(FS f)
        {
            f.Process(MyContext);
            return MyContext.Success;
            //return !MyContext.SetStatus(f.Process(MyContext)).IsError;
        }

    }
}