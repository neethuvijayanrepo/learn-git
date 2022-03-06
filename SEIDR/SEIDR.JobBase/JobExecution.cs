using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    public class JobExecution
    {
        /// <summary>
        /// Returns a Sample JobExecution object with the specified configurations
        /// </summary>
        /// <param name="JobExecutionID"></param>
        /// <param name="JobProfileID"></param>
        /// <param name="JobProfile_JobID"></param>
        /// <param name="JobID"></param>
        /// <param name="StepNumber"></param>
        /// <param name="UserKey"></param>
        /// <param name="UserKey1"></param>
        /// <param name="UserKey2"></param>
        /// <param name="ProcessingDate">If not specified (or set to null), will use DateTime.Now</param>
        /// <param name="ThreadID">RequiredThreadID</param>
        /// <param name="ExecutionStatusCode"></param>
        /// <param name="ExecutionStatusNameSpace"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileSize"></param>
        /// <param name="FileHash"></param>
        /// <param name="METRIX_ExportBatchID"></param>
        /// <param name="IsError"></param>
        /// <param name="METRIX_LoadBatchID"></param>
        /// <param name="OrganizationID"></param>
        /// <param name="ProjectID"></param>
        /// <returns></returns>
        public static JobExecution GetSample(long JobExecutionID, int JobProfileID, int JobProfile_JobID, int JobID,
            int StepNumber, int UserKey = 0, 
            string UserKey1 = null, string UserKey2 = null, DateTime? ProcessingDate = null,
            int? ThreadID = null, string ExecutionStatusCode = "SC", string ExecutionStatusNameSpace = nameof(SEIDR),
            string FilePath = null, long? FileSize = null, string FileHash = null, 
			int? METRIX_ExportBatchID = null, bool IsError=false,int? METRIX_LoadBatchID= null,
            int? OrganizationID = null, int? ProjectID = null)
        {
            return new JobExecution
            {
                JobExecutionID = JobExecutionID,
                JobProfileID = JobProfileID,
                JobProfile_JobID = JobProfile_JobID,
                JobID = JobID,
                OrganizationID = OrganizationID ?? 0,
                ProjectID = ProjectID,
                StepNumber = StepNumber,
                UserKey = UserKey,
                UserKey1 = UserKey1,
                UserKey2 = UserKey2,
                ProcessingDateTime = ProcessingDate ?? DateTime.Now,
                ExecutionStatusCode = ExecutionStatusCode,
                ExecutionStatusNameSpace = ExecutionStatusNameSpace,
                FilePath = FilePath,
                FileSize = FileSize,
                FileHash = FileHash,
                RequiredThreadID = ThreadID,
                METRIX_ExportBatchID = METRIX_ExportBatchID,
                IsError = IsError,
                METRIX_LoadBatchID= METRIX_LoadBatchID
            };
        }

        public static JobExecution GetSample(JobExecution init, 
            int? OrganizationID = null,
            int? ProjectID = null,
            int? UserKey = null, 
            string UserKey1 = null, string UserKey2 = null, 
            DateTime? ProcessingDate = null,
            int? ThreadID = null, 
            string ExecutionStatusCode = null, 
            string ExecutionStatusNameSpace = null,
            string FilePath = null, long? FileSize = null, string FileHash = null,
            int? METRIX_ExportBatchID = null, bool? IsError = null, int? METRIX_LoadBatchID = null)
        {
            return GetSample(init.JobExecutionID ?? -1, init.JobProfileID, init.JobProfile_JobID,
                             init.JobID, init.StepNumber,
                             UserKey ?? init.UserKey,
                             UserKey1 ?? init.UserKey1,
                             UserKey2 ?? init.UserKey2,
                             ProcessingDate ?? init.ProcessingDate,
                             ThreadID ?? init.RequiredThreadID,
                             ExecutionStatusCode ?? init.ExecutionStatusCode,
                             ExecutionStatusNameSpace ?? init.ExecutionStatusNameSpace,
                             FilePath ?? init.FilePath,
                             FileSize ?? init.FileSize,
                             FileHash ?? init.FileHash,
                             METRIX_ExportBatchID ?? init.METRIX_ExportBatchID,
                             IsError ?? init.IsError,
                             METRIX_LoadBatchID ?? init.METRIX_LoadBatchID,
                             OrganizationID ?? init.OrganizationID,
                             ProjectID ?? init.ProjectID
                            );
        }

        public void RefreshFileInfo()
        {
            if (FilePath == null)
            {
                FileHash = null;
                FileSize = null;
                return;
            }
            System.IO.FileInfo fi = new System.IO.FileInfo(FilePath);
            if (fi.Exists)
            {
                FileHash = SEIDR.Doc.DocExtensions.GetFileHash(fi.FullName);
                FileSize = fi.Length;
            }
            else
            {
                FileHash = null;
                FileSize = null;
            }
        }
        public void SetFileInfo(System.IO.FileInfo fi)
        {
            if (fi == null)
            {
                FilePath = null;
                FileHash = null;
                FileSize = null;
            }
            else
            {
                FilePath = fi.FullName;
                if (fi.Exists)
                {
                    FileHash = SEIDR.Doc.DocExtensions.GetFileHash(fi.FullName);
                    FileSize = fi.Length;
                }
                else
                {
                    FileHash = null;
                    FileSize = null;
                }
            }
        }

        public void SetFileInfo(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                SetFileInfo(null as System.IO.FileInfo);
            else
                SetFileInfo(new System.IO.FileInfo(filePath));
        }
        public JobExecution() { }

        #region DB Properties
        /// <summary>
        /// Database Primary Key
        /// </summary>
        public long? JobExecutionID { get; protected set; }
        public int JobProfileID { get; protected set; }

        /// <summary>
        /// Indicates whether or not the profile has a requiredThreadID set
        /// </summary>
        public int? RequiredThreadID { get; set; }
        /// <summary>
        /// Bridge JobProfile to an IJob, based on StepNumber and status (If specific status handling is specified). Specifies Job and settings
        /// </summary>
        public int JobProfile_JobID { get; protected set; }
        /// <summary>
        /// Used as part of bridging to JobProfile_JobID.
        /// </summary>
        public int StepNumber { get; protected set; }
        /// <summary>
        /// Description from JobProfile_Job bridge (tied to step number)
        /// </summary>
        public string Step { get; protected set; }
        /// <summary>
        /// Identifies the IJob to use based on what's in the Database
        /// </summary>
        public int JobID { get; protected set; }

        /// <summary>
        /// Usage depends on profile
        /// </summary>
        public int UserKey { get; protected set; }
        /// <summary>
        /// Configuration category
        /// </summary>
        public string UserKey1 { get; protected set; }
        /// <summary>
        /// Secondary configuration category.
        /// </summary>
        public string UserKey2 { get; set; }

        public int OrganizationID { get; protected set; } = 0;
        public string Organization { get; protected set; }
        public int? ProjectID { get; protected set; }
        public string Project { get; protected set; }
        public int? LoadProfileID { get; protected set; }

        public DateTime ProcessingDateTime { get; protected set; } = DateTime.Now; //default.
        /// <summary>
        /// ProcessingDateTime without time component.
        /// </summary>
        public DateTime ProcessingDate => ProcessingDateTime.Date;
        /// <summary>
        /// ProcessingDateTime, time portion.
        /// </summary>
        public TimeSpan ProcessingTime => ProcessingDateTime.TimeOfDay;
        /// <summary>
        /// Status information to help specify the outcome of executing
        /// </summary>
        public string ExecutionStatusCode { get; protected set; }
        /// <summary>
        /// Job Namespace that the status belongs to. Prevent overlap with other jobs that may want to use the same code.
        /// </summary>
        public string ExecutionStatusNameSpace { get; protected set; }

        /// <summary>
        /// Full path to a file
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Size of the file. Should be set if FilePath is not null
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// Hash of the file content, for comparing duplication
        /// </summary>
        public string FileHash { get; set; }
        /// <summary>
        /// Name portion of filepath (No Directory information)
        /// </summary>
        public string FileName => FilePath == null ? null : System.IO.Path.GetFileName(FilePath);

        public int TotalExecutionTimeSeconds { get; protected set; } = 0;
        /// <summary>
        /// ExportBatchID from Metrix database
        /// </summary>
        public int? METRIX_ExportBatchID { get; set; }
        /// <summary>
        /// IsError
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// METRIX_LoadBatchID
        /// </summary>
        public int? METRIX_LoadBatchID { get; set; }

        public string Branch { get; protected set; } = "MAIN";
        public string PreviousBranch { get; protected set; }
        public short RetryCountBeforeFailureNotification { get; protected set; }

        #endregion

        #region DEBUG Property setters
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetBranch(string value)
        {
            Branch = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetPreviousBranch(string value)
        {
            PreviousBranch = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetRetryCountBeforeFailureNotification(short value)
        {
            RetryCountBeforeFailureNotification = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetJobExecutionID(long value)
        {
            JobExecutionID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetJobProfileID(int value)
        {
            JobProfileID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetExecutionStatusCode(string value)
        {
            ExecutionStatusCode = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetExecutionStatusNameSpace(string value)
        {
            ExecutionStatusNameSpace = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetProcessingDateTime(DateTime value)
        {
            ProcessingDateTime = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetOrganizationID(int value)
        {
            OrganizationID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetProjectID(int? value)
        {
            ProjectID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetJobID(int value)
        {
            JobID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetJobProfile_JobID(int value)
        {
            JobProfile_JobID = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetUserKey1(string value)
        {
            UserKey1 = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetUserKey(int value)
        {
            UserKey = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetStepNumber(int value)
        {
            StepNumber = value;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public void SetLoadProfileID(int value)
        {
            LoadProfileID = value;
        }
        #endregion
    }
}
