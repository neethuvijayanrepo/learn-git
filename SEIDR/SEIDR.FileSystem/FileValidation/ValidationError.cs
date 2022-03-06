using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem
{
    /// <summary>
    /// Flag enum, see the flags section of <see href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/enumeration-types"/> for more information
    /// </summary>
    [Flags]
    public enum ValidationError
    {
        None, //Note: 0 | x = x, so if var e is set to 0/None, e |= x => e becomes x

        /// <summary>
        /// Column Count
        /// </summary>
        [Description("Column Count did not match Expected Column Count")]
        CC = 1,
        /// <summary>
        /// Column Name
        /// </summary>
        [Description("Column Name did not match expected Column name at this position")]
        CN = 2 * CC,
        [Description("Column name and Count did not match expected")]
        NC = CC | CN, // 2 | 1 = 3
        /// <summary>
        /// File Size
        /// </summary>
        [Description("File size did not match with required threshhold file size")]
        FS = 2 * CN,//4
        /// <summary>
        /// Unconfigured
        /// </summary>
        [Description("No Meta Data found for file, needs to be configured but job is not configured to run the metaData")]
        UC = 2 * FS, //8, give space in case we want combinations of file size, column name, column count, etc
        /// <summary>
        /// MetaData inferring
        /// </summary>
        [Description("Inferred Meta Data could not be inserted to Database")]
        MD = 2 * UC,
        /// <summary>
        /// Text Qualify Colum Count
        /// </summary>
        [Description("Column Count did not match with configured Text Qualify ColumnNumber count")]
        CT = 2 * MD,
        /// <summary>
        /// FTP remote path
        /// </summary>
        [Description("FTP/SFTP remote path not configured/invalid remote server path")]
        FR = 2 * CT,//32
        /// <summary>
        /// FTP local path
        /// </summary>
        [Description("FTP/SFTP local path not configured/invalid local server path")]
        FL = 2* FR,
        /// <summary>
        /// FTP Job Fail
        /// </summary>
        [Description("FTP/SFTP failure")]
        FT = 2 * FL
    }
}
