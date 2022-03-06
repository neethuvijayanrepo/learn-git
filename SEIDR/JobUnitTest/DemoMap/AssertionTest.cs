using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase.HelperClasses;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class AssertionTest : ContextJobTestBase<DMAPOutputAssertionJob, MappingContext>
    {
        [TestMethod]
        public void Test()
        {
            MyContext.Execution.FilePath = @"\\ncimtxfls01\andromeda_UAT\Wellstar\DailyLoads\_Preprocessing\Demo\Csharp_testing\assert\output\CYMWELL_newdailydemo_20200704_DEMO.CYM";
            MyContext.Execution.SetBranch("A"); //Note: DEBUG only method.
            FileAssertionTestConfiguration config = new FileAssertionTestConfiguration(@"\\ncimtxfls01\andromeda_UAT\WellStar\DailyLoads\_Preprocessing\Demo\51\Output\A\CYMWELL_newdailydemo_20200704_A.CYM", false, false);
            config.SkipColumns = "PatientLanguageCode;GuarantorLanguageCode;GuarantorCountryCode;PatientCountryCode";


            int lc = -1;
            bool finishedActual = false;
            try
            {
                _JOB.DoTest(config, MyContext, ref lc, out finishedActual);
            }
            catch (Exception ex)
            {
                string errFile = finishedActual ? "Expected" : "Actual";
                MyContext.LogError("Uncaught exception at " + errFile + " file line # " + lc, ex);
                Assert.Fail();
            }

        }
    }
}
