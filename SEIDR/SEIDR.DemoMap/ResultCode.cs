using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;

namespace SEIDR.DemoMap
{
    public enum ResultCode
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
        [Description("No Actual File found")]
        NA,
        [Description("No Expected file found")]
        NE,
        /// <summary>
        /// Post-Transformation validation failure.
        /// </summary>
        [Description("Post-transformation Validation Failure")]
        VF,
        [Description("File Validation Failed after Transform")]
        FV,
        /// <summary>
        /// Missing columns by column name.
        /// </summary>
        [Description("Missing Columns by Name")]
        MC,
        /// <summary>
        /// Column order
        /// </summary>
        [Description("Column Order")]
        CO,
        /// <summary>
        /// Data Discrepancy 
        /// </summary>
        [Description("Data Discrepancy")]
        DD,

        /*** END ERROR CODES  ****/
        /// <summary>
        /// Basic success status code. Used for <see cref="MappingContext.SUCCESS_BOUNDARY"/> and <see cref="MappingContext.DEFAULT_RESULT"/> 
        /// </summary>
        [Description("Step Successful")]
        SC,

        /*** END SUCCESS, INCOMPLETE CODES **/
        /*
         * Success codes for Complete without error - IsComplete = 1
         *
         * These will force SEIDR JobExecution to stop
         */
        /// <summary>
        /// Complete - Result Status Codes with values equal to or greater than this will forcefully stop SEIDR from continuing to any following step.
        /// <para>Used for <see cref="MappingContext.COMPLETION_BOUNDARY"/> and <see cref="MappingContext.DEFAULT_COMPLETE"/> </para>
        /// </summary>
        [Description("Complete")]
        C
        /*** START SUCCESS, COMPLETE CODES  *******/
    }
}
