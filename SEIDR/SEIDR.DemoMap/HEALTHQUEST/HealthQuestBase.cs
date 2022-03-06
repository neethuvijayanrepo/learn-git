using System;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.HEALTHQUEST
{
    [IJobMetaData(JobName: nameof(HealthQuestBase), NameSpace: NAMESPACE, Description: 
        "Health Quest basic Demo Map", ThreadName: "Generic HealthQuest",
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob",
        AllowRetry:false)]
    public class HealthQuestBase : DemoMapJob<HealthQuestContext>
    {
        public void RemoveNegativeSign(Account record) 
        {
            // Should we actually be just removing negative signs? Or actually flipping? Seems questionable.
            var columnList = GetMoneyColumns(record);

            foreach (var item in columnList)
            {
                string moneyField = record[item]; //?.Trim(); //Trim after checking null/white space
                if (!string.IsNullOrWhiteSpace(moneyField))
                    record[item] = moneyField.Replace("-", "").Trim();
                else
                    record[item] = "0";
            }
        }
        public void getBillingStatusCode(Account record , HealthQuestContext context)
        {
            if(record.OriginalBillDate.HasValue)
            {
                record.BillingStatus = BillingStatusCode.BILLED;
                record["BillingStatusDate"] = context.ReconciliationDate;
            }
            else
            {
                record.BillingStatus = BillingStatusCode.UNBILLED;
                record["BillingStatusDate"] = string.Empty;
            }
        }

        public void FormatLanguage(Account record)
        {
            var languageFormats = record.GetColumnsContaining("LanguageCode");
            foreach (var item in languageFormats)
            {
                string lang = record[item]; //?.Trim();
                if (string.IsNullOrWhiteSpace(lang))
                {
                    record[item] = "EN";
                    continue;
                }
                if (lang.Trim().Equals("SPA", StringComparison.OrdinalIgnoreCase))
                    record[item] = "SP";
                else
                    record[item] = "EN";
            }
        }
        public void getPatientTypeCode(Account record)
        {
            string patientTypeCode = record["PatientTypeCode"]?.Trim();
            record.Inpatient = patientTypeCode == "I";
        }

        public void getGuarantorName(Account record)
        {
            /*
            //Consider testing in a future client, where we'll have testing and review done over multiple days worth of files
            // Leave alone for a mature client, though.            

            record.FormatGuarantorName(NameHelperUpdateMode.Default);

            // which is equivalent to the following:
            var guarantorName = record.GuarantorName;
            if (guarantorName.IsEmpty)
                return;
            guarantorName.SetValues(NameHelperUpdateMode.Default);
            
            */

            string GuarantorLastName = record["GuarantorLastName"];
            if (string.IsNullOrEmpty(GuarantorLastName))
                return;
            string[] charSeparatorsGuarantor = GuarantorLastName.Split(',');
            if (charSeparatorsGuarantor.Length > 2)
                record["GuarantorMI"] = charSeparatorsGuarantor[2].Substring(0, 1).Trim();
            else
                record["GuarantorMI"] = null;
            record["GuarantorLastName"] = charSeparatorsGuarantor[0];
            record["GuarantorFirstName"] = charSeparatorsGuarantor[1];
        }
        public override bool StartTransform(Account acct, HealthQuestContext context)
        {
            getBillingStatusCode(acct, context);
            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, HealthQuestContext context)
        {
            RemoveNegativeSign(acct);
            FormatLanguage(acct);
            getPatientTypeCode(acct);
            getGuarantorName(acct);
            return base.PreFinancialTransform(acct, context);
        }
        
    }
}
