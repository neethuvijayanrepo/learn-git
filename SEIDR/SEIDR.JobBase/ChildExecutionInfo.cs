using System;
using System.Diagnostics;

namespace SEIDR.JobBase
{
    /// <summary>
    /// Child JobExecution information for creating a follow up JobExecution with slightly different information.
    /// <para>E.g., spawning for additional file paths or organization/Project/ProcessingDate overrides.</para>
    /// </summary>
    public class ChildExecutionInfo
    {
        public ChildExecutionInfo(JobBase.JobExecution parent, bool continueToNextStep = false)
        {
            Debug.Assert(parent.JobExecutionID != null, "parent.JobExecutionID != null");
            ParentJobExecutionID = parent.JobExecutionID.Value; //Readonly auto property
            JobProfileID = parent.JobProfileID;
            ProcessingDate = parent.ProcessingDate;
            OrganizationID = parent.OrganizationID;
            ProjectID = parent.ProjectID;
            FilePath = parent.FilePath;
            _parentStepNumber = parent.StepNumber;
            ContinueToNextStep = continueToNextStep;
            Branch = parent.Branch;
        }
        
        public ChildExecutionInfo(JobBase.JobExecution parent, string FilePath, bool continueToNextStep = false)
            : this(parent, continueToNextStep)
        {
            this.FilePath = FilePath;
        }

        private readonly int _parentStepNumber;
        public int StepNumber => ContinueToNextStep ? _parentStepNumber + 1 : 1;

        public string InitializationStatusCode { get; set; } = ExecutionStatus.SPAWN;
        public int JobProfileID { get; }
        public int? ProjectID { get; set; }
        public int? OrganizationID { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string FilePath { get; set; }
        public string Branch { get; set; }
        public long ParentJobExecutionID { get; }
        /// <summary>
        /// Allow controlling whether or not the child JobExecution will start over from StepNumber = 1, or move on to the following step.
        /// <para>If using Continue to Next step, should probably force the current JobExecution to a complete state.</para>
        /// </summary>
        public bool ContinueToNextStep { get; set; } = false;
    }
}
