using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap.BaseImplementation
{
    public enum BillingStatusCode
    {
        /// <summary>
        /// Unknown billing status. NOTE: this is not valid.
        /// </summary>
        UNKNOWN = 0,
        /// <summary>
        /// Unbilled - default when account does not have a final bill date.
        /// </summary>
        UNBILLED = 50,
        /// <summary>
        /// Billed - default when account has a final bill date
        /// </summary>
        BILLED = 100,
        /// <summary>
        /// Payment Plan Client - typically identified using statement load data.
        /// </summary>
        PAYPLAN_CLIENT = 180,
        /// <summary>
        /// Payment Plan. Not typically loaded.
        /// </summary>
        PAYPLAN = 210,
        /// <summary>
        /// Outside Vendor.
        /// </summary>
        VEND = 215,
        /// <summary>
        /// Bad Debt Account. Can technically set from A load/REC load, but should not. Would typically be filtered.
        /// </summary>
        BADDEBT = 255
    }
}
