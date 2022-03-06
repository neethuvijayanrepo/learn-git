using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.Statements
{
    public class VendorBalanceModel
    {
        public string MRN { get; set; }
        [DisplayName("VISIT #")]
        public string VisitNumber { get; set; }
        [DisplayName("VISIT BALANCE")]
        public decimal VisitBalance { get; set; }
        [DisplayName("GUARANTOR NAME")]
        public string GuarantorName { get; set; }
        [DisplayName("DATE OF SERVICE")]
        public DateTime DateOfService { get; set; }
        [DisplayName("PATIENT NAME")]
        public string PatientName { get; set; }
        [DisplayName("SELF PAY DATE")]
        public DateTime? SelfPayDate { get; set; }
        public string BadDebt { get; set; }
    }

}

