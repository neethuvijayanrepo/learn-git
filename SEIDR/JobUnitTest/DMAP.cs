using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest
{
    [TestClass]
    public class MLKDMAP : JobTestBase<MLK_DEMO>
    {
        [TestMethod]
        public void TestMLKFile()
        {
            _TestExecution.SetJobProfileID(112);          
            _TestExecution.SetStepNumber(1);
            _TestExecution.SetJobProfile_JobID(168);
            _TestExecution.FilePath = @"C:\temp\DMAP\CYM_MLK_DEMO_20190702.CYM";
            ExecuteTest();
        }
    }
}
