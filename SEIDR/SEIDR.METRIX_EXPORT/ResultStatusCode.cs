using System;
using System.ComponentModel;

namespace SEIDR.METRIX_EXPORT
{
    /// <summary>
    /// Result status code for determining SEIDR execution status.
    /// <para>See notes and <see cref="ExportJobBase.SUCCESS_BOUNDARY"/> and <see cref="ExportJobBase.COMPLETION_BOUNDARY"/> for some details to consider when adding or using result codes.
    /// </para>
    /// </summary>
    public enum ResultStatusCode
    {
        /*
         * NOTE FOR NEW STATUSES
         * Determine whether your status is an error, success, or success and complete (Error and complete is technically possible, but would need to be an explicit status.
         *
         * Once you know where your status fits, place it in the corresponding section below.
         * The ExportJobBase.GetExportStatus method will translate the enum into an ExportStatus object based on the following conventions:
            * If the enum's value is less than SC_SUCCESS, then the status is an error. Otherwise, it's a success
            * If the enum's value is greater than C_COMPLETE, then it is not only a success, but it is complete and the JobExecution will not process any more steps.
         *
         * Enum value is determined by order unless explicit. So for ease of adding future statuses, you should just put your status in the appropriate section of the enum comments below
         * Description attribute can be added to give a more detailed description of what your status means, such as when a very specific/important case is failed in your job.
         */

        /*** START Error Codes  *******/
        /// <summary>
        /// No File.
        /// </summary>
        [Description("No File")]
        NF,
        /// <summary>
        /// Data not exported. Primarily for Andromeda_staging -> Andromeda export
        /// </summary>
        [Description("Data Not Exported")]
        NE,
        /// <summary>
        /// JobExecution is out of date based on settings and Processing Date.
        /// </summary>
        [Description("Out of Date")]
        OD,
        /// <summary>
        /// General error, when the job determines that it cannot successfully complete the current step (validation failures). Used for <see cref="ExportJobBase.DEFAULT_FAILURE_CODE"/>
        /// <para>Ideally, we have a more specific error code to return, though.</para>
        /// </summary>
        [Description("General Failure - unable to complete job due to validation failures.")]
        IE,
        /// <summary>
        /// Pending Details.
        /// </summary>
        [Description("Pending Details - insufficient details to starte execution.")]
        PD,
        /// <summary>
        /// No Data to export.
        /// </summary>
        [Description("No Data to Export")]
        ND,
        /*** END ERROR CODES  ****/
        /// <summary>
        /// Basic success status code. Used for <see cref="ExportJobBase.SUCCESS_BOUNDARY"/> and <see cref="ExportJobBase.DEFAULT_RESULT"/> 
        /// </summary>
        [Description("Export Step Successful")]
        SC,
        /*** START Success, Incomplete codes *******/



        /*** END SUCCESS, INCOMPLETE CODES **/
        /*
         * Success codes for Complete without error - IsComplete = 1
         *
         * These will force SEIDR JobExecution to stop
         */
        /// <summary>
        /// Complete - Result Status Codes with values equal to or greater than this will forcefully stop SEIDR from continuing to any following step.
        /// <para>Used for <see cref="ExportJobBase.COMPLETION_BOUNDARY"/> and <see cref="ExportJobBase.DEFAULT_COMPLETE"/> </para>
        /// </summary>
        [Description("Metrix Export Status Updated")]
        C
        /*** START SUCCESS, COMPLETE CODES  *******/

    }
}
