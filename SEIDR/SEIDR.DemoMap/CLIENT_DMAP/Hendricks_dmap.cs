using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    [IJobMetaData(JobName: nameof(Hendricks_DMAP), NameSpace: NAMESPACE, Description:
        "Hendricks Demo Map", ThreadName: "HENDRICKS DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class Hendricks_DMAP : Generic_dmap
    {
        public static void FixPhone(Account acct,string field)
        {
            acct[field] = acct[field] == "0000000000" ? "" : acct[field];
        }

        public override void FixSSN(Account acct,string field)
        {
            const string Pattern = @"\d{3}-\d{2}-\d{4}";
            string val = acct[field];
            if (string.IsNullOrEmpty(val) || val.Length < 9 || Regex.Match(val, Pattern).Success) return;
            acct[field] = string.Format("{0}-{1}-{2}", val.Substring(1, 3), val.Substring(4, 2), val.Substring(6, 4));
        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            // *************  HARD CODED VALUES **********************
            acct["PatientCountryCode"] = acct["GuarantorCountryCode"] = "US";       // hard code country USA
            acct["PatientLanguageCode"] = acct["GuarantorLanguageCode"] = "EN";     // hard code language to english
            acct["FirstBillDate"] = acct["OriginalBillDate"];
            acct["BillingStatusDate"] = acct.LastReconciliationDate;
            // *************  HARD CODED VALUES **********************

            acct["GuarantorRelationtoPatient"] = acct["GuarantorRelationtoPatient"].Substring(0, Math.Min(15,acct["GuarantorRelationtoPatient"].Length));
            acct.BillingStatus = acct.OriginalBillDate != null ? BillingStatusCode.BILLED : BillingStatusCode.UNBILLED;
            FixPhone(acct, "GuarantorEmployerPhone");  // setting to blank if phone number is all zeroes.

            acct["Ins1_LastBillDate"] = string.IsNullOrEmpty(acct["Ins1_LastBillDate"])
                ? (acct.OriginalBillDate?.ToString("MM/dd/yyyy"))
                : acct["Ins1_LastBillDate"];

            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
            acct.CurrentInsuranceBalance = acct.CurrentAccountBalance - acct.CurrentPatientBalance; // set ins bal to cur bal less patient bal
            acct.FlipSigns("TotalPatientPayments"); 
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