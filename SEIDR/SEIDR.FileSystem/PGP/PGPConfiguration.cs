using System;

using SEIDR.DataBase;

namespace SEIDR.FileSystem.PGP
{
    public class PGPConfiguration
    {
        const string GET_EXECUTION_INFO = "SEIDR.usp_PGPJobConfiguration_ss";

        public int PGPJobID { get; set; }
        public int JobProfile_JobID { get; set; }
        public byte PGPOperationID { get; set; }
        public string SourcePath { get; set; }
        public string OutputPath { get; set; }
        public string PublicKeyFile { get; set; }
        public string PrivateKeyFile { get; set; }
        public string KeyIdentity { get; set; }
        public string PassPhrase { get; set; }
        public string Description { get; set; }
        public string PGPOperationName { get; set; }

        static public PGPConfiguration GetConfiguration(DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(JobProfile_JobID)] = JobProfile_JobID;
                return dm.SelectSingle<PGPConfiguration>(helper, true, false);
            }
        }

    }
}
