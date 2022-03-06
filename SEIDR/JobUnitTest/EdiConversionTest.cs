using System;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.FileSystem.EDI;

namespace JobUnitTest
{
    [TestClass]
    public class EdiConversionTest
    {
        TestExecutor ex;
        EdiConversionJob job;
        EdiConversion shellSetting;
        SEIDR.JobBase.ExecutionStatus status = new SEIDR.JobBase.ExecutionStatus { NameSpace = "FILESYSTEM" };
        public EdiConversionTest()
        {
            DatabaseConnection db = new DatabaseConnection(@"SDSRV015.cymetrix.com\sql2014", "MIMIR");
            ex = new TestExecutor(db);

            job = new EdiConversionJob();
            shellSetting = new EdiConversion
            {
                KeepOriginal = true,
            };
        }
        [TestMethod]
        public void BulkTest()
        {
            shellSetting.OutputFolder = @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_Cleaned\<YYYY>_<MM>_<DD>\"; //ProcessingDate will default to today for testing purposes.
            
            var dir = new System.IO.DirectoryInfo(@"\\sdsrv031.cymetrix.com\FTP\vanderbilt");
            var fileList = dir.GetFiles("*.835");
            foreach(var f in fileList)
            {
                Call(f.FullName);
            }

        }
        [TestMethod]
        public void TestNoCLP()
        {
            shellSetting.OutputFolder = @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_Cleaned\<YYYY>_<MM>_<DD>\"; //ProcessingDate will default to today for testing purposes.

            Call(@"\\SDSRV015.CYMETRIX.COM\ANDROMEDAFILESSANDBOX\VANDERBILT\DAILY_LOADS\_835\41455\INPUT\_Processed_\PB_WELLCARE_OF_KENTUCKY_INC__SPLIT_20190208T064534_190208nr.835");
            Assert.AreEqual("NC", status.ExecutionStatusCode);
        }

        /*
        @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180222T063025_COMBINED.835"
        @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180305T070022_COMBINED.835"
        @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180308T070020_COMBINED.835"
        @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180322T113022_COMBINED.835"
        @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180412T064520_COMBINED.835"
         */

        public void Call(string FilePath)
        {
            var ex = new SEIDR.JobBase.JobExecution { FilePath = FilePath };
            job.ProcessSettings(ex, shellSetting, ref status);
        }

        [TestMethod]
        public void NoFolderTest()
        {
            Call(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180222T063025_COMBINED.835");
        }
        [TestMethod]
        public void FolderTest()
        {
            shellSetting.OutputFolder = @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\DestinationTest";
            Call(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180322T113022_COMBINED.835");
        }

        [TestMethod]
        public void NoFolderTestSmallBlock()            
        {
            shellSetting.BlockSize = 50; //Should go to init size
            Call(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180222T063025_COMBINED.835");
        }
        [TestMethod]
        public void FolderTestSmallBlock()
        {
            shellSetting.BlockSize = 50; //Should go to init size
            shellSetting.OutputFolder = @"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\DestinationTest";
            Call(@"\\sdsrv031.cymetrix.com\is\DATA\_SourceFiles\Vanderbilt\835_TEST\HB_ALABAMA_20180322T113022_COMBINED.835");
        }


    }
}
