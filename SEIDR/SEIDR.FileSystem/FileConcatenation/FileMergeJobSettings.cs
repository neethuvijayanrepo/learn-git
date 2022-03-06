using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.FileConcatenation
{
    public class FileMergeJobSettings
    {
        public bool InnerJoin { get; set; }
        public string MergeFile { get; set; }
        public string LeftKey1 { get; set; }
        public string RightKey1 { get; set; }
        public string LeftKey2 { get; set; }
        public string RightKey2 { get; set; }
        public string LeftKey3 { get; set; }
        public string RightKey3 { get; set; }
        
        public string OutputFilePath { get; set; }
        public bool Overwrite { get;  set; }
        public bool PreSorted { get; set; }
        public bool CaseSensitive { get;  set; }

        public bool RightInputHasHeader { get;  set; }
        public bool LeftInputHasHeader { get;  set; }
        public bool IncludeHeader { get;  set; }
        /// <summary>
        /// Remove columns from the output where the column name is in both files. Keep the column from the left file.
        /// </summary>
        public bool RemoveDuplicateColumns { get; set; }
        /// <summary>
        /// Remove columns from the output file that were used for the right file's merging.
        /// </summary>
        public bool RemoveExtraMergeColumns { get; set; }
        
        public bool KeepDelimiter { get; set; } = true;
        public bool HasTextQualifier { get; set; } = true;
        public bool Trim { get; set; } = false;
    }
}
