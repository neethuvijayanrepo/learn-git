using System.ComponentModel.DataAnnotations;

namespace TestProject.Utilities.Common
{
    /// <summary>
    /// Data Sort orders.
    /// </summary>
    public enum TestProjectSortOrder
    {
        ASC = 1,
        DESC = 2
    }
    /// <summary>
    /// Function execution status values.
    /// </summary>
    public enum ExecutionStatus
    {
        Error = 0,
        Success = 1
    }
    /// <summary>
    /// login status
    /// </summary>
    public enum loginstatus
    {
        success = 1,
        failure = 0,
        locked = 2
    }

    /// <summary>
    /// Record Status values for db entity
    /// </summary>
    public enum EntityStatus
    {
        [Display(Name = "Parent Deleted")]
        ParentDeleted = -2,

        [Display(Name = "Deleted")]
        Deleted = -1,

        [Display(Name = "Inactive")]
        Inactive = 0,

        [Display(Name = "Active")]
        Active = 1
    }

    public enum RevisionStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Send For Review")]
        SendForReview = 2,

        [Display(Name = "Rejected")]
        Rejected = 3,

        [Display(Name = "Approved")]
        Approved = 4,

        [Display(Name = "Published")]
        Published = 5,

        [Display(Name = "Archived")]
        Archived = 6
    }

    /// <summary>
    /// Execution status from DB.
    /// </summary>
    public enum DbExecutionStatus
    {
        Failed = 0,
        Success = 1
    }

}
