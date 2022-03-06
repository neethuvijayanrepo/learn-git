using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.FileConcatenation
{
    public class FileConcatenationSettings
    {
        public string SecondaryFilePath { get; set; }
        public bool HasHeader { get; set; }
        public bool SecondaryFileHasHeader { get; set; }
        public string OutputPath { get; set; }
    }
}
