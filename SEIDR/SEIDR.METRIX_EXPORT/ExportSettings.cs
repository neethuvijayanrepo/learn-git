using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT
{
    public class ExportSetting
    {
        public string ArchiveLocation { get; private set; }
        public int MetrixDatabaseLookupID { get; private set; } //Lookup by key description.
        public string VendorName { get; private set; }
        public string ExportType { get; private set; }
        public string ImportType { get; private set; }

        public ExportSetting()
        {

        }

        public ExportSetting(string archiveLocation, int metrixDatabaseLookupID = 1, string vendorName = null,
            string exportType = null, string importType = null)
        {
            ArchiveLocation = archiveLocation;
            MetrixDatabaseLookupID = metrixDatabaseLookupID;
            VendorName = vendorName;
            ExportType = exportType;
            ImportType = importType;
        }
    }
}
