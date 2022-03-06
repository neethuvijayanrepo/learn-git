using System;
using System.Text;

namespace SEIDR.MetrixProcessing.Invoice.Physician
{
    public class InvoiceableTransaction_LineItem
    {
        public bool CheckInvoiceSettings(Project_InvoiceSettings settings)
        {
            Billable = AffectsInvoice;
            if (!AffectsInvoice)
                AppendRemark("No Charge: Excluded Trans Code");
            if (PostingDateSerial <= PlacementDateSerial + 
                (IsInitialEncounterPlacement?settings.GracePeriodDaysAfterInitialPlacement:settings.GracePeriodDaysAfterPlacement))
                
            {
                Billable = false;
                AppendRemark("No Charge: Within Placement Grace Period");
            }
            /*
             To Consider: Patient payments made before bill are probably deposits made at time of service or prior.
             Could add service date information and compare, potentially?
            if (PostingDateSerial < FirstFinalBillDateSerial && SourceCode == "P")
            {
                //Patient deposit?
                //Also, replace SourceCode from transaction type with type of payerMaster?
                Billable = false;
                AppendRemark("No Charge: Patient Deposit"); 
            } */
            if (PostingDateSerial <= FirstFinalBillDateSerial + settings.GracePeriodDaysAfterFirstFinalBill)
            {
                Billable = false;
                AppendRemark("No Charge: Within Final Bill Grace Period");
            }

            if (PostingDateSerial > CancellationDateSerial + settings.GracePeriodDaysAfterCancellation)
            {
                Billable = false;
                AppendRemark("No Charge: After Cancellation Grace Period");
            }
            else if (Billable && PostingDateSerial >= CancellationDateSerial)
                AppendRemark("Charge: Within cancellation Grace Period");
            ProjectID = settings.ProjectID;
            ProjectDescription = settings.ProjectDescription;
            GracePeriodDaysAfterCancellation = settings.GracePeriodDaysAfterCancellation;
            GracePeriodDaysAfterPlacement =  IsInitialEncounterPlacement?settings.GracePeriodDaysAfterInitialPlacement:settings.GracePeriodDaysAfterPlacement;
            GracePeriodDaysAfterFirstFinalBill = settings.GracePeriodDaysAfterFirstFinalBill;
            if (Billable)
                Rate = settings.Rate;
            else
                Rate = 0;
            return Billable;
        }
        [Obsolete("Use this only if we take NewCharges and NewAdjustments out of the InvoiceableTransaction_LineItem view.", true)]
        public void SetNewChargesAdj(decimal newCharges, decimal newAdj)
        {
            NewCharges = newCharges;
            NewAdjustments = newAdj;
        }
        [IgnoreBulkCopy]
        public bool Billable { get; private set; }

        public InvoiceableTransaction_LineItem(System.Data.IDataRecord source, DateTime invoiceDate)
        {
            InvoiceDate = invoiceDate;        
            Transaction_LineItemID = (int) source[nameof(Transaction_LineItemID)];
            TransactionID = (int) source[nameof(TransactionID)];
            LineItemID = source[nameof(LineItemID)] as int?;
            UndistributedTransaction = (bool) source[nameof(UndistributedTransaction)];
            AmountApplied = (decimal) source[nameof(AmountApplied)];

            PostingDate = (DateTime) source[nameof(PostingDate)];
            PostingDateSerial = (int) source[nameof(PostingDateSerial)];
            PlacementDate = (DateTime) source[nameof(PlacementDate)];
            PlacementDateSerial = (int) source[nameof(PlacementDateSerial)];

            //DaysToCancellation = (int)source[nameof(DaysToCancellation)];
            var tempObj = source[nameof(CancellationDate)];
            if (tempObj is DBNull)
            {
                CancellationDate = null;
                CancellationDateSerial = 75000;
                DaysToCancellation = null;
            }
            else
            {
                CancellationDate = (DateTime)tempObj;
                CancellationDateSerial = (int) source[nameof(CancellationDateSerial)];
                DaysToCancellation = CancellationDateSerial - PostingDateSerial;
            }

            /*
            PlacementAge = (int) source[nameof(PlacementAge)];
            FinalBillAge = (int) source[nameof(FinalBillAge)];
            
            //when possible, simple c# math is probably better performing,
            //since it doesn't need a name > column mapping lookup
            */
            PlacementAge = PostingDateSerial - PlacementDateSerial; 
            tempObj = source[nameof(FirstFinalBillDate)];
            if (tempObj is DBNull)
            {
                FirstFinalBillDate = null;
                FirstFinalBillDateSerial = PlacementDateSerial;
                FinalBillAge = PlacementAge; 
            }
            else
            {
                FirstFinalBillDate = (DateTime) tempObj;
                FirstFinalBillDateSerial = (int) source[nameof(FirstFinalBillDateSerial)];
                FinalBillAge = PostingDateSerial - FirstFinalBillDateSerial;
            }

            PreviousPlacementDate = source[nameof(PreviousPlacementDate)] as DateTime?;
            PreviousCancellationDate = source[nameof(PreviousCancellationDate)] as DateTime?;


            EncounterID = (int) source[nameof(EncounterID)];
            Encounter_ProjectID = (int) source[nameof(Encounter_ProjectID)];
            AccountID = (int) source[nameof(AccountID)];
            Account_ProjectID = (int) source[nameof(Account_ProjectID)];

            ServiceFromDate = source[nameof(ServiceFromDate)] as DateTime?;
            ServiceThroughDate = source[nameof(ServiceThroughDate)] as DateTime?;

            FacilityID = (short) source[nameof(FacilityID)];
            MonthEnd = (DateTime) source[nameof(MonthEnd)];
            /*
            if (!(source[nameof(ProjectFromDate)] is DBNull))
                ProjectFromDate = (DateTime) source[nameof(ProjectFromDate)]; 
            else
                ProjectFromDate = DateTime.MinValue; //Probably don't really need here...
            */

            Remark = source[nameof(Remark)] as string;

            FinancialClassCode = source[nameof(FinancialClassCode)] as string;
            //TransactionTypeID = (int) source[nameof(TransactionTypeID)];
            TransactionType = source[nameof(TransactionType)].ToString();
            TransactionCode = source[nameof(TransactionCode)].ToString();

            TypeCode = source[nameof(TypeCode)].ToString();
            SourceCode = source[nameof(SourceCode)].ToString();

            AffectsInvoice = (bool) source[nameof(AffectsInvoice)];
            

            PayerMasterID = source[nameof(PayerMasterID)] as int?;
            if(PayerMasterID.HasValue)
                PayerCode = source[nameof(PayerCode)].ToString();


            UndistributedTransaction = (bool) source[nameof(UndistributedTransaction)];
            IncludePatientSource = (bool)source[nameof(IncludePatientSource)];

            TotalTransactionAmount = (decimal) source[nameof(TotalTransactionAmount)];

            DC = (DateTime) source[nameof(DC)];
            PlacedBalance = (decimal) source[nameof(PlacedBalance)];
            //Sum up from c# instead. Can better reuse query results of charges/adjustments, and won't use DB processing
            //48s with including the filtered selects versus 
            NewAdjustments = (decimal)source[nameof(NewAdjustments)];
            NewCharges = (decimal)source[nameof(NewCharges)];

        }
        #region Properties for Load table

        public int ProjectID { get; private set; }
        public string ProjectDescription { get; private set; }
        //public decimal InvoicedAmount => InvoicedTransactionAmount;
        public decimal Rate { get; set; }
        public int GracePeriodDaysAfterPlacement { get; private set; }
        public int GracePeriodDaysAfterCancellation { get; private set; }
        public int GracePeriodDaysAfterFirstFinalBill { get; private set; }

        public bool InScope => Billable;
        public DateTime InvoiceDate { get; set; }



        #endregion
        #region view properties


        public int Transaction_LineItemID{ get; private set; }
        [IgnoreBulkCopy]
        public int TransactionID{ get; private set; }
        public int? LineItemID{ get; private set; }
        [IgnoreBulkCopy]
        public bool UndistributedTransaction{ get; private set; }
        [IgnoreBulkCopy]
        public decimal AmountApplied{ get; private set; }
        public int EncounterID{ get; private set; }
        public int Encounter_ProjectID { get; private set; }
        public int AccountID { get; private set; }
        [IgnoreBulkCopy]
        public int Account_ProjectID { get; private set; }
        [IgnoreBulkCopy]
        public short FacilityID { get; private set; }
        public int PostingDateSerial{ get; private set; }
        public DateTime PostingDate{ get; private set; }

        public DateTime MonthEnd{ get; private set; }
        /// <summary>
        /// Remark from the Transaction
        /// </summary>
        public string Remark { get; private set; }
        public string TransactionCode { get; private set; }
        public string TransactionType { get; private set; }
        [IgnoreBulkCopy]
        public string TypeCode { get; private set; }
        [IgnoreBulkCopy]
        public string SourceCode{ get; private set; }
        [IgnoreBulkCopy]
        public bool AffectsInvoice{ get; private set; }

        public string PayerCode { get; private set; }

        /// <summary>
        /// AMB.Transaction.PayerMasterID
        /// </summary>
        public int? PayerMasterID { get; private set; }

        /// <summary>
        /// AMB.Transaction.Amount
        /// </summary>
        [IgnoreBulkCopy]
        public decimal TotalTransactionAmount{ get; private set; }
        [IgnoreBulkCopy]
        public bool IncludePatientSource{ get; private set; }
        [IgnoreBulkCopy]
        public int FirstFinalBillDateSerial{ get; private set; }
        public DateTime? FirstFinalBillDate{ get; private set; }
        public DateTime PlacementDate{ get; private set; }
        [IgnoreBulkCopy]
        public int PlacementDateSerial{ get; private set; }
        public DateTime? CancellationDate{ get; private set; }
        [IgnoreBulkCopy]
        public int CancellationDateSerial { get; private set; }
        public int? DaysToCancellation{ get; private set; }
        public int PlacementAge{ get; private set; }
        public int FinalBillAge{ get; private set; }
        public decimal PlacedBalance{ get; private set; }
        [IgnoreBulkCopy]
        public DateTime DC { get; private set; }
        public DateTime TransactionLoadDate => DC;
        public decimal NewAdjustments { get; private set; }
        public decimal NewCharges { get; private set; }

        public DateTime? ServiceFromDate { get; private set; }
        public DateTime? ServiceThroughDate { get; private set; }
        [IgnoreBulkCopy]
        public bool IsInitialEncounterPlacement => !PreviousPlacementDate.HasValue;

        #endregion

        #region derived properties

        public EncounterDetail EncounterDetail;
        
        public string PatientName => EncounterDetail.PatientName;
        public decimal CurrentEncounterBalance => EncounterDetail.CurrentEncounterBalance;
        public decimal TotalCharges => EncounterDetail.TotalCharges;

         
        public string AccountNumber => EncounterDetail.AccountNumber;
        public string UserSpecifiedAccountNumber => EncounterDetail.UserSpecifiedAccountNumber;
        public string EncounterNumber => EncounterDetail.EncounterNumber;

        public InsurancePayers PayerInfo;
        public string Ins1 => PayerInfo.Ins1;
        public string Ins2 => PayerInfo.Ins2;
        public string Ins3 => PayerInfo.Ins3;
        public string Ins4 => PayerInfo.Ins4;
        
        public string FinancialClassCode { get; private set; }
        public DateTime? PreviousPlacementDate { get; private set; }
        public DateTime? PreviousCancellationDate { get; private set; }

        #endregion

        public decimal PreviousPayments { get; private set; }
        /// <summary>
        /// Transaction_LineItem.AmountApplied
        /// </summary>
        public decimal PaymentAmount => AmountApplied;
        /// <summary>
        /// Previous payments (prior to this transaction/lineItem) + this transaction/line Item's payment amount.
        /// </summary>
        public decimal PaymentsToDate => PreviousPayments + PaymentAmount;

        public void UpdatePreviousPayments(ref decimal previousPayments)
        {
            PreviousPayments = previousPayments;
            previousPayments += AmountApplied;
        }
        private readonly StringBuilder invoiceRemark = new StringBuilder();

        public decimal PreviouslyBilled { get; set; } = 0;
        public decimal RemainingBillable { get; set; } = 0;
        public decimal InScopePayment => Billable ? PaymentAmount : 0m;
        public decimal AdjustedPlacedBalance { get; set; }

        /// <summary>
        /// Invoice remark based on evaluating the transaction. Is not from the SQL
        /// </summary>
        public string InvoiceRemark => invoiceRemark.ToString();

        public decimal InvoicedTransactionAmount { get; private set; } = 0;
        public void SetInvoiceInfo(string remark = null, decimal amount = 0)
        {
            if(remark != null)
                AppendRemark(remark);
            InvoicedTransactionAmount = amount;
        }

        public void ClearRemark()
        {
            invoiceRemark.Clear();
        }
        /// <summary>
        /// Adds an additional clause to the <see cref="InvoiceRemark"/>. Do not add spacers (e.g., '; ')
        /// </summary>
        /// <param name="additionalRemark"></param>
        public void AppendRemark(string additionalRemark)
        {
            if (string.IsNullOrEmpty(additionalRemark))
                return;
            if(invoiceRemark.Length > 0)
                invoiceRemark.Append("; ");
            invoiceRemark.Append(additionalRemark);
        }
        
        /// <summary>
        /// If an invoice remark has not been set, sets InvoiceRemark to <see cref="Invoice.InvoiceRemark.DEFAULT_INVOICE_REMARK"/> 
        /// </summary>
        public void CheckDefaultRemark()
        {
            if (invoiceRemark.Length == 0)
                invoiceRemark.Append(Invoice.InvoiceRemark.DEFAULT_INVOICE_REMARK);
        }

    }
}
