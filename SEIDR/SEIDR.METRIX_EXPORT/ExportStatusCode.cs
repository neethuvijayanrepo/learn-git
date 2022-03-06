using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT
{
    /// <summary>
    /// Export statuses for the EXPORT.ExportBatch table
    /// </summary>
    public enum ExportStatusCode
    {
        
        /// <summary>
        /// SEIDR - Requested. JobExecution exists but has not started processing
        /// </summary>
        SR,
        /// <summary>
        /// SEIDR - Queued.
        /// <para>Use this when ready to begin processing - it is in the work queue for SEIDR (essentially, when the job has started)</para>
        /// </summary>
        SQ,
        /// <summary>
        /// SEIDR Intermediate - file complete.
        /// <para>Use this status when the job execution should be ready to send files to the vendors</para>
        /// </summary>
        SI,
        /// <summary>
        /// SEIDR Failure - File has not been delivered to the vendor and would require manual intervention to correct. Will not be worked by SEIDR.
        /// <para>Must be set by the ExportStatusUpdateJob - cannot set from the ExportBatchModel class</para>
        /// </summary>
        SF,
        /// <summary>
        /// SEIDR Completion - File has been delivered to the vendor. WILL not be worked by SEIDR.
        /// <para>Must be set by the ExportStatusUpdateJob - cannot set from the ExportBatchModel class</para>
        /// </summary>
        SC,
        /// <summary>
        /// SEIDR NO DATA
        /// </summary>
        ND,


        /// <summary>
        /// Pending Import
        /// </summary>
        P,

        /// <summary>
        /// Complete - included for testing/verification purposes.
        /// <para>Should not be expected under normal circumstances.</para>
        /// </summary>
        C

    }
}
