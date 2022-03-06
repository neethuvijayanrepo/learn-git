using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;

namespace JobUnitTest
{
    public partial class FileSystemJobTests
    {

        public readonly string DUMMY_FOLDER_PATH = $@"C:\SEIDR\{DateTime.Today:yyyyMM_dd}\Test\";
        [TestMethod]
        public void CreateDirTest()
        {
            FS Dir = new FS
            {
                Operation = FileOperation.CREATEDIR,
                OutputPath = DUMMY_FOLDER_TEST
            };
            
            Assert.IsTrue(Test(Dir));
            Assert.IsTrue(Directory.Exists(DUMMY_FOLDER_PATH));
        }
        [TestMethod]
        public void MoveDirTest()
        {
            string expected = $@"C:\SEIDR\{DateTime.Today:yyyyMM_dd}\Dummy{DateTime.Today:yyyyMd}.txt";
            CreateDirTest();
            Dummy.OutputPath = DUMMY_FOLDER_TEST + "Dummy<YYYY><M><D>.txt";
            RunDummy();
            FS MoveDir = new FS
            {
                Operation = FileOperation.MOVEDIR,
                Source = DUMMY_FOLDER_TEST,
                OutputPath = DUMMY_DESTINATION,
                Overwrite = true
            };
            Assert.IsTrue(Test(MoveDir));
            Assert.IsTrue(File.Exists(expected));

        }
        [TestMethod, ExpectedException(typeof(IOException))]
        public void MoveDirTest_NoOverwrite()
        {
            MoveDirTest();
            string expected = $@"C:\SEIDR\{DateTime.Today:yyyyMM_dd}\Dummy{DateTime.Today:yyyyMd}.txt";
            CreateDirTest();
            Dummy.OutputPath = DUMMY_FOLDER_TEST + "Dummy<YYYY><M><D>.txt";
            RunDummy();
            FS MoveDir = new FS
            {
                Operation = FileOperation.MOVEDIR,
                Source = DUMMY_FOLDER_TEST,
                OutputPath = DUMMY_DESTINATION,
                Overwrite = false
            };
            Test(MoveDir);
            Assert.Fail();
        }
    }
}
