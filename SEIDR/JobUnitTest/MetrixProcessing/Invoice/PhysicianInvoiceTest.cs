using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobBase;
using SEIDR.MetrixProcessing.Invoice;
using SEIDR.MetrixProcessing.Invoice.Physician;

namespace JobUnitTest.MetrixProcessing.Invoice
{
    [TestClass]
    public class PhysicianInvoiceTest : ContextJobTestBase<PhysicianInvoiceJob,InvoicingContext>
    {
        [TestMethod]
        public void TestGeneration()
        {
            _JOB.Process(MyContext);
            Assert.IsTrue(MyContext.Success);
        }
        protected override void Init()
        {
            base.Init();
            const int TEST_PROJECT_ID = 201; //Test for dev/QA
            //_TestExecution = JobExecution.GetSample(_TestExecution, ProjectID: TEST_PROJECT_ID);
            _TestExecution.SetProjectID(TEST_PROJECT_ID);
            _TestExecution.SetProcessingDateTime(new DateTime(2020, 3, 31));

        }
    }
}
