using SEIDR.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.DemoMap;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.CERNER
{
    [IJobMetaData(JobName: nameof(CernerBase), NameSpace: NAMESPACE, Description
        : "Cerner Generic Demo Map", ThreadName: nameof(CERNER), AllowRetry:false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob"
        )]
    public class CernerBase : DemoMapJob
    {

        public override bool StartTransform(Account a, BasicContext context)
        {
            ValidateSSN(a);
            FormatLanguage(a);

            //Generic - if can filter to very specific conditions, may be a slight improvement to performance sometimes. Worth testing
            //a.CheckBoolValueNoNull("SendToAlternateAddress", false); 
            AlternateAddress(a);

            TruncateState(a);
            TruncateExtension(a);
            return true;
        }
        /* Note: can override this again in an inheriting object.*/
        public override bool PreFinancialTransform(Account a, BasicContext context)
        {
            //Do this check here rather than in StartTransform, so that it's after date validations.
            a.BillingStatus = a.OriginalBillDate.HasValue ? BillingStatusCode.BILLED : BillingStatusCode.UNBILLED;
            a.SetDateTime("BillingStatusDate", a.OriginalBillDate);
            return base.PreFinancialTransform(a, context);
        }

        protected virtual void FormatLanguage(Account record)
        {
            foreach (var item in record.GetColumnsContaining("LanguageCode"))
            {
                string Lang = record[item]?.Trim();
                if (!string.IsNullOrEmpty(Lang) && Lang.Equals("Spanish", StringComparison.OrdinalIgnoreCase))
                    record[item] = "SP";
                else
                    record[item] = "EN";
            }
        }
        protected virtual void AlternateAddress(
            Account record
            //DocRecord record
            )
        {
            string AddressIndicator = record["SendToAlternateAddress"]?.Trim();
            if (!string.IsNullOrEmpty(AddressIndicator) && AddressIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
                record["SendToAlternateAddress"] = "1";
            else
                record["SendToAlternateAddress"] = "0";
        }

        protected virtual void ValidateSSN(Account record)
        {
            foreach (var item in record.GetColumnsContaining("SSN"))
            {
                string ssnNo = record[item]?.Trim();
                if (!string.IsNullOrEmpty(ssnNo) && ssnNo.Length == 9)
                    record[item] = ssnNo;
                else
                    record[item] = string.Empty;
            }
        }

        protected virtual void TruncateState(Account record)
        {

            foreach (var item in record.GetColumnsContaining("State"))
            {
                string StateName = record[item]?.Trim();
                if (!string.IsNullOrEmpty(StateName))
                {

                    if (StateName.Equals("District of Columbia", StringComparison.OrdinalIgnoreCase))
                        record[item] = "DC";
                    else if (StateName.Length > 2)
                        record[item] = StateName.Substring(0, 2);
                    else
                        record[item] = StateName;
                }
                else
                    record[item] = string.Empty;
            }
        }

        protected virtual void TruncateExtension(Account record)
        {
            const string EXTENSION = "BillToExtension";
            foreach (var b in record.Buckets)
            {
                string billToExt = b[EXTENSION];
                if (billToExt != null && billToExt.Length > 5)
                {
                    b[EXTENSION] = billToExt.Substring(0, 4);
                }
            }
            /*
            for (int i = 1; i <= 8; i++)
            {
                string extColName = GetInsuranceColumnName(i, "BillToExtension");

                string BilltoExtension = record[extColName]?.Trim();
                if (!string.IsNullOrEmpty(BilltoExtension))
                {
                    //Table meta data looks like it supports length of 10, so this is probably a data fix.
                    if (BilltoExtension.Length > 5)
                        record[extColName] = BilltoExtension.Substring(0, 4);
                    else
                        record[extColName] = BilltoExtension;
                }
                else
                    record[extColName] = string.Empty;
            }
            */
        }
        
    }
}
