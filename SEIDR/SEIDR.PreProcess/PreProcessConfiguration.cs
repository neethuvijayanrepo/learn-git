using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.PreProcess
{
    public class PreProcessConfiguration
    {
        /// <summary>
        /// Procedure for getting Pre process record.
        /// </summary>
        const string GET_EXECUTION_INFO = "SEIDR.usp_LoaderJob_ss";

        public int JobProfile_JobID { get; set; }

        public string ServerInstanceName { get; set; }

        public string AndromedaServer { get; set; }

        public string OutputFolder { get; set; }

        public int PackageID { get; set; }        

        public string Category { get; set; }

        public string Name { get; set; }

        public string ServerName { get; set; }

        public string PackagePath { get; set; }

        public string  DatabaseConnectionManager { get; set; }
        public int? DatabaseConnection_DatabaseLookupID { get; set; }

        /// <summary>
        /// Get Pre process configuration using jobprofile_jobid.
        /// </summary>
        /// <returns></returns>
        public static DataRow GetPreProcessConfiguration(DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper["JobProfile_JobID"] = JobProfile_JobID;
                return dm.Execute(helper).GetFirstRowOrNull();
            }
        }


    }
}
