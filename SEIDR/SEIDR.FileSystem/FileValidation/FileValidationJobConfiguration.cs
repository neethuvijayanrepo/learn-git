using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;

namespace SEIDR.FileSystem.FileValidation
{
    public class FileValidationJobConfiguration
    {
        public FileValidationJobConfiguration()
        {
            this.CurrentSwitch = "\0 \0";
            this.DesireSwitch = "\0\t\0";
        }

        const string GET_EXECUTION_INFO = "SEIDR.usp_FileValidationJob_ss";
        public int FileValidationJobID { get; set; }
        public int JobProfile_JobID { get; set; }
        public int SkipLines { get; set; } = 0;
        public bool HasHeader { get; set; } = true;
        public bool DoMetaDataConfiguration { get; set; } = true;
        public int? CurrentMetaDataVersion { get; set; }
        public string TextQualifier { get; set; } = "\"";
        public string Delimiter { get; set; } = "|"; //default delimiter for output..
        /// <summary>
        /// Multi Record type... Only do the merge with next line if sufficient column count. 
        /// Better to do a file split first generally, but only possible if the file has a 'record type' column
        /// </summary>
        public int MinimumColumnCountForMerge { get; set; } = 0;
        public int? SizeThreshold { get; set; }
        public byte? SizeThresholdDayRange { get; set; }
        public bool SizeThresholdWarningMode { get; set; } = false;
        public string NotificationList { get; set; }
        public bool HasTrailer { get; set; } = false;
        
        public bool RemoveTextQual { get; set; } = false;
        public int? TextQualifyColumnNumber { get; set; }
        public string CurrentSwitch { get; set; }
        public string DesireSwitch { get; set; }
        public char CurrentDelimiter { get; set; }
        public bool KeepOriginal { get; set; } = true;
        public string OverrideExtension { get; set; } = null;
        public bool LineEnd_CR { get; set; } = true;
        public bool LineEnd_LF { get; set; } = true;
        /// <summary>
        /// Get configuration details from database based on JobProfile_JobID
        /// </summary>
        /// <param name="dm">DatabaseManager</param>
        /// <param name="JobProfile_JobID">JobProfile_JobID</param>
        /// <returns></returns>
        public static FileValidationJobConfiguration GetFileValidationJobConfiguration(DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(JobProfile_JobID)] = JobProfile_JobID;
                return dm.SelectSingle<FileValidationJobConfiguration>(helper, true, false);
            }
        }
    }
}
