using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public static class InvoiceRemark
    {
        public const string NONE = null;
        public const string DEFAULT_INVOICE_REMARK = "Normal rate applied";
        public const string NOT_INVOICED_EXCEED_PLACED = "Not Invoiced: amount invoiced already exceeds Placed Balance";
        public const string NO_PAYMENT_EXCEED_PLACED = "No payment: amount invoiced exceeds Placed Balance";
        public const string REFUND_AMOUNT_EXCEED_PLACED= "Refund due to adj/charges: amount invoiced exceeded Placed Balance";
        public const string PARTIAL_UP_TO_PLACED = "Partially invoiced up to Placed Balance";
        public const string PARTIAL_CREDIT_NET_PAY = "Partially Invoiced (previous net pay < 0)";
        public const string REFUND_FULL = "Refunded full amount";
        public const string NOT_INVOICED_ZERO_OR_CREDIT_NET_PAYMENTS = "Not Invoiced - net payments not > 0";
        public const string PARTIAL_PREVIOUS_BILLED = "Partial refund up to previously billed";
        public const string PARTIAL_NET_RECEIPTS = "Partial refund up to net receipts";
        public const string NO_REFUND_NET_RECEIPT_EXCEED_INVOICE = "No refund - net receipts >= amount invoiced";
        public const string NO_REFUND_NET_PREVIOUS_INVOICE_ZERO = "No refund - net previously invoiced = 0";
        public const string PLACED_AMOUNT_ADJUSTED = "Placed amt adjusted";
        public const string PLACED_AMOUNT_ORIGINAL = "Using Original Placed amt";
        public const string RATE_OVERRIDE = "Project Rate Override - Posting Date";
    }
}
