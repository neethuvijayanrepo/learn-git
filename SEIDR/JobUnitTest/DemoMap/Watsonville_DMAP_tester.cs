using SEIDR.DemoMap.BaseImplementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap.CLIENT_DMAP;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class Watsonville_DMAP_test : ContextJobTestBase<Watsonville_DMAP, MappingContext>
    {
        [TestMethod]
        public void RunTest()
        {
            // const string INPUT_FILE = @"C:\DMAP_Testing\input\NAV_Demographics_20200303.csv";
            const string INPUT_FILE = @"C:\DMAP_Testing\input\dmap.txt.cln";
            const string LOCAL_OUTPUT_FOLDER = @"c:\dmap_testing\output";
            const int ORGANIZATION_ID = -20;    

            //Should prevent trying to actually create the child job executions
            var mm = NewMockModelQualified("SEIDR.usp_JobExecution_i_ss");
            mm.MapToNewRow(new { JobExecutionID = (long?)0 });

            _TestExecution.SetOrganizationID(ORGANIZATION_ID);

            DemoMapJobConfiguration config = new DemoMapJobConfiguration
            {
                PayerLookupDatabaseID = 1, //Metrix DatabaseLookupID (Load table columns and PayerMaster lookup)
                FileMapDatabaseID = 2, //DataServices Lookup ID
                FileMapID = 1107 //PackageID
            };
            config.SetOutputFolder(LOCAL_OUTPUT_FOLDER);
            config.SetDoAPB(true);
            //Working file = Local copy of input file.
            //Can put a breakpoint here to step through from the beginning
            //or put a breakpoint in the transform method to step through. 
            //Filtered breakpoints useful for checking specific accounts or conditions
            _JOB.Process(MyContext, config, INPUT_FILE, true);
        }
    }
}
