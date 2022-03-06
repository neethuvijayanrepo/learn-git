using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    [IJobMetaData(JobName: nameof(CCMC_DMAP), NameSpace: NAMESPACE, Description:
        "CCMC Demo Map", ThreadName: "CCMC DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class CCMC_DMAP : Generic_dmap
    {
  


        public override bool StartTransform(Account acct, BasicContext context)
        {

            Decode(acct, "Country", "GuarantorCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Country", "PatientCountryCode", DecodeResponses.ON_FAILURE_DEFAULT);

            acct["BillingStatusDate"] = acct.LastReconciliationDate;

            FixDates(acct);                     // have to deal with dummy dates

            acct.BillingStatus = BillingStatusCode.BILLED;
            if(acct.OriginalBillDate == new DateTime(0001,01,01))  // if original bill date is 0001-01-01 set to null
            {
                acct.OriginalBillDate = null;
                acct.BillingStatus = BillingStatusCode.UNBILLED;
            }

            //DS-41823 calculating the Inpatient value from the Patent type code value           
            SetInpatientFromPatientType(acct, "I");

            Decode(acct, "Language", "PatientLanguageCode", DecodeResponses.ON_FAILURE_DEFAULT);
            Decode(acct, "Language", "GuarantorLanguageCode", DecodeResponses.ON_FAILURE_DEFAULT);
           Decode(acct, "State", "GuarantorState", DecodeResponses.ON_FAILURE_DEFAULT);
           Decode(acct, "State", "PatientState", DecodeResponses.ON_FAILURE_DEFAULT);
            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {

            string[] signFlips = new string[] {
                "TotalPayments",
                "TotalInsurancePayments",
                "TotalPatientPayments",
                "Ins1_TotalPayments",
                "Ins2_TotalPayments",
                "Ins3_TotalPayments",
                "TotalAdjustments",
                "InsuranceAdjustments",
                "PatientAdjustments"
            };
            acct.FlipSigns(signFlips);                // flip some of the signs to make them Metrix friendly.
           // SelfPayFix(acct);               // For self pay accounts move the balance into patient balance and zero out the insurance balances.

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
