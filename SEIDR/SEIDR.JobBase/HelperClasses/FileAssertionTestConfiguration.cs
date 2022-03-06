using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase.HelperClasses
{
    public class FileAssertionTestConfiguration
    {
        public string ExpectedOutputFile { get; private set; }
        public bool CheckColumnNameMatch { get; private set; }
        public bool CheckColumnOrderMatch { get; private set; }
        public string SkipColumns { get; set; }

        /// <summary>
        /// Default parameterless constructor needed for reflection.
        /// </summary>
        public FileAssertionTestConfiguration(){}
        public FileAssertionTestConfiguration(string ExpectedOutputPath, bool NameMatch = false, bool OrderMatch = false)
        {
            ExpectedOutputFile = ExpectedOutputPath;
            CheckColumnNameMatch = NameMatch;
            CheckColumnOrderMatch = OrderMatch;
        }
    }
}
