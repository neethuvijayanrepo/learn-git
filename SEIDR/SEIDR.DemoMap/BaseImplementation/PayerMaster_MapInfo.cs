using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap.BaseImplementation
{
    // ReSharper disable once InconsistentNaming
    public class PayerMaster_MapInfo
    {
        public PayerMaster_MapInfo() { }

        public PayerMaster_MapInfo(string payerCode, string facilityCode, bool selfPay)
        {
            this.PayerCode = payerCode;
            FacilityCode = facilityCode;
            IsSelfPay = selfPay;

            PayerType = selfPay ? PayerType.SELFPAY : PayerType.INSURANCE;
        }
        public string PayerCode { get; private set; }
        public string FacilityCode { get; private set; }
        public bool IsSelfPay { get; private set; }
        public PayerType PayerType { get; private set; } = PayerType.UNKNOWN;
    }
}
