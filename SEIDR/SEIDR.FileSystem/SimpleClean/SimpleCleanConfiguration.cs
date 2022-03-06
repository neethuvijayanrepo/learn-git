using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.SimpleClean
{
    public class SimpleCleanConfiguration
    {
        public bool LineEnd_CR { get; set; } = true;
        public bool LineEnd_LF { get; set; } = true;
        public int? Line_MinLength { get; set; } = null;
        public int? Line_MaxLength { get; set; } = null;
        public string Extension { get; set; } = "CLN";
        public int? BlockSize { get; set; } = null;
        public int? CodePage { get; set; } = null;
        public bool AddTrailer { get; set; } = false;

        public bool KeepOriginal { get; set; } = true;
    }
}
