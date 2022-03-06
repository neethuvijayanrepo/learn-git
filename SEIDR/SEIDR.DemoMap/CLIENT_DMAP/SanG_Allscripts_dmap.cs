using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    [IJobMetaData(JobName: nameof(SanG_Allscripts_dmap), NameSpace: NAMESPACE, Description:
        "SanG Allscripts Demo Map", ThreadName: "SanG Allscripts DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class SanG_Allscripts_dmap : Generic_dmap
    {
        //public override int VERSION_NUMBER { get => 2; }

        public override void FixDate(Account a, string item)
        {
            a[item] = a[item] == "0/0/0" ? null : a[item];
        }

        public void FixSSN(Account acct)
        {
            //    acct["GuarantorSSN"] = acct["GuarantorSSN"] == "UNKNOWN" || string.IsNullOrEmpty(acct["GuarantorSSN"]) ? "000000000" : acct["GuarantorSSN"];
            //    acct["PatientSSN"] = acct["PatientSSN"] == "UNKNOWN" || string.IsNullOrEmpty(acct["PatientSSN"]) ? "000000000" : acct["PatientSSN"];
            acct["GuarantorSSN"] = (acct["GuarantorSSN"] == "UNKNOWN" || acct["GuarantorSSN"] == "NONE") ? "000000000" : acct["GuarantorSSN"];
            acct["PatientSSN"] = (acct["PatientSSN"] == "UNKNOWN" || acct["PatientSSN"] == "NONE") ? "000000000" : acct["PatientSSN"];

        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            FixDates(acct);
            //FixSSN(acct);
            //DecodeHomeLess(acct);
            // *************  HARD CODED VALUES **********************
            acct["BillingStatusDate"] = acct.LastReconciliationDate;
            acct.BillingStatus = acct.OriginalBillDate != null ? BillingStatusCode.BILLED : BillingStatusCode.UNBILLED;
            //  acct["PatientCountryCode"] = DecodeCountry(acct["PatientCountryCode"]);
            //  acct["GuarantorCountryCode"] = DecodeCountry(acct["GuarantorCountryCode"]);
            acct.FacilityKey = "SGA";
            //  acct["PatientLanguageCode"] = DecodeLanguage(acct["PatientLanguageCode"]);
            //  acct["GuarantorLanguageCode"] = DecodeLanguage(acct["GuarantorLanguageCode"]);


            //DS-41759 calculating the Inpatient value from the Patent type code value           
            SetInpatientFromPatientType(acct, "IP");



            Decode(acct, "Country", "PatientCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Country", "GuarantorCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Language", "PatientLanguageCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Language", "GuarantorLanguageCode", DecodeResponses.ON_FAILURE_DEFAULT);

            Decode(acct, "Homeless", "Homeless", DecodeResponses.ON_FAILURE_DEFAULT);  // can probably be used for all clients.

            string[] fields = {"TotalPayments",
                "TotalInsurancePayments",
                "TotalPatientPayments",
                 "InsuranceAdjustments",
                "PatientAdjustments",
                "Ins1_TotalPayments",
                "Ins2_TotalPayments",
                "Ins3_TotalPayments",
                "TotalAdjustments"
                 };
            acct.FlipSigns(fields);

            acct.CurrentInsuranceBalance = acct.CurrentAccountBalance - acct.CurrentPatientBalance;

            // acct["PatientLanguageCode"]
            // *************  HARD CODED VALUES **********************

            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
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
