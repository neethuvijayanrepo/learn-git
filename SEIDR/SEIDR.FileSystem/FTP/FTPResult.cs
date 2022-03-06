using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.FTP
{
    public enum FTPResult
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
        /// Default failure status.
        /// </summary>
        [Description(nameof(JobBase.ExecutionStatus.FAILURE))]
        F,
        /// <summary>
        /// Process was working with a local file, but could not move file to its final/output path.
        /// </summary>
        [Description("Unable to move Local File to final path")]
        LO,
        /// <summary>
        /// Remote Target path is not configured.
        /// </summary>
        [Description("Missing Remote Target")]
        RT,
        /// <summary>
        /// Missing local file path configuration
        /// </summary>
        [Description("Local File  Missing")]
        FL,
        /// <summary>
        /// Missing remote file path configuration
        /// </summary>
        [Description("Remote File Missing")]
        FR,
        /// <summary>
        /// FTP Exception.
        /// </summary>
        [Description("General FTP Failure")]
        FT,
        /*** END ERROR CODES  ****/
        /// <summary>
        /// Basic success status code. Used for <see cref="FileSystemContext.SUCCESS_BOUNDARY"/> and <see cref="FileSystemContext.DEFAULT_RESULT"/> 
        /// </summary>
        [Description("Step Successful")]
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
        /// <para>Used for <see cref="FileSystemContext.COMPLETION_BOUNDARY"/> and <see cref="FileSystemHelper.DEFAULT_COMPLETE"/> </para>
        /// </summary>
        [Description("Complete")]
        C,
        /*** START SUCCESS, COMPLETE CODES  *******/
        /// <summary>
        /// Sync register - Complete
        /// </summary>
        [Description("Sync Registration Complete")]
        SR
    }
}
