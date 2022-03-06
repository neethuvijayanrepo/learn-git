using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem
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
        /// Default failure status.
        /// </summary>
        [Description(nameof(SEIDR.JobBase.ExecutionStatus.FAILURE))]
        F,
        /// <summary>
        /// File not found
        /// </summary>
        [Description("File Not Found")]
        NF,
        /// <summary>
        /// No source file/directory
        /// </summary>
        [Description("No Source File/Directory")]
        NS,
        /// <summary>
        /// No Destination file/directory
        /// </summary>
        [Description("No Destination File/Directory")]
        ND,
        /// <summary>
        /// Invalid LoadProfile
        /// </summary>
        [Description("Invalid Profile")]
        IP,
        /// <summary>
        /// No root directory for Target
        /// </summary>
        [Description("No Root Directory for Target")]
        NR,
        /// <summary>
        /// Bad Destination configuration.
        /// </summary>
        [Description("Bad Destination Configuration")]
        BD,
        /// <summary>
        /// Target file already exists.
        /// </summary>
        [Description("Target File Already exists")]
        AE,
        /// <summary>
        /// File filter failure
        /// </summary>
        [Description("File Filter Failure")]
        FF,
        /// <summary>
        /// Missing Join Column
        /// </summary>
        [Description("Missing Join Column")]
        MC,
        /// <summary>
        /// Size check failure
        /// </summary>
        [Description("Size Check Failure")]
        SZ,
        /// <summary>
        /// Empty file - size check failure
        /// </summary>
        [Description("Size Check Failure - Empty file")]
        EF,
        /// <summary>
        /// Large file - size check failure
        /// </summary>
        [Description("Size Check Failure - Large File")]
        LF,
        /// <summary>
        /// Generic IO exception
        /// </summary>
        [Description("IO Exception")]
        IO,
        /// <summary>
        /// Duplicate columns in MetaData for files
        /// </summary>
        [Description("Duplicate Column in MetaData")]
        DC,
        /// <summary>
        /// Duplicate file
        /// </summary>
        [Description("Duplicate File")]
        DF,
        /// <summary>
        /// Type Code Mismatch
        /// </summary>
        [Description("UserKey1 versus LoadBatchTypeCode Mismatch")]
        TM,
        /// <summary>
        /// Sequence Check - Old file
        /// </summary>
        [Description("Out of Sequence - Old file")]
        SO,
        /// <summary>
        /// File Cleaning - High line length
        /// </summary>
        [Description("Max Length exceeded")]
        HL,
        /// <summary>
        /// File Cleaning - Low Line Length
        /// </summary>
        [Description("Min Length not reached")]
        LL,

        /*** END ERROR CODES  ****/
        /// <summary>
        /// Basic success status code. Used for <see cref="FileSystemContext.SUCCESS_BOUNDARY"/> and <see cref="FileSystemContext.DEFAULT_RESULT"/> 
        /// </summary>
        [Description("Step Successful")]
        SC,
        /*** START Success, Incomplete codes *******/
        [Description("Filter Match")]
        FM,


        /// <summary>
        /// File filter mismatch. Use for Branching opposite from FM
        /// </summary>
        [Description("Filter Mismatch. Differs from Expected Filter")]
        FD,

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
        C
        /*** START SUCCESS, COMPLETE CODES  *******/

    }
}
