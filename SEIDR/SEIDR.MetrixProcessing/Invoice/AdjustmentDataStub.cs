using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class AdjustmentDataStub
    {
        public string TypeCode { get; private set; }
        public int PostingDateSerial { get; private set; }
        public decimal Amount { get; private set; }
    }
}
