using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.FileSystem;
using SEIDR.FileSystem.FileValidation;
using SEIDR.JobBase;

namespace JobUnitTest
{
    public partial class FileSystemJobTests
    {
        //TODO: Majority of tests need to be rewritten in a way that is environment agnostic. Preferably using the Test Base.

        [TestMethod]
        public void RunDummy()
        {
            Assert.IsTrue(Test(Dummy));
            //UpdateExecutionPath = true, so FilePath on the JobExecution is going to be the source from here.
            Assert.AreEqual(Dummy.OutputPath, MyContext.FilePath);
            Assert.IsTrue(File.Exists(Dummy.OutputPath));
        }

        [TestMethod]
        public void RunSimpleCleanCopyTest()
        {
            var source = GetTestFile("FileToClean.csv", "FileSystem");
            var dest = PrepSubFolder("DEST");
            FS helper = new FS
            {
                Operation = FileOperation.CLEAN_COPY,
                OutputPath = dest.FullName,
                Source = source.FullName,
                UpdateExecutionPath = true,
                Overwrite = true
            };
            helper.Process(MyContext);
            //Even though it's being cleaned, we're going to still have the original file name in the output folder.
            string expectedPath = Path.Combine(dest.FullName, source.Name); 
            Assert.AreEqual(expectedPath, _TestExecution.FilePath, true);
            var actual = new FileInfo(expectedPath);
            AssertFileContent("FileToClean.csv.CLN", actual, false, "FileSystem");
            string hashClean = SEIDR.Doc.DocExtensions.GetFileHash(actual);


            helper.Operation = FileOperation.COPY;
            helper.Source = source.FullName; //Reset because same object.
            helper.Process(MyContext);
            Assert.AreEqual(expectedPath, _TestExecution.FilePath, true);
            actual.Refresh();
            //Will be a different hash due to not going through the simple clean process
            string hashNormal = SEIDR.Doc.DocExtensions.GetFileHash(actual); 
            Assert.AreNotEqual(hashClean, hashNormal);
        }

        [TestMethod(), ExpectedException(typeof(IOException))]
        public void ProcessMoveTest()
        {
            //ToDo: this test doesn't really work as expected. Reevaluate intent
            FS FileSystemHelper = new FS
            {
                Operation = FileOperation.MOVE,
                OutputPath = DESTINATION_TEST_FOLDER,
                Overwrite = true,
                UpdateExecutionPath = true
            };
            cleanup();
            RunDummy();
            FileSystemContext context = new FileSystemContext();
            context.Init(_Executor, batch);
            FileSystemHelper.Process(context);
            bool success = context.Success;
            Assert.AreNotEqual(Dummy.OutputPath, batch.FilePath);
            Assert.IsTrue(success);
            AssertCheckBatchPath();

            RunDummy(); //Overwrite
            //context.Init(_Executor, batch);
            FileSystemHelper.Process(context);
            success = context.Success;
            Assert.AreNotEqual(Dummy.OutputPath, batch.FilePath);
            Assert.IsTrue(success);

            FileSystemHelper.Overwrite = false;
            RunDummy();
            //No overwrite, IO Exception.
            FileSystemHelper.Process(context);
            //Should not reach.
            Assert.Fail();
        }
        void AssertCheckBatchPath(SEIDR.JobBase.JobExecution job) => Assert.IsTrue(CheckBatchPath(job));
        void AssertCheckBatchPath() => AssertCheckBatchPath(batch);
        /// <summary>
        /// Check that the FilePath associated with the JobExecution <paramref name="job"/> exists
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        bool CheckBatchPath(SEIDR.JobBase.JobExecution job) => File.Exists(job.FilePath);
        

        [TestMethod(), ExpectedException(typeof(IOException))]
        public void ProcessCopyTest1() => ProcessCopyTest(false);
        [TestMethod(), ExpectedException(typeof(IOException))]
        public void ProcessCopyTest2() => ProcessCopyTest(true);

        public void ProcessCopyTest(bool updatePath)
        {
            FS FileSystemHelper = new FS
            {
                Operation = FileOperation.COPY,
                OutputPath = DESTINATION_TEST_FOLDER,
                Overwrite = true,
                UpdateExecutionPath = updatePath
            };
            if (!updatePath)
                FileSystemHelper.Source = Dummy.OutputPath;

            cleanup();
            RunDummy();
            MyContext.Init(_Executor, batch);
            bool success = Test(FileSystemHelper);
            Assert.AreEqual(updatePath, FileSystemHelper.UpdateExecutionPath);
            if (!updatePath)
                Assert.AreEqual(Dummy.OutputPath, batch.FilePath);
            Assert.IsTrue(success);
            AssertCheckBatchPath();

            
            success = Test(FileSystemHelper);
            Assert.IsTrue(success);

            FileSystemHelper.Overwrite = false;
            //No overwrite, IO Exception
            FileSystemHelper.Process(MyContext);
            //Should not reach
            Assert.Fail();
        }
        [TestMethod]
        public void DateMaskTest()
        {
            DateTime Process = DateTime.Today;
            string s = FS.Combine(JobFolder, "charges_<YYYY><MM><DD><-1D>.TXT");
            string source = FS.ApplyDateMask(s, Process);
            string d = @"\\RandomFolder\*";
            string dest = FS.ReplaceStar(d, Path.GetFileName(source));
            MyContext.LogInfo(source);
            MyContext.LogInfo(dest);
        }

        /* Test Cases For Zip , UNZip and COPY_METRIX Operations */

        //Run Zip If the Source and Destination is available
        [TestMethod]
        public void RunZip()
        {
            CreateRandomFile();
            CreateRandomFile();
            FS ZIP = new FS
            {
                Operation = FileOperation.ZIP,
                FileFilter = "*.txt",
                Source = JobFolder,
                OutputPath = FS.Combine(JobFolder, "<YYYY>_<MM>_<DD>.Zip"),
                Overwrite = true
            };
            
            Assert.IsTrue(Test(ZIP));
        }

        //Run Zip If the Destination is not available
        [TestMethod]
        public void RunZipND()
        {

            var fz = GetTestFile("TestTwoFiles.zip", nameof(SEIDR.FileSystem));
            FS ZIP = new FS
            {
                Operation = FileOperation.ZIP,
                Source = fz.FullName,
                OutputPath = null,
                Overwrite = true
            };
            Test(ZIP);
            Assert.AreEqual(ResultStatusCode.ND, MyContext.ResultCode);
        }

        //Run UNZip If the Source and  Destination is  available
        [TestMethod]
        public void RunUNZip()
        {
            var fz = GetTestFile("TestTwoFiles.zip", nameof(SEIDR.FileSystem));
            FS UNZIP = new FS
            {
                Operation = FileOperation.UNZIP,
                Source = fz.FullName,
                OutputPath = JobFolder,
                Overwrite = true
            };
            Assert.IsTrue(Test(UNZIP));
        }

        //Run UNZip If the Source file  is not   available
        [TestMethod]
        public void RunUNZipNS()
        {
            FS UNZIP = new FS
            {
                Operation = FileOperation.UNZIP,
                Source= JobFolder,
                OutputPath = @"C:\SEIDR\Destination",
                Overwrite = true
            };
            UNZIP.Process(MyContext);
            Assert.AreEqual(ResultStatusCode.NS, MyContext.ResultCode);
        }

        //Run UNZip If the  Destination is not available
        [TestMethod]
        public void RunUNZipND()
        {
            var fz = GetTestFile("TestTwoFiles.zip", nameof(SEIDR.FileSystem));
            FS UNZIP = new FS
            {
                Operation = FileOperation.UNZIP,
                Source = fz.FullName,
                OutputPath = null,
                Overwrite = true
            };
            UNZIP.Process(MyContext);
            Assert.AreEqual(ResultStatusCode.ND, MyContext.ResultCode);
        }

        //Run Copy Metrix If the Source & Destination is  available
        [TestMethod]
        public void RunCopyMetrix()
        {
            var subDir = PrepSubFolder("INPUT");
            var source = CreateRandomFile();
            _TestExecution.SetUserKey1("A");
            _TestExecution.SetLoadProfileID(123);
            const int EXPECTED_LOADBATCHID = 1234;

            var copyMetrix = new FS
            {
                Operation = FileOperation.COPY_METRIX,
                Source = source.FullName,
                Overwrite = true,
                LoadProfileID = 123,
                FileFilter = "*.*",
                DatabaseLookUpID = 1
            };
            var profileMap = new
            {
                InputFolder = subDir.FullName,
                LoadBatchTypeCode = _TestExecution.UserKey1,
                LoadProfileID = _TestExecution.LoadProfileID.Value,
                FileNameFilter = copyMetrix.FileFilter
            };
            var staging = (MockDatabaseManager) _Executor.GetManager(1);
            staging.DefaultSchema = "STAGING";
            var loadProfile = staging.NewMockModel("usp_LoadProfile_ss_load", "STAGING");
            loadProfile.MapToNewRow(profileMap);

            var sequence = staging.NewMockModel("usp_LoadBatch_CheckSequence");
            sequence.SetOutParameter("CanLoad", true)
                    .SetOutParameter("OutOfSequenceOld", false)
                    .SetOutParameter("OutOfSequenceReason", string.Empty);

            var register = staging.NewMockModel("usp_LoadBatch_Register");
            register.SetOutParameter("CanLoad", true)
                    .SetOutParameter("LoadBatchID", EXPECTED_LOADBATCHID);

            staging.NewMockModel("usp_Loader_SetStatus");

            Assert.IsTrue(Test(copyMetrix));
            Assert.AreEqual(EXPECTED_LOADBATCHID, _TestExecution.METRIX_LoadBatchID);
            string expectedFilePath = Path.Combine(subDir.FullName, "_Processed_", source.Name);
            Assert.IsTrue(File.Exists(expectedFilePath));
        }
        [TestMethod]
        public void CopyMetrix_KeyMismatch()
        {
            var subDir = PrepSubFolder("INPUT");
            var source = CreateRandomFile();
            _TestExecution.SetUserKey1("A");
            _TestExecution.SetLoadProfileID(123);
            const int EXPECTED_LOADBATCHID = 1234;

            var copyMetrix = new FS
            {
                Operation = FileOperation.COPY_METRIX,
                Source = source.FullName,
                Overwrite = true,
                LoadProfileID = 123,
                FileFilter = "*.*",
                DatabaseLookUpID = 1
            };
            var profileMap = new
            {
                InputFolder = subDir.FullName,
                LoadBatchTypeCode = "TX",
                LoadProfileID = _TestExecution.LoadProfileID.Value, //Same profile ID but user key/Load type mismatch
                FileNameFilter = copyMetrix.FileFilter
            };
            var staging = (MockDatabaseManager)_Executor.GetManager(1);
            staging.DefaultSchema = "STAGING";
            var loadProfile = staging.NewMockModel("usp_LoadProfile_ss_load", "STAGING");
            loadProfile.MapToNewRow(profileMap);

            var sequence = staging.NewMockModel("usp_LoadBatch_CheckSequence");
            sequence.SetOutParameter("CanLoad", true)
            .SetOutParameter("OutOfSequenceOld", false)
                .SetOutParameter("OutOfSequenceReason", string.Empty);

            var register = staging.NewMockModel("usp_LoadBatch_Register");
            register.SetOutParameter("CanLoad", true)
            .SetOutParameter("LoadBatchID", EXPECTED_LOADBATCHID);

            staging.NewMockModel("usp_Loader_SetStatus");

            Assert.IsFalse(Test(copyMetrix));
            Assert.AreEqual(ResultStatusCode.TM.ToString(), MyContext.ResultStatus.ExecutionStatusCode);
        }
        [TestMethod]
        public void CopyMetrix_KeyMismatch_DifferentProfile()
        {
            var subDir = PrepSubFolder("INPUT");
            var source = CreateRandomFile();
            _TestExecution.SetUserKey1("A");
            _TestExecution.SetLoadProfileID(123);
            const int EXPECTED_LOADBATCHID = 1234;

            var copyMetrix = new FS
            {
                Operation = FileOperation.COPY_METRIX,
                Source = source.FullName,
                Overwrite = true,
                LoadProfileID = 123,
                FileFilter = "*.*",
                DatabaseLookUpID = 1
            };
            var profileMap = new
            {
                InputFolder = subDir.FullName,
                LoadBatchTypeCode = "TX",
                LoadProfileID = _TestExecution.LoadProfileID.Value + 10, //user key/Load type mismatch, but a different LoadProfileID
                FileNameFilter = copyMetrix.FileFilter
            };
            var staging = (MockDatabaseManager)_Executor.GetManager(1);
            staging.DefaultSchema = "STAGING";
            var loadProfile = staging.NewMockModel("usp_LoadProfile_ss_load", "STAGING");
            loadProfile.MapToNewRow(profileMap);

            var sequence = staging.NewMockModel("usp_LoadBatch_CheckSequence");
            sequence.SetOutParameter("CanLoad", true)
                    .SetOutParameter("OutOfSequenceOld", false)
                    .SetOutParameter("OutOfSequenceReason", string.Empty);

            var register = staging.NewMockModel("usp_LoadBatch_Register");
            register.SetOutParameter("CanLoad", true)
                    .SetOutParameter("LoadBatchID", EXPECTED_LOADBATCHID);

            staging.NewMockModel("usp_Loader_SetStatus");

            Assert.IsTrue(Test(copyMetrix));
            Assert.AreEqual(EXPECTED_LOADBATCHID, _TestExecution.METRIX_LoadBatchID);
            string expectedFilePath = Path.Combine(subDir.FullName, "_Processed_", source.Name);
            Assert.IsTrue(File.Exists(expectedFilePath));
        }

    /*
    //Run Copy Metrix for Invalid profile
    [TestMethod]
    public void RunCopyMetrixIP()
    {
        FS CopyMetrix = new FS
        {
            Operation = FileOperation.COPY_METRIX,
            Source = @"C:\SEIDR\Source\TextDocument.txt",
            InputFolder = @"C:\SEIDR\Input\",
            FileFilter = "*.TXT",
            Overwrite = true,
            LoadProfileID = 411346,
            ServerName = @"NCIHCTSTSQL07.nciwin.local\SQL2014",
            DatabaseName = "Andromeda_Staging"
        };

        string s;
        Assert.IsTrue(CopyMetrix.Process(job, batch, mgr, out s));
    }

    //Run Copy Metrix if the Input folder/Destination is not available
    [TestMethod]
    public void RunCopyMetrixND()
    {
        FS CopyMetrix = new FS
        {
            Operation = FileOperation.COPY_METRIX,
            Source = @"C:\SEIDR\Source\TextDocument.txt",
            InputFolder = null,
            FileFilter = "*.TXT",
            Overwrite = true,
            LoadProfileID = 41135,
            ServerName = @"NCIHCTSTSQL07.nciwin.local\SQL2014",
            DatabaseName = "Andromeda_Staging"
        };

        string s;
        Assert.IsTrue(CopyMetrix.Process(job, batch, mgr, out s));
    }

    //Run Copy Metrix if the Input folder/Destination is not available
    [TestMethod]
    public void RunCopyMetrixFF()
    {
        FS CopyMetrix = new FS
        {
            Operation = FileOperation.COPY_METRIX,
            Source = @"C:\SEIDR\Source\TextDocument.txt",
            InputFolder = @"C:\SEIDR\Input\",
            FileFilter = null,
            Overwrite = true,
            LoadProfileID = 41136,
            ServerName = @"NCIHCTSTSQL07.nciwin.local\SQL2014",
            DatabaseName = "Andromeda_Staging"
        };

        string s;
        Assert.IsTrue(CopyMetrix.Process(job, batch, mgr, out s));
    }


    //Run Copy Metrix If the Source file is not available
    [TestMethod]
    public void RunCopyMetrixNS()
    {
        FS CopyMetrix = new FS
        {
            Operation = FileOperation.COPY_METRIX,
            Source = @"C:\SEIDR\Source\",
            InputFolder = @"C:\SEIDR\Input\",
            FileFilter = "*.TXT",
            Overwrite = true,
            LoadProfileID = 41134,
            ServerName = @"NCIHCTSTSQL07.nciwin.local\SQL2014",
            DatabaseName = "Andromeda_Staging"
        };

        string s;
        Assert.IsTrue(CopyMetrix.Process(job, batch, mgr, out s));
    }

    //Test cases using database Connection and Database values at runtime
    [TestMethod]
    public void RunZipJob()
    {
        var Manager = new DatabaseManager(this.c, DefaultSchema: "SEIDR");
        using (var h = Manager.GetBasicHelper())
        {
            h.QualifiedProcedure = "SEIDR.usp_FileSystem_ss_JobExecution";
            h["JobProfile_JobID"] = 14;

            //Get the configuration for this step.
            var fs = Manager.Execute(h).GetFirstRowOrNull().ToContentRecord<FS>(); //equivalent: Manager.SelectSingle<FS>(h);  
            string statCode = null;
            bool success;
            success = fs.Process(this.job, this.batch, Manager, out statCode);
        }
    }

    [TestMethod]
    public void RunUnZipJob()
    {
        var Manager = new DatabaseManager(this.c, DefaultSchema: "SEIDR");
        using (var h = Manager.GetBasicHelper())
        {
            h.QualifiedProcedure = "SEIDR.usp_FileSystem_ss_JobExecution";
            h["JobProfile_JobID"] = 15;

            //Get the configuration for this step.
            var fs = Manager.Execute(h).GetFirstRowOrNull().ToContentRecord<FS>(); //equivalent: Manager.SelectSingle<FS>(h);  
            string statCode = null;
            bool success;
            success = fs.Process(this.job, this.batch, Manager, out statCode);
        }
    }


    [TestMethod]
    public void CopyToMetrixJob()
    {
        var Manager = new DatabaseManager(this.c, DefaultSchema: "SEIDR");
        using (var h = Manager.GetBasicHelper())
        {
            h.QualifiedProcedure = "SEIDR.usp_FileSystem_ss_JobExecution";
            h["JobProfile_JobID"] = 16;

            //Get the configuration for this step.
            var fs = Manager.Execute(h).GetFirstRowOrNull().ToContentRecord<FS>(); //equivalent: Manager.SelectSingle<FS>(h);  
            string statCode = null;
            bool success;
           success = fs.Process(this.job, this.batch, Manager, out statCode);


        }

    }

    [TestMethod]
    public void MoveToMetrixJob()
    {
        JobExecution je = new JobExecution();

        FS MoveToMetrixJob = new FS
        {
            Operation = FileOperation.MOVE_METRIX,
            Source = @"D:\SEIDR DEMO\A.txt",
            InputFolder = @"D:\SEIDR DEMO\Test\",
            LoadProfileID = 40246,
            FileFilter = "*.TXT",
            Overwrite = true,
            ServerName = @"NCIMTXDEVSE01",
            DatabaseName = "Andromeda_Staging_LEGACY"
        };


        string s;
        Assert.IsTrue(MoveToMetrixJob.Process(job, batch, mgr, out s));

    }

    */
}

}
