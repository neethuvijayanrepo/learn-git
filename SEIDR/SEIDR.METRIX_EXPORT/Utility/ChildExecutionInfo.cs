using System;
using System.Diagnostics;

namespace SEIDR.METRIX_EXPORT.Utility
{
    /// <summary>
    /// Child JobExecution information for creating a follow up JobExecution with slightly different information.
    /// <para>E.g., spawning for additional file paths or organization/Project/ProcessingDate overrides.</para>
    /// </summary>
    public class ExportChildExecutionInfo
    {
        public ExportChildExecutionInfo(JobBase.JobExecution parent)
        {
            Debug.Assert(parent.JobExecutionID != null, "parent.JobExecutionID != null");
            ParentJobExecutionID = parent.JobExecutionID.Value; //Readonly auto property
            JobProfileID = parent.JobProfileID;
            ProcessingDate = parent.ProcessingDate;
            OrganizationID = parent.OrganizationID;
            ProjectID = parent.ProjectID;
            FilePath = parent.FilePath;
            _parentStepNumber = parent.StepNumber;
            Branch = parent.Branch;
        }

        public ExportChildExecutionInfo(JobBase.JobExecution parent, string ExportType)
            : this(parent)
        {
            this.ExportType = ExportType;
        }
        public ExportChildExecutionInfo(JobBase.JobExecution parent, string FilePath, string ExportType)
            : this(parent)
        {
            this.ExportType = ExportType;
            this.FilePath = FilePath;
        }
        private readonly int _parentStepNumber;
        public int StepNumber => ContinueToNextStep ? _parentStepNumber + 1 : 1;
        public string Branch { get; set; } = "MAIN";
        public int? UserKeyOverride { get; set; }

        public int JobProfileID { get; }
        public int? ProjectID { get; set; }
        public int? OrganizationID { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string FilePath { get; set; }
        public string ExportType { get; set; }
        public long ParentJobExecutionID { get; }
        /// <summary>
        /// Allow controlling whether or not the child JobExecution will start over from StepNumber = 1, or move on to the following step.
        /// <para>If using Continue to Next step, should probably force the current JobExecution to a complete state.</para>
        /// </summary>
        public bool ContinueToNextStep { get; set; } = false;
        /// <summary>
        /// For use with ExportBatch creation. Comes from userKeyOverride  (for now)
        /// </summary>
        public int? FacilityID => UserKeyOverride;
    }
}
