using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem.Scripting;
using SEIDR.JobBase;

namespace JobUnitTest
{
    [TestClass]
    public class BatchScriptTest : JobTestBase<BatchScriptJob>
    {
        string DEFAULT_FILEPATH { get
            {
                return System.IO.Path.Combine(JobFolder, "Output.txt");
            }
        }
        //const string DEFAULT_FILEPATH = @"C:\SEIDR\Test\Output.txt";        
        System.IO.FileInfo dummy = new System.IO.FileInfo(@"..\..\FileResource folder\DummyFile.txt");
        bool success;
        protected override void Init()
        {
            base.Init();        
            if (System.IO.File.Exists(DEFAULT_FILEPATH))
                System.IO.File.Delete(DEFAULT_FILEPATH);
        }
        ExecutionStatus call(string TestScript, out bool success, string FilePath = null, string Parm3 = null, string Parm4 = null, DateTime? ProcessingDate = null)
        {
            BatchScriptJobConfiguration config = new BatchScriptJobConfiguration
            {
                BatchScriptPath = TestScript,
                Parameter3 = Parm3,
                Parameter4 = Parm4
            };
            JobExecution je = JobExecution.GetSample(1234, 1, 1, 1, 1, FilePath: FilePath, ProcessingDate: ProcessingDate);
            config.SetupArgs(je);
            ExecutionStatus result = new ExecutionStatus();
            try
            {
                success = _JOB.Call(config, _Executor, ref result);
            }
            catch(Exception ex)
            {
                _Executor.LogError("Process Error", ex, null);
                result.ExecutionStatusCode = "F";
                result.NameSpace = "SEIDR";
                success = false;
            }
            return result;
        }
        [TestMethod]
        public void TestScriptCall()
        {                                   
            call(@"..\..\FileResource folder\BatchScripting1.bat", out success,
                   DEFAULT_FILEPATH, //Parm1
                    dummy.FullName,
                  "values! <YYYY> <MM> <DD>");
            Assert.IsTrue(success);
        }
        [TestMethod]
        public void TestScriptDatedOutputCall()
        {
            call(@"..\..\FileResource folder\BatchScripting4.bat", out success,
                    Parm3: dummy.FullName, Parm4: "<YYYY><MM>testOutput<DD>.txt", ProcessingDate: new DateTime(2018, 12, 14));
            Assert.IsTrue(success);
            var fi = new System.IO.FileInfo(@"..\..\FileResource folder\201812testOutput14.txt");
            Assert.IsTrue(fi.Exists);//Relative path from script check.
        }
    }
}
