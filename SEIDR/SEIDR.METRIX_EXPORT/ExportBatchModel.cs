using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT
{
    /// <summary>
    /// Model for describing an ExportBatch, taking information from Export.ExportBatch, Export.ExportProfile, Export.ExportType, and APP.Project (ProjectDescription)
    /// </summary>
    public class ExportBatchModel
    {
        public int ExportBatchID { get; private set; }
        /// <summary>
        /// ExportBatch.SubmissionDate - when initializing from SEIDR, we just set this to match up with the ProcessingDate
        /// </summary>
        public DateTime SubmissionDate { get; private set; }
        public int ExportProfileID { get; private set; }
        public int ExportTypeID { get; private set; }
        /// <summary>
        /// ExportType.ScopeCode
        /// </summary>
        public string ScopeCode { get; private set; }
        /// <summary>
        /// ExportType.BatchSize
        /// </summary>
        public int BatchSize { get; private set; }

        /// <summary>
        /// ExportType.GetDataSProc
        /// </summary>
        public string GetDataSProc { get; private set; }
        /// <summary>
        /// ExportType.GetDataSProc
        /// </summary>
        public string UpdateDataSProc { get; private set; }
        /// <summary>
        /// ExportType.IsMultiBatch
        /// </summary>
        public bool IsMultiBatch { get; private set; }
        /// <summary>
        /// ExportProfile.MaxBatchRecordCount
        /// </summary>
        public int? MaxBatchRecordCount { get; private set; }
        /// <summary>
        /// ExportProfile.MaxDBReadRecordCount
        /// </summary>
        public int? MaxDBReadRecordCount { get; private set; }
        /// <summary>
        /// ExportBatch.ExportBatchStatusCode
        /// </summary>
        public ExportStatusCode ExportBatchStatusCode { get; private set; }

        /// <summary>
        /// Has been pulled into/from the SEIDR work queue and begun executing
        /// </summary>
        public const ExportStatusCode QUEUED = ExportStatusCode.SQ;
        /// <summary>
        /// Job has been requested in SEIDR (JobExecution table). Has not been pulled from SEIDR work queue yet
        /// </summary>
        public const ExportStatusCode REQUESTED = ExportStatusCode.SR;
        /// <summary>
        /// Job has been completed up to final delivery to vendor.
        /// </summary>
        public const ExportStatusCode INTERMEDIATE_COMPLETION = ExportStatusCode.SI;
        /// <summary>
        /// Sets the ExportBatchStatusCode
        /// </summary>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public ExportBatchModel SetExportStatus(ExportStatusCode newStatus)
        {
            if (newStatus.In(ExportStatusCode.SF, ExportStatusCode.SC))
                throw new InvalidOperationException(
                    "Completion statuses should be set by the MetrixExportStatusUpdateJob, not through the ExportBatchModel.");
            ExportBatchStatusCode = newStatus;
            return this;
        }
        /// <summary>
        /// ExportBatch.OutputFilePath
        /// </summary>
        public string OutputFilePath { get; set; }

        /// <summary>
        /// FileName of <see cref="OutputFilePath"/> 
        /// </summary>
        public string OutputFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OutputFilePath))
                    return null;
                return System.IO.Path.GetFileName(OutputFilePath);
            }
        }

        /// <summary>
        /// ExportBatch.RecordCount
        /// </summary>
        public int? RecordCount { get; set; }
        /// <summary>
        /// ExportBatch.DateFrom
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// ExportBatch.DateThrough
        /// </summary>
        public DateTime? DateThrough { get; set; }
        /// <summary>
        /// ExportBatch.Active
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// ExportBatch.ProjectID
        /// </summary>
        public int? ProjectID { get; private set; }
        /// <summary>
        /// Project.Description
        /// </summary>
        public string Project { get; private set; }
        /// <summary>
        /// ExportBatch.FacilityID
        /// </summary>
        public short? FacilityID { get; private set; }
        /// <summary>
        /// Facility.Description
        /// </summary>
        public string Facility { get; private set; }
    }
}
