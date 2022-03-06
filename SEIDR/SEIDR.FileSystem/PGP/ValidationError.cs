using System;
using System.ComponentModel;


namespace SEIDR.FileSystem.PGP
{
    public enum ValidationError
    {
        None, //Note: 0 | x = x, so if var e is set to 0/None, e |= x => e becomes x
        /// <summary>
        /// PGP source path.
        /// </summary>
        [Description("PGP source path not configured/invalid source path.")]
        PS,
        /// <summary>
        /// PGP output path.
        /// </summary>
        [Description("PGP output path not configured/invalid output path.")]
        PO,
        /// <summary>
        /// PGP job failed.
        /// </summary>
        [Description("PGP job failed. For more info check SEIDR.JobExecutionError database table.")]
        PJ,
        /// <summary>
        /// PGP private key file not configured.
        /// </summary>
        [Description("PGP private key file not configured or path//file not exists.")]
        PI,
        /// <summary>
        /// PGP private key file not configured.
        /// </summary>
        [Description("PGP public key file not configured or path//file not exists.")]
        PU
    }
}
