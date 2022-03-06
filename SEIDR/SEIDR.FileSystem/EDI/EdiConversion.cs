using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.EDI
{
    public class EdiConversion
    {
        public int JobProfile_JobID { get; set; }
        public int? CodePage { get; set; } = null;
        public string OutputFolder { get; set; } = null;
        public bool KeepOriginal { get; set; } = true;

        public int BlockSize = 10000; // Not to be configured except in testing.
    }
}
