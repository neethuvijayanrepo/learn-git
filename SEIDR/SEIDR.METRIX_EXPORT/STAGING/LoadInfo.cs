using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.STAGING
{
    public class LoadInfo
    {
        public int LoadProfileID { get; private set; }
        public string LoadBatchTypeCode { get; private set; }
        public DateTime? MinLastExportCompleteDate { get; private set; }
    }
}
