using System;

namespace SEIDR.MetrixProcessing.Invoice.Physician
{
    public class InvoicedTransaction_LineItem
    {
        public int Transaction_LineItemID { get; private set; }
        public int InvoiceID { get; private set; }
        public int InvoiceDateSerial { get; private set; }
        public DateTime InvoiceDate { get; private set; }
        public int Encounter_ProjectID { get; private set; }
        public DateTime PostingDate { get; private set; }
        public int PostingDateSerial { get; private set; }
        /// <summary>
        /// Original Amount from AMB.Transaction_LineItem
        /// </summary>
        public decimal AmountApplied { get; private set; }
        /// <summary>
        /// Amount that could actually be included in Invoice
        /// </summary>
        public decimal InvoicedTransactionAmount { get; private set; }
        /// <summary>
        /// InvoicedTransactionAmount after applying Rate.
        /// </summary>
        public decimal InvoicedAmount { get; private set; }
    }
}