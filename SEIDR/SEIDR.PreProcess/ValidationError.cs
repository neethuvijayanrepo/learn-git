using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.PreProcess
{
    [Obsolete("See ResultStatusCode used by SSISPackage/SSISExecutor.")]
    public enum ValidationError
    {
        None, //Note: 0 | x = x, so if var e is set to 0/None, e |= x => e becomes x

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
        /// PreProcess Job Sucess
        /// </summary>
        [Description("Success")]
        SS,
        /// <summary>
        /// FTP Job Fail
        /// </summary>
        [Description("PreProcess job failed. PreProcess Job details missing")]
        PP
    }
}
