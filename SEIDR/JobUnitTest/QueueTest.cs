using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR;
using SEIDR.DataBase;
using SEIDR.JobExecutor;

namespace JobUnitTest
{
    [TestClass]
    public class QueueTest: TestBase
    {
        Queue q;
        SEIDR.JobBase.JobProfile testProfile;
        public QueueTest()
        {
            _Service.ExecutionThreadCount = 0;          
            q = new Queue(_Service, _Manager);
#if !DEBUG
            throw new Exception("Can only test Queue in debug mode.");
#endif
        }
        [TestMethod]
        public void TestDateMaskedDestination()
        {

            Prep("TEST", DestinationFolderName: @"<YYYY><MM>_<DD><CC>\Destination");

            TestCall();
            

        }
        [TestMethod]
        public void TestDateMaskedDestination_Watch()
        {
            Prep("TEST", DestinationFolderName: @"<YYYY><MM>_<DD><CC>\Destination");

            PrepFileCreation("File20190104.txt"); //Create a file for 2019/01/04 to be moved to a folder based on this name
            TestCall();

            DirectoryInfo di = new DirectoryInfo(BASE_DESTINATION_ROOT + "\\201901_0420\\Destination");
            Assert.IsTrue(di.Exists);

        }/*
        [TestMethod]
        public void TestNetworkAccess()
        {
            try
            {
                testProfile = null;
                //testProfile = dbm.SelectSingle<SEIDR.JobBase.JobProfile>(new { JobProfileID = 430 }, Schema: "SEIDR");                
                TestCall();               
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Assert.Fail();
            }
        }*/
        #region UTILITY procedures for testing
        void TestCall()
        {
#if DEBUG
            // Method that only needs to be exposed for debug purposes, not for production purposes. Only available in debug to ensure it's only used for testing purposes.
            q.TestWork(testProfile); //Note: JobProfileID will be null, so it's not going to be able to actually register...
#else
            Assert.Fail();
#endif

        }
        const string BASE_ROOT = @"C:\SEIDR\FileWatch";
        const string BASE_DESTINATION_ROOT = BASE_ROOT + @"\DESTINATIONS";
        /// <summary>
        /// Prepare a JobProfile to use for testing the queue. Turns off Database modification (<see cref="SEIDR.JobBase.RegistrationFile.SetNoRegisterMode"/>) 
        /// <para>
        /// Also clean up the folders from the Root ( or SubDirectory of <see cref="BASE_ROOT"/> )
        /// </para>
        /// </summary>
        /// <param name="SubDirectory"></param>
        /// <param name="fileFilter"></param>
        /// <param name="exclusionFilter"></param>
        /// <param name="FileDateMask"></param>
        /// <param name="DestinationFolderName"></param>
        private void Prep(string SubDirectory = null, string fileFilter = "*.*", string exclusionFilter = null, string FileDateMask = "<YYYY><MM><DD>", string DestinationFolderName = null, bool CleanDirectories = true)
        {
            
            SEIDR.JobBase.JobProfile test = new SEIDR.JobBase.JobProfile();
            test.SetJobProfileID(-1); //DEBUG ONLY.
            string root = BASE_ROOT;
            if (SubDirectory != null)
                root = Path.Combine(root, SubDirectory);
            DirectoryInfo di = new DirectoryInfo(root);
            if (!di.Exists)
                di.Create();
            else if (CleanDirectories)
            {
                di.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete()); //Clean out files
                di.EnumerateDirectories().ForEach(d => d.Delete(true)); //Clean subdirectories.
            }
            test.FileFilter = fileFilter;
            test.FileExclusionFilter = exclusionFilter;
            test.FileDateMask = FileDateMask;
            test.RegistrationFolder = root;

            if (DestinationFolderName != null)
            {
                if (DestinationFolderName.Contains("<"))
                {
                    if (!DestinationFolderName.StartsWith("\\"))
                        DestinationFolderName = "\\" + DestinationFolderName; // Put a \ at the beginning

                    test.RegistrationDestinationFolder = BASE_DESTINATION_ROOT + DestinationFolderName;
                    if (CleanDirectories)
                    {
                        var d2 = new DirectoryInfo(BASE_DESTINATION_ROOT);
                        if (d2.Exists)
                        {
                            d2.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete());
                            d2.EnumerateDirectories().ForEach(d => d.Delete(true));
                        }
                    }
                }
                else
                {
                    string droot = Path.Combine(BASE_DESTINATION_ROOT, DestinationFolderName);
                    test.RegistrationDestinationFolder = droot;
                    DirectoryInfo d2 = new DirectoryInfo(droot);
                    if(d2.Exists && CleanDirectories)
                    {
                        d2.EnumerateFiles("*.*", SearchOption.AllDirectories).ForEach(fi => fi.Delete());
                        d2.EnumerateDirectories().ForEach(d => d.Delete(true));
                    } //Clean registration destination if possible.
                }
            }
            testProfile = test;
            SEIDR.JobBase.RegistrationFile.SetNoRegisterMode(); //Note: if testProfile is null, then database access will be used to pull list of profiles to do file watch with            
        }
        private void PrepFileCreation(params string[] FileNameList) => TestFileCreation(FileNameList);
        private void TestFileCreation(params string[] FileNameList)
        {
            foreach(string file in FileNameList)
            {
                string fullPath = Path.Combine(testProfile.RegistrationFolder, file);
                File.WriteAllText(fullPath, file);
            }
        }
        public string GetRegistrationFilePath(string Name)
        {
            return Path.Combine(testProfile.RegistrationFolder, Name);
        }
        public void AssertRegistrationFilePath(string FileName)
        {
            if (!File.Exists(GetRegistrationFilePath(FileName)))
                throw new FileNotFoundException($"Expected File '{FileName}' not found in file watch folder.");
        }
        public string GetRegistrationDestinationFilePath(string Name)
        {
            if (testProfile.RegistrationDestinationFolder != null)
                return Path.Combine(testProfile.RegistrationDestinationFolder, Name);

            return Path.Combine(testProfile.RegistrationFolder, Queue.REGISTERED_FOLDER_NAME, Name);
        }
        public void AssertRegistrationDestinationFilePath(string FileName)
        {
            if (!File.Exists(GetRegistrationDestinationFilePath(FileName)))
                throw new FileNotFoundException($"Expected File '{FileName}' not found in destination.");
        }
        [TestMethod]
        public void TestExclusionFilter()
        {
            const string T1_E = "test.csv";
            const string T2_E = "test.txt";
            const string T3 = "SEIDR.csv";
            const string T4_E = "SEIDR.file";
            const string T5 = "SEIDR.txt";
            try
            {
                Prep(exclusionFilter: "test.*;SEIDR.file", 
                        fileFilter: "*.txt;*.csv;*.file");
                TestFileCreation(T1_E, T2_E, T3, T4_E, T5);
                Queue.CUTOVER = 0; //Allow moving these files.

                TestCall();
                AssertRegistrationFilePath(T1_E);
                AssertRegistrationFilePath(T2_E);
                AssertRegistrationDestinationFilePath(T3);
                AssertRegistrationFilePath(T4_E);
                AssertRegistrationDestinationFilePath(T5);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestNoExclusionFilter()
        {
            const string T1 = "test.csv";
            const string T2 = "test.txt";
            const string T3 = "test.file";
            try
            {
                Prep(fileFilter: "*.csv;*.txt");
                TestFileCreation(T1, T2, T3);
                Queue.CUTOVER = 0;

                TestCall();
                AssertRegistrationDestinationFilePath(T1);
                AssertRegistrationDestinationFilePath(T2);
                AssertRegistrationFilePath(T3);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestCutOver()
        {
            const string T = "TEST.csv";
            Prep();
            TestFileCreation(T);
            FileInfo ti = new FileInfo(GetRegistrationFilePath(T));            
            Queue.CUTOVER = -5;
            TestCall();

            AssertRegistrationFilePath(T); //Should not have moved.
            ti.LastWriteTime = DateTime.Now.AddMinutes(-10); //Move last write time backwards
            TestCall(); 
            AssertRegistrationDestinationFilePath(T); //Should now have passed cutover condition.

        }

        #endregion
    }
}
