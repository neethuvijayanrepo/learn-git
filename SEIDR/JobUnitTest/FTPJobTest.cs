using System;
using System.Text;
using System.Collections.Generic;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.FileSystem.FTP;

namespace JobUnitTest
{
    /// <summary>
    /// Summary description for FTPJobTest
    /// </summary>
    [TestClass]
    public class FTPJobTest : JobTestBase<FTPJob>
    {

        DatabaseConnection c;
        DatabaseManager mgr;
        ExecutionStatus Status = new ExecutionStatus();
        TestExecutor test => _Executor;
        FTPJob ftp = new FTPJob();
        public FTPJobTest()
        {
            c = new DatabaseConnection(@"NCIHCTSTSQL07\sql2014", "MIMIR");
            mgr = new DatabaseManager(c);
        }
        


        /*
         * ToDo: Store an FTP Account configuration to a file - create a method to check that the FTP Account exists (if in DB mode, or to just use the data directly for FTP configuration otherwise)
         * Move this class to implement JobTestBase instead of extending JobExecution.
         * Add file prep, and a way of checking that file is actually sent (besides the WinSCP result, even if that should be correct)
         * Also: More test methods for the other FTP operations
         
         
             */
        
        [TestMethod]
        public void FTPSend()
        {
            //ToDo: Need to redo.
        }
    }
}
