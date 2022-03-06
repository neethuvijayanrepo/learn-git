using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    [IJobMetaData(JobName: nameof(Wellstar_DMAP), NameSpace: NAMESPACE, Description:
        "Wellstar Demo Map", ThreadName: "WELLSTAR DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class Wellstar_DMAP : Generic_dmap
    {
        bool IsCancelFile = false;

        public override void FixDate(Account a, string item)
        {
            a[item] = string.IsNullOrEmpty(a[item]) ? null : a[item].Substring(0, Math.Min(8, a[item].Length));  /// just a dummy placeholder for now.
            base.FixDate(a, item);
        }

        public void SetBillingStatus(Account a)
        {
            switch (a["BillingStatusCode"])
                {
                case "Closed":
                case "Billed":
                    a.BillingStatus = BaseImplementation.BillingStatusCode.BILLED;
                    break;
                case "Discharged/Not Billed":
                    a.BillingStatus = BaseImplementation.BillingStatusCode.UNBILLED;
                    break;
                default:
                    a.BillingStatus = BaseImplementation.BillingStatusCode.UNBILLED;
                    break;
            }
            a.BillingStatus = string.IsNullOrEmpty(a["BillingStatusCode"]) && a.OriginalBillDate != null ? BillingStatusCode.BILLED : a.BillingStatus;
        }

        private bool SetCountry(Account a, string field)
        {
            return Decode(a, "Country", field, DecodeResponses.ON_FAILURE_DEFAULT);
        }

        private bool SetLanguage(Account a, string field)
        {
            return Decode(a, "Language", field, DecodeResponses.ON_FAILURE_DEFAULT);
        }

        public override BasicContext Setup(MappingContext callingContext, DemoMapJobConfiguration settings)
        {
            IsCancelFile = callingContext.Execution.FileName.ToLower().Contains("_wdrawdemo_") ? true : false;
            return base.Setup(callingContext, settings);
        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            // set the cancel date for withdrawal records one day into future.
            // Just copying from existing wellstar dmap package .  not sure why.
            if (IsCancelFile)
            {
                DateTime CancelDate = context.ProcessingDate.AddDays(1);
                acct["CancelDate"] = CancelDate.ToString("MM/dd/yyyy");
            }

            // file has tag with date in first column.  So just return false to skip this row.
            if (acct.FacilityKey.StartsWith(context.ProcessingDate.ToString("MMddyyyy")))
            {
                return false;
            }
            FixDates(acct);
            acct["PatientTypeCode"] = acct["Inpatient"] == "0" ? "O" : "I";
            SetCountry(acct, "PatientCountryCode");
            SetCountry(acct, "GuarantorCountryCode");
            SetLanguage(acct, "PatientLanguageCode");
            SetLanguage(acct, "GuarantorLanguageCode");
            // *************  HARD CODED VALUES **********************
            // acct["BillingStatusDate"] = acct.LastReconciliationDate;
            // *************  HARD CODED VALUES **********************

            SetBillingStatus(acct);

            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
            string[] fieldsToFlip = {
                "TotalInsurancePayments",
                "TotalPatientPayments",
                "TotalPayments",
                "PatientAdjustments",
                "TotalAdjustments",
            };

            acct.FlipSigns(fieldsToFlip);

            return base.PreFinancialTransform(acct, context); 
        }
        public override bool FinishTransform(Account account, BasicContext context)
        {
            return base.FinishTransform(account, context);
        }
        public override void ValidateFinancialTotals(Account account, BasicContext context)
        {
            base.ValidateFinancialTotals(account, context);
        }
    }
}
