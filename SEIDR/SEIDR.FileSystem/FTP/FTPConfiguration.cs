using SEIDR.DataBase;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SEIDR.FileSystem.FTP
{
    public class FTPConfiguration
    {
        const string GET_EXECUTION_INFO = "SEIDR.usp_FTPJobConfiguration_ss";
        
        public int FTPJobID { get; set; }
        public int JobProfile_JobID { get; set; }
        public int FTPAccountID { get; set; }
        public byte FTPOperationID { get; set; }
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public string RemoteTargetPath { get; set; }
        public bool Active { get; set; }
        public bool Overwrite { get; set; }
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public bool Passive { get; set; }
        public string Fingerprint { get; set; }
        public int FTPProtocolID { get; set; }
        public string Protocol { get; set; }
        /*
         * See DatabaseExtensions.Map - an enum in c# will attempt to parse using Enum.Parse:
         *
                        Type underType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                        if (underType.IsEnum)
                        {
                            nValue = Enum.Parse(underType, nValue.ToString(), true);
                        }

            Simple Test:
            Enum.Parse(typeof(FTPOperation), ((byte)1).ToString(), true);
            Returns RECEIVE

            
            Enum.Parse(typeof(FTPOperation), (nameof(FTPOperation.MOVE_REMOTE)).ToString(), true);
            Returns MOVE_REMOTE
         */
        public FTPOperation Operation { get; set; }
        public string OperationName { get; set; }
        public string PpkFileName { get; set; }
        public bool Delete { get; set; }
        public bool DateFlag { get; set; }
        public DateTime ProcessingDate { get; set; }
        public bool TransferResumeSupport { get; set; }

        /// <summary>
        /// Get FTP configuration based on the JobProfile_JobID
        /// </summary>
        /// <param name="dm">DatabaseManager</param>
        /// <param name="JobProfile_JobID">JobProfile_JobID</param>
        /// <returns></returns>
        public static FTPConfiguration GetFTPConfiguration(DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(JobProfile_JobID)] = JobProfile_JobID;
                return dm.SelectSingle<FTPConfiguration>(helper);
            }
        }
        
    }
}
