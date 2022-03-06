using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem.PGP;
using System;
using System.IO;
using System.Data;
using System.Threading;

namespace JobUnitTest
{
    public static class IntegrationTestsSynchronization
    {
        public static readonly object LockObject = new object();
    }

    [TestClass]
    public class PGPTest
    {
        public PGPTest()
        {
            PGPConfiguration config = new PGPConfiguration();
            PGP pgp = new PGP(config);
        }

        static void  CleanUpDir(string Dir)
        {
            DirectoryInfo di = new DirectoryInfo(Dir);
            FileInfo[] fi = di.GetFiles();
            foreach (FileInfo f in fi)
                f.Delete();
        }

        static bool CompareTwoFiles(string FullFileName1, string FullFileName2)
        {
            string File1Content;
            using (StreamReader reader = new StreamReader(FullFileName1))
                File1Content = reader.ReadToEnd();

            string File2Content;
            using (StreamReader reader = new StreamReader(FullFileName2))
                File2Content = reader.ReadToEnd();

            var res = String.Equals(File1Content, File2Content, StringComparison.OrdinalIgnoreCase);
            return res;
        }

        private static readonly string InitTestRootResorcesPath = @"D:\Navigant\SEIDR_ROOT\SEIDR_VS2017_9\";

        // Correct keys used for all tests except GenerateKeys_OneOperation_ ..... tests generated with this PassPhrase and KeyIdentity
        private static readonly string PublicKeyFileFullFileName =  InitTestRootResorcesPath +  @"JobUnitTest\FileResource folder\public.key";
        private static readonly string PrivateKeyFileFullFileName = InitTestRootResorcesPath +  @"\JobUnitTest\FileResource folder\private.key";

        private static readonly string PassPhrase = "S@turd@y";
        private static readonly string KeyIdentity = "Cymetr1x";

        private static readonly string InitExistingTestPathRoot = @"D:\PGP_Test\";

        private static readonly string PGP_TestFileDAILYTrans_FullFileName = InitTestRootResorcesPath + @"JobUnitTest\FileResource folder\PGP_TestFileDAILYTRANS20150220.txt";
        private static readonly string PGP_TestFile1_FullFileName = InitTestRootResorcesPath + @"JobUnitTest\FileResource folder\PGP_TestFile1.txt";

        private static readonly string NonExistingPath = @"C:\NonExistingPath\";


        [TestInitialize()]
        public void Init()
        {
            Monitor.Enter(IntegrationTestsSynchronization.LockObject);

            //if (Directory.Exists(InitExistingTestPathRoot)) {
            //    Directory.Delete(InitExistingTestPathRoot, true);}

            Directory.CreateDirectory(InitExistingTestPathRoot);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(InitExistingTestPathRoot)){
                Directory.Delete(InitExistingTestPathRoot, true);}

            Monitor.Exit(IntegrationTestsSynchronization.LockObject);
        }

        #region  GenerateKeys_OneOperation_ test
        [TestMethod]
        public void GenerateKeys_OneOperation_Can_Run_Without_Exceptions_Test()
        {
            var KeyFilePath = InitExistingTestPathRoot;

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration {PGPOperationID = (int)PGPOperation.GenerateKey, KeyIdentity = "Cymetr1x", PassPhrase = "S@turd@y"};

            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";
           
            PGP pgpInstance = new PGP(config);
            bool result = pgpInstance.Process(ref error);
            Assert.IsTrue(result);
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.PublicKeyFile));
            Assert.IsTrue(File.Exists(config.PrivateKeyFile));
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_KeyStoreUrl_Is_Empty_For_Public_Test()
        {
            var KeyFilePath = InitExistingTestPathRoot;

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration {PGPOperationID = (int)PGPOperation.GenerateKey, KeyIdentity = "Cymetr1x", PassPhrase = "S@turd@y"};

            config.PublicKeyFile = "";
            config.PrivateKeyFile = KeyFilePath + "private.key";        

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PU);
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_KeyStoreUrl_Is_Empty_For_Private_Test()
        {
            var KeyFilePath = InitExistingTestPathRoot;

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration {PGPOperationID = (int)PGPOperation.GenerateKey, KeyIdentity = "Cymetr1x", PassPhrase = "S@turd@y" };

            config.PublicKeyFile = KeyFilePath + "public.key"; 
            config.PrivateKeyFile = "";

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PI);
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_KeyIdentity_Is_DBNullOrEmpty_Test()
        {
            var KeyFilePath = InitExistingTestPathRoot;

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration { PGPOperationID = (int)PGPOperation.GenerateKey, PassPhrase = "S@turd@y" };

            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";
            config.KeyIdentity = "";

            PGP pgpInstance = new PGP(config);
            bool result = pgpInstance.Process(ref error);
            Assert.IsTrue(result);
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.PublicKeyFile));
            Assert.IsTrue(File.Exists(config.PrivateKeyFile));           
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_KeyIdentity_IsNull_Test()
        {
            ValidationError error = ValidationError.None;
            var KeyFilePath = InitExistingTestPathRoot;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.GenerateKey,
                PublicKeyFile = KeyFilePath + "public.key",
                PrivateKeyFile = KeyFilePath + "private.key",
                PassPhrase = "S@turd@y",

                KeyIdentity = DBNull.Value.ToString()
            };

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_PassPhrase_IsDBNullOrEmpty_Test()
        {
            ValidationError error = ValidationError.None;
            var KeyFilePath = InitExistingTestPathRoot;

            PGPConfiguration config = new PGPConfiguration();
            config.PGPOperationID = (int)PGPOperation.GenerateKey;
            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";
            config.KeyIdentity = "Cymetr1x";
            config.PassPhrase = DBNull.Value.ToString();

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);
        }

        [TestMethod]
        public void GenerateKeys_OneOperation_PassPhrase_IsNull_Test()
        {
            ValidationError error = ValidationError.None;
            var KeyFilePath = InitExistingTestPathRoot;

            PGPConfiguration config = new PGPConfiguration();
            config.PGPOperationID = (int)PGPOperation.GenerateKey;
            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";
            config.KeyIdentity = "Cymetr1x";
            config.PassPhrase = null;

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);
        }
        #endregion

        #region EncryptOneOperation tests 
        //  -------------------------------------------
        [TestMethod]
        public void Encrypt_OneOperation_Test_Can_Run_Without_Exeception_Encrypt_One_File()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);

            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            bool result = pgpInstance.Process(ref error);
            Assert.IsTrue(result);
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var resFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) +".pgp";

            Assert.IsTrue(File.Exists(resFileName));
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Can_Run_Without_Exeception_Encrypt_All_Files_In_The_Folder()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            File.Copy(PGP_TestFileDAILYTrans_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFileDAILYTrans_FullFileName), true);

            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            bool result = pgpInstance.Process(ref error);
            Assert.IsTrue(result);
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp"));
            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFileDAILYTrans_FullFileName) + ".pgp"));
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Public_Key_Is_Empty()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = DBNull.Value.ToString(),
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PU);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Public_Key_NoExist()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = NonExistingPath + "public.key",
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PU);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_SourcePath_Is_Empty()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            config.SourcePath = DBNull.Value.ToString();

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PS);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_SourcePath_NotExists()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            config.SourcePath = NonExistingPath;

            Assert.IsTrue(!Directory.Exists(config.SourcePath));

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PS);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_OutPutPath_Is_Empty()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            config.OutputPath = DBNull.Value.ToString();

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PO);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_OutputPath_NotExists()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            config.OutputPath = NonExistingPath;

            Assert.IsTrue(!Directory.Exists(config.OutputPath));

            PGP pgpInstance = new PGP(config);
            Assert.IsFalse(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.PO);
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Source_Is_Existing_Full_FileName_Txt()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            config.SourcePath = config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName);

            Assert.IsTrue(File.Exists(config.SourcePath));
            Assert.AreEqual(Path.GetExtension(config.SourcePath), ".txt", true);

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp"));
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Source_Has_Files_WithDifferent_Ext_Txt_And_Not_Txt()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            File.Copy(PGP_TestFileDAILYTrans_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFileDAILYTrans_FullFileName) 
                + ".sql", true);

            Assert.IsTrue(Directory.Exists(config.SourcePath));

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp"));
        }

        [TestMethod]
        public void Encrypt_OneOperation_Test_Source_With_Not_Txt_Ext_Mask_Has_Files_WithDifferent_Ext_Txt_And_Not_Txt()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);

            var FullFileNameToEncrypt = config.SourcePath + Path.GetFileNameWithoutExtension(PGP_TestFileDAILYTrans_FullFileName) + ".sql";
            File.Copy(PGP_TestFileDAILYTrans_FullFileName, FullFileNameToEncrypt, true);

            Assert.IsTrue(File.Exists(FullFileNameToEncrypt));
 
            config.SourcePath = config.SourcePath + "*.sql";

            Assert.AreEqual(Path.GetExtension(config.SourcePath), ".sql", true);

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(FullFileNameToEncrypt) + ".pgp"));
            Assert.IsTrue(SEIDR.FileSystem.Utility.GetFiles(config.OutputPath, "*.txt").GetLength(0) == 0);
        }

        #endregion

        #region DecryptOneOperation tests 
        [TestMethod]
        public void Test()
        {
            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Decrypt,
                SourcePath = "C:\\Source\\*_08112018.txt.pgp",
                OutputPath = "D:\\Output",
                PassPhrase = PassPhrase
            };

            string s = PGP.GetOutputFile(config);

            config.PGPOperationID = (int)PGPOperation.Encrypt;
            config.SourcePath = "C:\\Source\\*.08112018.txt";
            config.OutputPath = "D://Output//";

            s = PGP.GetOutputFile(config);

            config.PGPOperationID = (int)PGPOperation.Sign;
            config.SourcePath = "C:\\Source\\*.08112018.txt";
            config.OutputPath = "D://Output//";

            s = PGP.GetOutputFile(config);

            config.PGPOperationID = (int)PGPOperation.SignAndEncrypt;
            config.SourcePath = "C:\\Source\\*.08112018.txt";
            config.OutputPath = "D://Output//";

            s = PGP.GetOutputFile(config);

            config.PGPOperationID = (int)PGPOperation.GenerateKey;
            config.SourcePath = "C:\\Source\\*.08112018.txt";
            config.OutputPath = "D://Output//";
            config.PrivateKeyFile = "c:\\output\\private.key";
            config.PublicKeyFile = "c:\\output\\public.key";

            s = PGP.GetOutputFile(config);
        }

        [TestMethod]
        public void Decrypt_OneOperation_Test_Can_Run_Without_Exeception_Decrypt_One_File()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Decrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PrivateKeyFile = PrivateKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName + ".pgp", config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp", true);                    

            PGP pgpInstance = new PGP(config);
            bool result = pgpInstance.Process(ref error);
            Assert.IsTrue(result);
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var resFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName);
            Assert.IsTrue(File.Exists(resFileName));
            Assert.IsTrue(Path.GetExtension(resFileName) == ".txt");
        }        
        #endregion

        #region SignAndEncryptOneOperation tests 
        [TestMethod]
        public void SignAndEncrypt_OneOperation_Test_Can_Run_Without_Exeception_SignAndEncrypt_One_File_And_Can_Verify_Signed()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.SignAndEncrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PrivateKeyFile = PrivateKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var resFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp";
            Assert.IsTrue(File.Exists(resFileName));

            Assert.IsTrue(PGP.VerifySignFile(resFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));        
        }

        [TestMethod]
        public void SignAndEncrypt_OneOperation_Test_Can_Verify_File_Was_Signed_By_Unproper_Key()
        {
            var KeyFilePath = InitExistingTestPathRoot + @"OtherKeys\";
            Directory.CreateDirectory(KeyFilePath);

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.GenerateKey,
                KeyIdentity = "Other_Cymetr1x",
                PassPhrase = "Other_S@turd@y"
            };

            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";

            PGP pgpInstance = new PGP(config);

            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            // Step#1 finished - other keys generated;

            Assert.IsTrue(File.Exists(config.PublicKeyFile));
            Assert.IsTrue(File.Exists(config.PrivateKeyFile));

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);
            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            config.SourcePath = config.SourcePath + "*.txt";

            config.PGPOperationID = (int)PGPOperation.SignAndEncrypt;
            pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));

            var OtherKeySignedAndEncryptedFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp";
            Assert.IsTrue(File.Exists(OtherKeySignedAndEncryptedFileName));

            // Step#2 finished - file signed and encrypted by other keys;

            Assert.IsTrue(PGP.VerifySignFile(OtherKeySignedAndEncryptedFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));

            config.PublicKeyFile = PublicKeyFileFullFileName;
            config.PrivateKeyFile = PrivateKeyFileFullFileName;
            config.KeyIdentity = "Cymetr1x";
            config.PassPhrase = "S@turd@y";

            Assert.IsFalse(PGP.VerifySignFile(OtherKeySignedAndEncryptedFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));
        }

        #endregion

        #region SignOneOperation tests 

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void SignAndEncrypt_OneOperation_Test_Cannot_Verify_File_Without_Signature()
        {
            PGPConfiguration config = new PGPConfiguration
            {
                PublicKeyFile = PublicKeyFileFullFileName,
                PrivateKeyFile = PrivateKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            PGP.VerifySignFile(PGP_TestFile1_FullFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase);
        }

        [TestMethod]
        public void Sign_OneOperation_Test_Can_Run_Without_Exeception_Sign_One_File_And_Can_Verify_Signed()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Sign,
                PublicKeyFile = PublicKeyFileFullFileName,
                PrivateKeyFile = PrivateKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var resFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".sgn";
            Assert.IsTrue(File.Exists(resFileName));

            Assert.IsTrue(PGP.VerifySignFile(resFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));
        }      

        [TestMethod]
        public void Sign_OneOperation_Test_Can_Verify_File_Was_Signed_By_Unproper_Key()
        {
            var KeyFilePath = InitExistingTestPathRoot + @"OtherKeys\";
            Directory.CreateDirectory(KeyFilePath);

            ValidationError error = ValidationError.None;
            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.GenerateKey,
                KeyIdentity = "Other_Cymetr1x",
                PassPhrase = "Other_S@turd@y"
            };

            config.PublicKeyFile = KeyFilePath + "public.key";
            config.PrivateKeyFile = KeyFilePath + "private.key";

            PGP pgpInstance = new PGP(config);

            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            // Step#1 finished - other keys generated;

            Assert.IsTrue(File.Exists(config.PublicKeyFile));
            Assert.IsTrue(File.Exists(config.PrivateKeyFile));

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);
            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);
            config.SourcePath = config.SourcePath + "*.txt";

            config.PGPOperationID = (int)PGPOperation.Sign;
            pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));

            //var OtherKeySignedAndEncryptedFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp";
            var OtherKeySignedAndEncryptedFileName = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".sgn";
            Assert.IsTrue(File.Exists(OtherKeySignedAndEncryptedFileName));

            Assert.IsTrue(PGP.VerifySignFile(OtherKeySignedAndEncryptedFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));

            // Step#2 finished - file successfully signed by other keys;

            config.PublicKeyFile = PublicKeyFileFullFileName;
            config.PrivateKeyFile = PrivateKeyFileFullFileName;
            config.KeyIdentity = "Cymetr1x";
            config.PassPhrase = "S@turd@y";

            Assert.IsFalse(PGP.VerifySignFile(OtherKeySignedAndEncryptedFileName, config.PublicKeyFile, config.PrivateKeyFile, config.PassPhrase));
        }
        #endregion

        #region TwoOperations tests 
        [TestMethod]
        public void TwoOperations_Encrypt_Decrypt_Test_Can_Run_Without_Exeception_Encrypt_And_Decrypt_One_File_Get_Initial_File()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.Encrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);
           

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);

            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);
            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp"));

            config.PGPOperationID = (int)PGPOperation.Decrypt;

            config.SourcePath = InitExistingTestPathRoot + @"Output\";
            config.OutputPath = InitExistingTestPathRoot + @"Source\";
            config.PrivateKeyFile = PrivateKeyFileFullFileName;

            pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var FullFileNameDecrypted = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName);
            Assert.IsTrue(CompareTwoFiles(PGP_TestFile1_FullFileName, FullFileNameDecrypted));
        }

        [TestMethod]
        public void TwoOperations_SignAndEncrypt_Decrypt_Test_Can_Run_Without_Exeception_Encrypt_One_File_Get_Initial_File()
        {
            ValidationError error = ValidationError.None;

            PGPConfiguration config = new PGPConfiguration
            {
                PGPOperationID = (int)PGPOperation.SignAndEncrypt,
                PublicKeyFile = PublicKeyFileFullFileName,
                PrivateKeyFile = PrivateKeyFileFullFileName,
                PassPhrase = PassPhrase
            };

            config.SourcePath = InitExistingTestPathRoot + @"Source\";
            config.OutputPath = InitExistingTestPathRoot + @"Output\";
            Directory.CreateDirectory(config.SourcePath); Directory.CreateDirectory(config.OutputPath);

            File.Copy(PGP_TestFile1_FullFileName, config.SourcePath + Path.GetFileName(PGP_TestFile1_FullFileName), true);

            config.SourcePath = config.SourcePath + "*.txt";

            PGP pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);
            Assert.IsTrue(File.Exists(config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName) + ".pgp"));

            config.PGPOperationID = (int)PGPOperation.Decrypt;

            config.SourcePath = InitExistingTestPathRoot + @"Output\";
            config.OutputPath = InitExistingTestPathRoot + @"Source\";
            config.PrivateKeyFile = PrivateKeyFileFullFileName;

            pgpInstance = new PGP(config);
            Assert.IsTrue(pgpInstance.Process(ref error));
            Assert.AreEqual<ValidationError>(error, ValidationError.None);

            var FullFileNameDecrypted = config.OutputPath + Path.GetFileName(PGP_TestFile1_FullFileName);
            Assert.IsTrue(CompareTwoFiles(PGP_TestFile1_FullFileName, FullFileNameDecrypted));
        }

        #endregion        
    }
}
