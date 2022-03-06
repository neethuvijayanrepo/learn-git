using System.Collections.Generic;
using System.Data;
using System.Linq;
using SEIDR.DataBase;

namespace SEIDR.MetrixProcessing.Invoice.Physician
{
    public class EncounterContainer
    {
        #region INVOICE RULE KEYS
        private const string RATE = nameof(RATE);
        private const string ADD_FEE = "Add Fee";
        private const string INCLUDE_FEE_SUM = "Include FeeSum";
        #endregion
        private readonly InvoicePreviewGenerator _caller;
        private readonly InvoicingContext _context;
        private int lastEncounter_ProjectID = 0;
        public string DefaultFinancialClassCode { get; private set; } = null;
        public EncounterContainer(InvoicePreviewGenerator caller, int encounterID, InvoicingContext context)
        {
            _caller = caller;
            _context = context;
            EncounterID = encounterID;
        }

        public void LoadEncounterInformation(IDataRecord record)
        {
            payerInfo = new InsurancePayers(record);
            enc = new EncounterDetail(record);
        }
        #region Encounter level data
        public readonly int EncounterID;
        private InsurancePayers payerInfo;
        //private List<PreviousPlacementInfo> previousPlacements; //Move into original temp table
        //private List<FinancialClassDateRange> financialClassRanges; //move into original temp table
        private EncounterDetail enc;

        Project_InvoiceSettings ProjectSettings => _caller.ProjectSettings;
        #endregion
        #region working variables
        
        /// <summary>
        /// Prior Invoiced Sum for Encounter
        /// </summary>
        private decimal billed = 0;
        /// <summary>
        /// Sum of transactions we have previously billed, plus running sum of transactions we are going to bill.
        /// <para>Increase at the end of evaluating a transaction_lineItem</para>
        /// </summary>
        private decimal billedAndBillable = 0;
        /// <summary>
        /// Original billable amount comes from the Placed Balance (Cannot bill for more than the patient owes at time of assignment to Metrix)
        /// <para>This is reduced as we go through transactions - each transaction we bill reduces the billable amount.</para>
        /// <para>NOTE: acute only modifies this value while going through BILLABLE transactions. Non billable transactions can still influence the AdjustedPlacedBalance, though, so they do still have an effect.</para>
        /// </summary>
        private decimal remainingBillable = 0;
        private decimal currentFee = 0;
        private decimal feesToDate = 0;
        private decimal adjustedFee = 0;
        private decimal paymentsToDate = 0;
        private decimal adjustedPlacedBalance = 0;
        private decimal lastAdjustedPlacedBalance = 0;
        private decimal previouslyBilled = 0;
        /// <summary>
        /// Used to track previous payments on each individual Transaction_LineItem
        /// </summary>
        private decimal previousPaymentsRunning = 0;
        /// <summary>
        /// Indicate that the AdjustedPlacedBalance is not used, due to going below original PlacedBalance
        /// </summary>
        private bool _placedBalanceFloorHit;
        

        #endregion

        void LoadEncounterData(InvoiceableTransaction_LineItem encounterFirstItem)
        {
            billed = enc.Billed;
            previousPaymentsRunning = enc.PreviousPayments;
            InitWorkVariables(encounterFirstItem);
        }

        void InitWorkVariables(InvoiceableTransaction_LineItem trans)
        {
            if (lastEncounter_ProjectID == trans.Encounter_ProjectID)
                return;
            // These should be for the entire invoice.
            //_currentFee = _adjustedFee = _adjustedPayment = 0; 
            billedAndBillable = previouslyBilled = billed; //Will add new transactions to this amount as we go through transactions for each encounter

            feesToDate = enc.FeeSum;
            //Note: feesToDate is only changed when Account_ProjectID changes in acute version.
            //(Ordered by placement date, so we want the fees prior to the first transaction being evaluated per encounter)
            
            _placedBalanceFloorHit = false;
            lastAdjustedPlacedBalance = trans.PlacedBalance + trans.NewCharges - trans.NewAdjustments;
            remainingBillable = lastAdjustedPlacedBalance - previouslyBilled;

            lastEncounter_ProjectID = trans.Encounter_ProjectID;

        }
        /// <summary>
        /// Set encounter details that need to be included in each preview record
        /// </summary>
        /// <param name="tran"></param>
        void SetDetails(InvoiceableTransaction_LineItem tran)
        {
            /*
            var code = financialClassRanges.FirstOrDefault(fc => fc.FromDateSerial <= tran.PostingDateSerial
                                                                 && fc.ThroughDateSerial > tran.PostingDateSerial)
                                           ?.FinancialClassCode
                       ?? DefaultFinancialClassCode;
            if(!string.IsNullOrEmpty(code))
                tran.FinancialClassCode = code;*/

            //tran.PreviousPlacement = previousPlacements.FirstOrDefault(p => p.Encounter_ProjectID == tran.Encounter_ProjectID);
            tran.PayerInfo = payerInfo;
            //Payments from transaction. Any payment/refund amounts, regardless of whether or not they are invoiceable/billable.
            tran.UpdatePreviousPayments(ref previousPaymentsRunning);
            tran.EncounterDetail = enc;
        }
        void Evaluate(InvoiceableTransaction_LineItem transaction)
        {
            decimal previousPayments = transaction.PreviousPayments;
            decimal paymentAmount = transaction.AmountApplied;
            paymentsToDate = previousPayments + paymentAmount; //AmountApplied is R and P type codes. No Adjustments.
            
            //Note: NewCharges/NewAdjustments are set by comparing post date to the current Encounter_Project's Placement Date
            //Might be able to move this into the running sum init when changing encounter or encounter_project?
            //Charges since the placement increase how much we can bill, while net debit adjustments reduce how much we can bill.
            adjustedPlacedBalance = transaction.PlacedBalance + transaction.NewCharges; //Add any new charges to the placed balance - we can bill for more
            adjustedPlacedBalance = adjustedPlacedBalance - transaction.NewAdjustments; //Take out any adjustments that we won't bill - less remaining billable
            if (adjustedPlacedBalance < transaction.PlacedBalance)
            {
                adjustedPlacedBalance = transaction.PlacedBalance;
                _placedBalanceFloorHit = true;
                // Note: probably not necessary to reset this in the next transaction
                // If one transaction goes below, the next ones probably will as well... maybe charge reversal, though?
            }
            else
                _placedBalanceFloorHit = false; // Charge reversal could potentially reverse so that we're not forcing using the original placed balance.
            /*
             RemainingBillable and lastAdjustedPlacedBalanced are initialized during Encounter_ProjectID change in the init method
                lastAdjustedPlacedBalance = trans.PlacedBalance + trans.NewCharges - trans.NewAdjustments;
                remainingBillable = lastAdjustedPlacedBalance - previouslyBilled;

                previouslyBilled is initialized to the billed from prior invoices
             *
             */
            //Initializes lastAdjustedPLacedBalance and remainingBillable to 0.
            //So first run, adjustedPlacedBalance != 0 => remainingBillable = 0 + adjustedPlacedBalance - 0 = adjustedPlacedBalance
            if (adjustedPlacedBalance != lastAdjustedPlacedBalance)
            {
                remainingBillable += adjustedPlacedBalance - lastAdjustedPlacedBalance; //Add difference to remaining billable.
                lastAdjustedPlacedBalance = adjustedPlacedBalance; //set last AdjustedPlacedBalance = adjustedPlacedBalance if it was different.
            }

            if (paymentAmount > 0)
            {
                if (remainingBillable <= 0)
                {
                    if (remainingBillable == 0)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.NOT_INVOICED_EXCEED_PLACED);
                    }
                    else if (billedAndBillable == 0)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.NO_PAYMENT_EXCEED_PLACED);
                    }
                    else if (billedAndBillable + remainingBillable > 0)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.REFUND_AMOUNT_EXCEED_PLACED, remainingBillable);
                    }
                    else
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.REFUND_AMOUNT_EXCEED_PLACED, billedAndBillable);
                    }
                }
                else if (paymentsToDate > 0)
                {
                    if (paymentAmount > remainingBillable && previousPayments >= 0)
                    {

                        transaction.SetInvoiceInfo(InvoiceRemark.PARTIAL_UP_TO_PLACED, remainingBillable);
                    }
                    else if (previousPayments < 0 && paymentsToDate > 0)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.PARTIAL_CREDIT_NET_PAY, paymentsToDate);
                    }
                    else
                    {
                        transaction.SetInvoiceInfo(amount: paymentAmount);
                    }
                }
                else
                {
                    transaction.SetInvoiceInfo(InvoiceRemark.NOT_INVOICED_ZERO_OR_CREDIT_NET_PAYMENTS);
                }
            }
            else //Payment <= 0
            {
                if (billedAndBillable + paymentAmount >= 0 && previousPayments <= billedAndBillable)
                {
                    transaction.SetInvoiceInfo(InvoiceRemark.REFUND_FULL, paymentAmount);
                }
                else if (billedAndBillable > 0 && paymentsToDate < billedAndBillable)
                {
                    if (paymentsToDate < 0)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.PARTIAL_PREVIOUS_BILLED, -billedAndBillable);
                        //transaction.InvoicedTransactionAmount = -BilledAndBillable;
                        //transaction.InvoiceRemark = "Partial refund up to previously billed";
                    }
                    else
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.PARTIAL_NET_RECEIPTS, paymentsToDate - billedAndBillable);
                        //transaction.InvoicedTransactionAmount = paymentsToDate - BilledAndBillable;
                        //transaction.InvoiceRemark = "Partial Refund up to net receipts";
                    }
                }
                else if (billedAndBillable > 0)
                {
                    if (paymentsToDate >= billedAndBillable)
                    {
                        transaction.SetInvoiceInfo(InvoiceRemark.NO_REFUND_NET_RECEIPT_EXCEED_INVOICE);
                    }
                }
                else
                {
                    transaction.SetInvoiceInfo(InvoiceRemark.NO_REFUND_NET_PREVIOUS_INVOICE_ZERO);
                }
            }

            if (adjustedPlacedBalance != transaction.PlacedBalance)
                transaction.AppendRemark(InvoiceRemark.PLACED_AMOUNT_ADJUSTED);
            else if (_placedBalanceFloorHit)
                transaction.AppendRemark(InvoiceRemark.PLACED_AMOUNT_ORIGINAL);

            var rate = _context.CheckRuleValue(RATE, transaction.PostingDate, ProjectSettings.Rate);
            transaction.Rate = rate;
            if (rate != ProjectSettings.Rate)
            {
                transaction.AppendRemark(InvoiceRemark.RATE_OVERRIDE);
            }
            currentFee = transaction.InvoicedTransactionAmount * rate;
            if (_context.CheckRuleFlag(ADD_FEE, transaction.PostingDate))
            {
                feesToDate += currentFee;
            }

            if (transaction.InvoicedTransactionAmount > 0)
            {
                if (ProjectSettings.MaxFeePerPlacement > 0)
                {
                    /*
                        Rules:
                            If payment is positive, and fees to date are more than @MaxFeePerPlacement, then charge the difference
                            Need to address scenario where subsequent take-back should not be credited if maximum hit previously.
                            Payments exceed fee/payment max
                            Amount billed will be reduced, so take back will consider total previously billed


                    */
                    if (feesToDate > ProjectSettings.MaxFeePerPlacement && currentFee > 0)
                    {
                        //exceed max fee threshold
                        if (feesToDate - currentFee < ProjectSettings.MaxFeePerPlacement)
                        {
                            adjustedFee = ProjectSettings.MaxFeePerPlacement - feesToDate + currentFee;
                            transaction.SetInvoiceInfo(amount: adjustedFee / rate);
                            feesToDate = feesToDate - currentFee + adjustedFee;
                        }
                        else
                        {
                            adjustedFee = 0;
                            transaction.SetInvoiceInfo(amount: 0);
                        }
                        transaction.AppendRemark($"{ProjectSettings.MaxFeePerPlacement:C} maximum per-placement limit reached: Fee of {currentFee:C} reduced to {adjustedFee:C}");
                    }
                }

                if (ProjectSettings.MaxPaymentPerPlacement > 0)
                {
                    if (billedAndBillable + transaction.InvoicedTransactionAmount > ProjectSettings.MaxPaymentPerPlacement)
                    {
                        decimal adjustedTransactionAmount = ProjectSettings.MaxPaymentPerPlacement - billedAndBillable;
                        if (adjustedTransactionAmount < 0)
                            adjustedTransactionAmount = 0;
                        paymentsToDate = paymentsToDate - transaction.InvoicedTransactionAmount + adjustedTransactionAmount;
                        transaction.SetInvoiceInfo($"{ProjectSettings.MaxPaymentPerPlacement:C} maximum per-placement limit reached. Pmt of {transaction.InvoicedTransactionAmount:C} reduced to {adjustedTransactionAmount:C}",
                                                   adjustedTransactionAmount); //new invoice amount

                    }
                }


                //If no remark, then normal rate.
                transaction.CheckDefaultRemark();

                //Hold the adjusted placed balance.
                transaction.AdjustedPlacedBalance = adjustedPlacedBalance;
                transaction.PreviouslyBilled = previouslyBilled = billedAndBillable;
                // - transaction.InvoicedTransactionAmount;
                // Move billedAndBillable increase to after setting previouslyBilled

                
                billedAndBillable += transaction.InvoicedTransactionAmount;
                //RunningSum += transaction.InvoicedTransactionAmount;
                remainingBillable -= transaction.InvoicedTransactionAmount; //Placement balance minus billed transactions.

                //transaction.RemainingBillable = remainingBillable;
                // Above *should* be correct, but Acute version uses following logic when inserting to the actual preview table,
                // and does not set the value of RemainingBillable in the temp table.
                transaction.RemainingBillable = adjustedPlacedBalance - previouslyBilled;

            }
        }
        List<InvoiceableTransaction_LineItem> Work { get; } = new List<InvoiceableTransaction_LineItem>();

        public void AddInvoiceItem(InvoiceableTransaction_LineItem item)
        {
            Work.Add(item);
        }
        public bool DoWork()
        {
            LoadEncounterData(Work[0]);
            foreach (var tran in Work)
            {
                SetDetails(tran);
                //Check if Encounter_ProjectID has changed and reset work variables if needed.
                InitWorkVariables(tran); 
                if (tran.CheckInvoiceSettings(_caller.ProjectSettings))
                {
                    //Evaluate for invoice values.
                    Evaluate(tran);
                }
                if (!_caller.CheckBulkCopy(tran))
                    return false;
            }
            return true;
        }

    }
}