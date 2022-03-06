using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.STAGING
{
    public class StagingToAndromedaExportStatusJobSettings
    {
        public bool CheckProject  {get; private set;}
        public bool CheckOrganization { get; private set; }
        public bool IgnoreProcessingDate { get; private set; }
        public bool RequireCurrentProcessingDate { get; private set; }
        public bool MonitoredOnly { get; private set;}
        public string LoadBatchTypeList { get; private set; }
        public int DatabaseLookupID { get; private set; }
        public string Message { get; set; }
        public bool IgnoreUnusedProfiles { get; private set; }
        //From JobExecution.
        public DateTime ProcessingDate { get; set; }

    }
}
