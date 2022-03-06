using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap
{
    /// <summary>
    /// Owner information for an address. InsX_BillTo can be parsed from sequence number.
    /// </summary>
    public enum AddressOwner
    {
        Ins1_BillTo = 1,
        Ins2_BillTo = 2,
        Ins3_BillTo = 3,
        Ins4_BillTo = 4,
        Ins5_BillTo = 5,
        Ins6_BillTo = 6,
        Ins7_BillTo = 7,
        Ins8_BillTo = 8,
        Patient,
        Guarantor,
        PatientEmployer,
        GuarantorEmployer
    }
}
