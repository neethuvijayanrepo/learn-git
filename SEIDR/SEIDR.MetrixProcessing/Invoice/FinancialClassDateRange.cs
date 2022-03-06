using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class FinancialClassDateRange
    {
        public string FinancialClassCode { get; private set; } //Could add description potentially?
        public int FromDateSerial { get; private set; }
        public int ThroughDateSerial { get; private set; }
    }
}
