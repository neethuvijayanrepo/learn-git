using System;
using System.Collections.Generic;
using SEIDR.DataBase;

namespace SEIDR.FileSystem.SpawnJob
{
    public class SpawnConfiguration
    {
        const string GET_EXECUTION_INFO = "SEIDR.usp_SpawnJobConfiguration_sl";        

        public int SpawnJobID { get; set; }
        public int JobProfile_JobID { get; set; }
        public int JobProfileID { get; set; }
        public string SourceFile { get; set; }
        public int FileCounter = 0;

        static public List<SpawnConfiguration> GetConfiguration(DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(JobProfile_JobID)] = JobProfile_JobID;
                
                return dm.SelectList<SpawnConfiguration>(helper);
            }
        }
    }
}
