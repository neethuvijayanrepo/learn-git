using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class Project_InvoiceSettings
    {
        public int ProjectID { get; private set; }
        public string ProjectDescription { get; private set; }

        /*
         * Populated by calling APP.usp_Project_InvoiceSettings_ss
         * This procedure uses the function APP.ufn_GetSettingValue on each setting to get values - the function handles
         * coalescing with the default value for each setting, so should not have an issue with missing values.
         *
         */
        public decimal Rate { get; private set; }
        public int GracePeriodDaysAfterPlacement { get; private set; }
        public int GracePeriodDaysAfterCancellation { get; private set; }
        public int GracePeriodDaysAfterFirstFinalBill { get; private set; }
        public decimal MaxFeePerPlacement { get; private set; }
        public decimal MaxPaymentPerPlacement { get; private set; }
        public int GracePeriodDaysAfterInitialPlacement { get; private set; }
    }
}
