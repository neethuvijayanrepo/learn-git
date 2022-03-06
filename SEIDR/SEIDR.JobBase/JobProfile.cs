using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    public class JobProfile
    {
        /// <summary>
        /// Gets a basic sample Profile Object for testing purposes.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="RequiredThreadID"></param>
        /// <param name="RegistrationFolder"></param>
        /// <param name="RegistrationDestination"></param>
        /// <param name="FileFilter"></param>
        /// <param name="FileDateMask">Registration purposes</param>
        /// <returns></returns>
        public static JobProfile GetSample(int ID, int? RequiredThreadID = null, string RegistrationFolder = null, string RegistrationDestination = null
            , string FileFilter = null, string FileDateMask = null, int? loadProfileID = null)
        {
            return new JobProfile
            {
                JobProfileID = ID,
                Description = "Sample",
                RequiredThreadID = RequiredThreadID,
                RegistrationFolder = RegistrationFolder,
                RegistrationDestinationFolder = RegistrationDestination,
                FileFilter = FileFilter,
                FileDateMask = FileDateMask,
                Creator = Environment.UserName,
                loadProfileID = loadProfileID
            };
        }
        public int? JobProfileID { get; private set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public DateTime DC { get; private set; } = DateTime.Now;
        public DateTime LU { get; set; } = DateTime.Now;
        public DateTime? DD { get; set; } = null;
        /// <summary>
        /// Specifies the folder location to search for files to create JobExecutions under this JobProfileID
        /// </summary>
        public string RegistrationFolder { get; set; }
        public string RegistrationDestinationFolder { get; set; }
        /// <summary>
        /// Used for parsing a File ProcessingDate based on the fileName.
        /// </summary>
        public string FileDateMask { get; set; }
        /// <summary>
        /// Filters the files using DOS. Ex: *.* or testFile_2018*.txt
        /// </summary>
        public string FileFilter { get; set; }



        /// <summary>
        /// Exclude subset of files picked up by <see cref="FileFilter"/> 
        /// </summary>
        public string FileExclusionFilter { get; set; }

        public int UserKey { get; set; }
        public string UserKey1 { get; set; }
        public string UserKey2 { get; set; }

        /// <summary>
        /// Allows specifying that a JobProfile needs to run a specific thread number. Can be overridden at execution level.
        /// </summary>
        public int? RequiredThreadID { get; private set; }
        /// <summary>
        /// For creating JobExecutions without folder monitoring
        /// </summary>
        public int? ScheduleID { get; set; }
        public int? loadProfileID { get; set; }
        #region Debug setters
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetRequiredThreadID(int? value)
        {
            RequiredThreadID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetJobProfileID(int value)
        {
            JobProfileID = value;
        }
        #endregion
    }
}
