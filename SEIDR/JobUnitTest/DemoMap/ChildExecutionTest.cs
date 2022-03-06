using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class ChildExecutionTest: TestBase
    {
        [TestMethod]
        public void TestChildExecutionCreation()
        {
            var jp = CreateProfile("ChildExecutionUnitTest", "CHILD_TEST");
            var je = RegisterBasicExecution(jp, null);

            ChildExecutionInfo ce = new ChildExecutionInfo(je, true)
            {
                Branch = "TEST",
                FilePath = @"C:\Test\Test.txt"
            };
            var ctxt = new MappingContext();
            ctxt.Init(_Executor, je);
            var je2 = ContextJobBase<MappingContext>.RegisterChildExecution(ce, ctxt);
            Assert.IsNotNull(je2);
            //Note: If steps are configured, then we need to also have branch information in the JobProfile_Job or it may map to a different branch.
            Assert.AreEqual(ce.Branch, je2.Branch); 
            Assert.AreEqual(ce.StepNumber, je2.StepNumber);
            Assert.AreEqual(je.StepNumber + 1, ce.StepNumber);
            Assert.AreEqual(ce.FilePath, je2.FilePath);
            Assert.AreEqual(ce.InitializationStatusCode, je2.ExecutionStatusCode);

        }

    }
}
