using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.EDI
{
    public class EdiCriteria
    {
        public int EdiCriteriaID { get; set; }
        public short ProjectID { get; set; }
        public string Project { get; set; }
        public short FacilityID { get; set; }
        public string Facility { get; set; }
        public bool IncludeInpatient { get; set; }
        public bool IncludeOutpatient { get; set; }
        public bool IncludeER { get; set; }
        public short VendorID { get; set; }
        public byte? Priority { get; set; }
        public int? ClaimAmountMin{ get; set; }
        public int? ClaimAmountMax { get; set; }
        public DateTime? ClaimReleaseDateStart { get; set; }
        public DateTime? ClaimReleaseDateEnd { get; set; }
    }
}
