using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Globalization;

namespace SEIDR.DemoMap.EPIC
{
    [IJobMetaData(JobName: nameof(Tower_DMAP), NameSpace: NAMESPACE, Description:
        "Tower EPIC Demo Map", ThreadName: "TOWER DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class Tower_DMAP : EpicBase
    {
        public override void FixDate(Account a, string item) 
        {
           if(item.StartsWith("OccurDate"))
            {
                    a[item] =
                        string.IsNullOrEmpty(a[item])
                        ? null
                        : DateTime.ParseExact(a[item], "MMddyy", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            FixDates(acct);
            //acct.LastReconciliationDate = context.ProcessingDate.AddDays(-1);
            // *************  HARD CODED VALUES **********************
            // acct["BillingStatusDate"] = acct.LastReconciliationDate;
            // *************  HARD CODED VALUES **********************
            Doc.DocRecord source = (Doc.DocRecord)acct;

            acct["AdmitDiagnosis"] = acct["AdmitDiagnosis"]?.Split(',')[0];

            acct["DateofDeath"] = source[67] == "Y" ? acct["DischargeDate"] : null;

            acct["PatientMaritalStatus"] = acct["PatientMaritalStatus"]?.Substring(0, 1);

            acct["PatientLanguageCode"] = string.IsNullOrEmpty(acct["PatientLanguageCode"])
                ? "EN"
                : acct["PatientLanguageCode"]?.Substring(0, 2).ToUpper();

            acct["GuarantorLanguageCode"] = string.IsNullOrEmpty(acct["GuarantorLanguageCode"])
                ? "EN"
                : acct["GuarantorLanguageCode"]?.Substring(0, 2).ToUpper();

            acct["TotalPayments"] = (acct.GetMoney("TotalPatientPayments") + acct.GetMoney("TotalInsurancePayments")).ToString();
            acct["TotalAdjustments"] = (acct.GetMoney("PatientAdjustments") + acct.GetMoney("InsuranceAdjustments")).ToString();

            acct["PatientSSN"] = acct["PatientSSN"].Replace('X', '0').Replace('x', '0');

            //acct["ServiceLocationCode"] = string.IsNullOrEmpty(acct["SerivceLocationCode"])
            //    ? "UNK"
            //    : acct["ServiceLocationCode"];

            Decode(acct, "Country", "GuarantorCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Country", "PatientCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);

            acct.Inpatient = acct["PatientTypeCode"] == "I" ? true : false;

            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
            return base.PreFinancialTransform(acct, context); 
        }
        public override bool FinishTransform(Account acct, BasicContext context)
        {
            acct.BillingStatus = acct.OriginalBillDate != null ? BillingStatusCode.BILLED : BillingStatusCode.UNBILLED;
            // acct["TotalAdjustments"] = acct["PatientAdjustments"] + acct["InsuranceAdjustments"];
            return base.FinishTransform(acct, context);
        }
        public override void ValidateFinancialTotals(Account account, BasicContext context)
        {
            base.ValidateFinancialTotals(account, context);
        }
    }
}
