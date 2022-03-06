using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem.Scripting
{
    public class BatchScriptJobConfiguration
    {
        const string PARAMETER_SET = "\"{0}\" {1} \"{2}\" \"{3}\"";
        public string BatchScriptPath { get; set; }
        public string Parameter3 { get; set; }
        public string Parameter4 { get; set; }


        public string Args = null;

        public void SetupArgs(JobBase.JobExecution execution)
        {
            Args = string.Format(PARAMETER_SET, 
                    execution.FilePath, 
                    execution.JobExecutionID,
                    FS.ApplyDateMask(Parameter3, execution.ProcessingDate),
                    FS.ApplyDateMask(Parameter4, execution.ProcessingDate)
                    );
        }

    }
}
