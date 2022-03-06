using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.FileSystem.PGP;
using System.Threading;
using System.IO;
using JobUnitTest.MockData;
using SEIDR.FileSystem;

namespace JobUnitTest
{
    [TestClass]
    public class PGPJobTest :  JobTestBase<PGPJob>
    {
        const int Generate_JobProfile_JobID = 500;
        const int Encrypt_JobProfile_JobID = 501;
        const int Decrypt_JobProfile_JobID = 502;
        const int EncryptSign_JobProfile_JobID = 503;
        const int Sign_JobProfile_JobID = 504;

        // Befor run tests DB configuration is needed in correspondance with these const        
        //UPDATE SEIDR.PGPJob SET PublicKeyFile = 'D:\Navigant\SEIDR_ROOT\SEIDR_VS2017_8\JobUnitTest\FileResource folder\public.key',
        //PrivateKeyFile = 'D:\Navigant\SEIDR_ROOT\SEIDR_VS2017_8\JobUnitTest\FileResource folder\private.key',
        //KeyIdentity = 'Cymetr1x', PassPhrase = 'S@turd@y' WHERE JobProfile_JobID IN (Encrypt_JobProfile_JobID, Decrypt_JobProfile_JobID, EncryptSign_JobProfile_JobID, Sign_JobProfile_JobID)

        DatabaseConnection dbConnection;
        DatabaseManager dbMgr;
        ExecutionStatus Status = new ExecutionStatus();
        PGPJob pgpJob = new PGPJob();

        private static readonly string InitTestRootResorcesPath = @"D:\Navigant\SEIDR_ROOT\SEIDR_VS2017_8\";

        static readonly string RegistrationFolder = @"D:\PGP_Test\_Registered\";

        private static readonly string PGP_TestFileDAILYTrans_FullFileName = InitTestRootResorcesPath + @"JobUnitTest\FileResource folder\PGP_TestFileDAILYTRANS20150220.txt";
        private static readonly string PGP_TestFile1_FullFileName = InitTestRootResorcesPath + @"JobUnitTest\FileResource folder\PGP_TestFile1.txt";

        public PGPJobTest()
        {
            dbConnection = new DatabaseConnection(@"NCIHCTSTSQL07\sql2014", "MIMIR");
            dbMgr = new DatabaseManager(dbConnection);
        }

        [TestInitialize()]
        public void Init()
        {
            Monitor.Enter(IntegrationTestsSynchronization.LockObject);

            if (Directory.Exists(RegistrationFolder))
            {
                Directory.Delete(RegistrationFolder, true);
            }

            Directory.CreateDirectory(RegistrationFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Monitor.Exit(IntegrationTestsSynchronization.LockObject);
        }

        #region ApplyDateMask tests 

        [TestMethod]
        public void ApplyDateMask_Encrypt_One_Job_Operation_Test()
        {
            //JobExecution job = new  JobProfile_JobID = Encrypt_JobProfile_JobID };
            /*
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName);
            File.Copy(PGP_TestFile1_FullFileName, execution_FilePath, true);
            job.FilePath = RegistrationFolder + "*_<MM><DD><YYYY><-1D>.txt";

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }

        [TestMethod]
        public void ApplyDateMask_Test()
        {

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><-1D>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                            @"D:\PGP_Test\_Registered\*_12312008.txt");

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><+1D>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                           @"D:\PGP_Test\_Registered\*_01022009.txt");

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><-1M>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                          @"D:\PGP_Test\_Registered\*_12012008.txt");
            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><+1M>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                          @"D:\PGP_Test\_Registered\*_02012009.txt");

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><-1Y>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                          @"D:\PGP_Test\_Registered\*_01012008.txt");

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><+1Y>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                              @"D:\PGP_Test\_Registered\*_01012010.txt");

            //--------------------------

            var r = FS.ApplyDateMask(RegistrationFolder + "*_<YYYY><MM><DD><-1D>.txt", DateTime.Parse("Jan 1, 2009"));

            Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<YYYY><MM><DD><-1D>.txt", DateTime.Parse("Jan 1, 2009")),
                                                                                           @"D:\PGP_Test\_Registered\*_20081231.txt");

            r = FS.ApplyDateMask(RegistrationFolder + "*_<YYYY><MM><DD><+1D>.txt", DateTime.Parse("Jan 1, 2009"));
            Assert.AreEqual<string>(r, @"D:\PGP_Test\_Registered\*_20090102.txt");

            r = FS.ApplyDateMask(RegistrationFolder + "*_<YYYY><MM><DD><-1D>.txt", DateTime.Parse("Jan 1, 2009"));
            Assert.AreEqual<string>(r, @"D:\PGP_Test\_Registered\*_20081231.txt");

            r = FS.ApplyDateMask(RegistrationFolder + "*_<YYYY><MM><DD><+1M>.txt", DateTime.Parse("Jan 1, 2009"));
            Assert.AreEqual<string>(r, @"D:\PGP_Test\_Registered\*_20090201.txt");

            //---------------------------------------------------------------------
            // Ecxeption
            //r = FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><-1D>.txt", DateTime.Parse("Jan 1, 0001"));
            //Assert.AreEqual<string>(r, @"D:\PGP_Test\_Registered\*_20090201.txt");

            // var r0 = FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><YYYY><-3000Y>.txt", DateTime.Parse("Jan 1, 2009"));
            //-----------------------------------------------------------------------

            //
            // Incorrect mask
            // var res = FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><fYYY><+aD>.txt", DateTime.Parse("Jan 1, 2009"));

            //Assert.AreEqual<string>(FS.ApplyDateMask(RegistrationFolder + "*_<MM><DD><fYYY><+1D>.txt", DateTime.Parse("Jan 1, 2009")),
            //                                                                               @"D:\PGP_Test\_Registered\*_01022009.txt");
        }
        #endregion

        [TestMethod]
        public void GenerateKey_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = Generate_JobProfile_JobID };
         
            TestExecutor executor = new TestExecutor();

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);            
            */
        }

        [TestMethod]
        public void Encrypt_OneOperation_FilePath_Valid_Job_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = Encrypt_JobProfile_JobID };
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName);
            File.Copy(PGP_TestFile1_FullFileName, execution_FilePath, true);
            job.FilePath = execution_FilePath;

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }

        [TestMethod]
        public void Decrypt_OneOperation_FilePath_Valid_Job_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = Decrypt_JobProfile_JobID };
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp";
            File.Copy(PGP_TestFile1_FullFileName + ".pgp", execution_FilePath, true);
            job.FilePath = execution_FilePath;

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }

        [TestMethod]
        public void SignEncrypt_OneOperation_FilePath_Valid_Job_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = EncryptSign_JobProfile_JobID };
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName);
            File.Copy(PGP_TestFile1_FullFileName, execution_FilePath, true);
            job.FilePath = execution_FilePath;

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }


        [TestMethod]
        public void Sign_OneOperation_FilePath_Valid_Job_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = Sign_JobProfile_JobID };
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName);
            File.Copy(PGP_TestFile1_FullFileName, execution_FilePath, true);
            job.FilePath = execution_FilePath;

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }

        [TestMethod]
        public void FileMask_Encrypt_OneOperation_Test()
        {
            /*
            JobExecution job = new PGPJobTest() { JobProfile_JobID = Encrypt_JobProfile_JobID };
            TestExecutor executor = new TestExecutor();

            var execution_FilePath = RegistrationFolder + Path.GetFileName(PGP_TestFile1_FullFileName);
            File.Copy(PGP_TestFile1_FullFileName, execution_FilePath, true);
            job.FilePath = RegistrationFolder + "*_<MM><DD><YYYY><-1D>.txt";

            Status = new ExecutionStatus();
            bool result = pgpJob.Execute(executor, job, ref Status);
            Assert.IsTrue(result);
            Assert.IsTrue(!Status.IsError);
            */
        }
    }
}
