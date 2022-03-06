using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.DemoMap.HEALTHQUEST;
using SEIDR.JobBase;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class UABTest : ContextJobTestBase<UAB_DMAP, MappingContext>
    {
        [TestMethod]
        public void RunTest()
        {
            const string INPUT_FILE = "full input path";
            const string LOCAL_OUTPUT_FOLDER = "Output Folder full name";
            const int ORGANIZATION_ID = 994;


            //Should prevent trying to actually create the child job executions
            var mm = NewMockModelQualified("SEIDR.usp_JobExecution_i_ss");
            mm.MapToNewRow(new {JobExecutionID = (long?)0 });
            _TestExecution.SetOrganizationID(ORGANIZATION_ID);
            

            DemoMapJobConfiguration config = new DemoMapJobConfiguration
            {
                PayerLookupDatabaseID = 1, //Metrix DatabaseLookupID (Load table columns and PayerMaster lookup)
                FileMapDatabaseID = 1, //DataServices Lookup ID
                FileMapID = 1 //PackageID
            };
            config.SetOutputFolder(LOCAL_OUTPUT_FOLDER);
            //Working file = Local copy of input file.
            _JOB.Process(MyContext, config, INPUT_FILE, true);


        }

    }
}
