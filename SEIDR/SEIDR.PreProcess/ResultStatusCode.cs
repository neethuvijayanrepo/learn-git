using System.ComponentModel;
using SEIDR.FileSystem;

namespace SEIDR.PreProcess
{
    public enum ResultStatusCode
    {
        /*
       * NOTE FOR NEW STATUSES
       * Determine whether your status is an error, success, or success and complete (Error and complete is technically possible, but would need to be an explicit status.
       *
       * Once you know where your status fits, place it in the corresponding section below.
       * The ExportJobBase.GetExportStatus method will translate the enum into an ExportStatus object based on the following conventions:
          * If the enum's value is less than SC, then the status is an error. Otherwise, it's a success
          * If the enum's value is greater than C, then it is not only a success, but it is complete and the JobExecution will not process any more steps.
       *
       * Enum value is determined by order unless explicit. So for ease of adding future statuses, you should just put your status in the appropriate section of the enum comments below
       * Description attribute can be added to give a more detailed description of what your status means, such as when a very specific/important case is failed in your job.
       */

        /*** START Error Codes  *******/
        /// <summary>
        /// Configuration data
        /// </summary>
        [Description("Configuration data is not avilable.")]
        CD,
        /// <summary>
        /// CANCELLED
        /// </summary>
        [Description("CANCELLED")]
        CX,
        /// <summary>
        /// PreProcess Job Fail
        /// </summary>
        [Description("PreProcess job failed. Package path not set.")]
        PF,
        /// <summary>
        /// Package Started
        /// </summary>
        [Description("PreProcess job. Package Started.")]
        PS,
        /// <summary>
        /// PreProcess Job Fail
        /// </summary>
        [Description("FAILURE")]
        F,
        /// <summary>
        /// FTP Job Fail
        /// </summary>
        [Description("PreProcess job failed. PreProcess Job details missing")]
        PP,

        /*** END ERROR CODES  ****/


        /// <summary>
        /// Basic/default success status code.
        /// </summary>
        [Description("Step Successful")]
        SC,
        /*** START Success, Incomplete codes *******/

        /// <summary>
        /// PreProcess Job Sucess
        /// </summary>
        [Description("Success")]
        SS,

        /*** END SUCCESS, INCOMPLETE CODES **/
        /*
         * Success codes for Complete without error - IsComplete = 1
         *
         * These will force SEIDR JobExecution to stop
         */
        /// <summary>
        /// Complete - Result Status Codes with values equal to or greater than this will forcefully stop SEIDR from continuing to any following step.
        /// </summary>
        [Description("Complete")]
        C,
        /*** START SUCCESS, COMPLETE CODES  *******/
    }
}