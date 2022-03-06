using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    public class JobProfile_Job_SettingsFile: DataBase.DatabaseObject<JobProfile_Job_SettingsFile>
    {
        public int JobProfile_JobID { get; set; }
        public string SettingsFilePath { get; set; }

        public static JobProfile_Job_SettingsFile GetRecord(DataBase.DatabaseManager m, int JobProfile_JobID)
        {
            using (var h = m.GetBasicHelper())
            {
                h.QualifiedProcedure = "SEIDR.usp_JobProfile_Job_SettingsFile_ss";
                h["JobProfile_JobID"] = JobProfile_JobID;
                return m.SelectSingle<JobProfile_Job_SettingsFile>(h);
            }
        }
    }
}
