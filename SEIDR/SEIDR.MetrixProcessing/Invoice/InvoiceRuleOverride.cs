using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class InvoiceRuleOverride
    {
        public const string SELECTOR = "APP.usp_Project_InvoiceRuleOverride_sl";
        public string InvoiceRuleKey { get; private set; }
        public decimal OverrideValue { get; set; }
        public bool OverrideFlag => OverrideValue != 0;
        public DateTime DateFrom { get; private set; }
        public DateTime DateThrough { get; private set; }
    }
}
