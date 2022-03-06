using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.EDI
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EdiTransactionSetInfo
    {
        public int EDITransactionSetID { get; set; }
        public string Code { get; set; }
        public string VersionNumber { get; set; }
        public string ISA12_InterchangeControlVersionNumber { get; set; }
        public string GS08_VersionIdentifierCode { get; set; }
        public string ST03_ImplementationConventionReference { get; set; }

        public int EDIVendorID { get; set; }
        public string VendorName { get; set; }
        public int BatchTurnaroundHours { get; set; }
        public int MaxBatchRecordCount { get; set; }

        public string ISA01_AuthorizationInformationQualifier{ get; set; }
        public string ISA02_AuthorizationInformation { get; set; } = string.Empty;
        public string ISA03_SecurityInformationQualifier{ get; set; }
        public string ISA04_SecurityInformation { get; set; }
        public string ISA05_InterchangeIDQualifier { get; set; }
        public string ISA06_InterchangeSenderID { get; set; }
        public string ISA07_InterchangeIDQualifier { get; set; }
        public string ISA08_InterchangeReceiverID { get; set; }

        public string GS02_SenderCode { get; set; }
        public string GS03_ReceiverCode { get; set; }
    }
}
