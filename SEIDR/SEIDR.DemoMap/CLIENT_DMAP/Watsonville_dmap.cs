using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    [IJobMetaData(JobName: nameof(Watsonville_DMAP), NameSpace: NAMESPACE, Description:
        "Watsonville Demo Map", ThreadName: "WATSONVILLE DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class Watsonville_DMAP : Generic_dmap
    {
        public void ParseName(Account acct, string s, string firstNameField, string lastNameField)
        {
            s = String.IsNullOrEmpty(s) ? "" : s;
            string pattern = @"^(?<last>[\w-]+\s)(?<first>.+$)";
            Regex r = new Regex(pattern);
            var matches = r.Match(s);
            if (matches.Groups[0].Success)
            {
                acct[firstNameField] = matches.Groups["first"].Value;
                acct[lastNameField] = matches.Groups["last"].Value;
            }
            else
            {
                acct[lastNameField] = s;
            }
        }

        public void ParseNames(Account acct)
        {
            // parse last name from fields and put everything else in first name.
            ParseName(acct, acct.PatientName.FirstNameOriginal, "PatientFirstName", "PatientLastName");
            ParseName(acct, acct.GuarantorName.FirstNameOriginal, "GuarantorFirstName", "GuarantorLastName");
            ParseName(acct, acct["AttendingPhysicianFirstName"], "AttendingPhysicianFirstName", "AttendingPhysicianLastName");
        }

        public string FormatSSN(string s)
        {
            if (s == "1")
            {
                return "000000000";
            }

            decimal d;
            return decimal.TryParse(s, out d) ? string.Format("{0:000000000}", d) : "000000000";
        }

        public void FixSSN(Account acct)
        {
            acct["GuarantorSSN"] = FormatSSN(acct["GuarantorSSN"]);
            acct["PatientSSN"] = FormatSSN(acct["PatientSSN"]);
        }

        public void ParseCityState(Account acct)
        {
            Regex regexObjj = new Regex(@"^(?<city>.+\w+\s)(?<state>[A-Z]{0}.+$)", RegexOptions.IgnoreCase);

            var x = regexObjj.Match(acct["PatientCity"]).Groups;
            if (x[0].Success)
            {
                acct["PatientState"] = x["state"].Value.Replace("  ", "").Trim();
                acct["PatientCity"] = x["city"].Value.Trim();
            }
        }

        public string FixPayerCode(string payercode, string misc)
        {
            return !string.IsNullOrWhiteSpace(payercode) && !string.IsNullOrWhiteSpace(misc)
                ? payercode + "-" + misc
                : payercode;
        }

        public string FixIsSelfPay(string payercode)
        {
            return string.IsNullOrEmpty(payercode) ? null : "0";
        }

        public void FixPhone(Account a, string phone)
        {
            // Replacing commas with nothing in phone number.  Came as a number 555,555,1222
            a[phone] = string.IsNullOrEmpty(a[phone]) ? null : a[phone].Replace(",", "").Replace(" ", "");
        }

        public void FixPayerInfo(Account acct)
        {
            for (int i = 1; i <= 4; i++)
            {
                Bucket b = acct[i];
                if (b == null)
                {
                    continue;
                }
                b.SetPayerInfo(FixPayerCode(b.PayerCode, acct["MISC" + i]), false);
            }
        }

        public void FixVendorCode(Account acct)
        {
            acct.VendorCode = string.IsNullOrEmpty(acct.VendorCode) || acct.VendorCode.Equals("???")
                                    ? "UNKNOWN"
                                    : acct.VendorCode;
        }

        public void SelfPayFix(Account acct)
        {
            bool decode = this.DecodeCheck(acct, "IsSelfPay", acct["FinancialClassCode"]);
            if (decode)  // we found this in list of self pay financial codes.
            {
                acct.CurrentPatientBalance = acct.CurrentAccountBalance; // put full balance to patient and zero out ins balance.
                acct.CurrentInsuranceBalance = 0;
                for (int i = 1; i <= 3; i++)
                {
                    if (acct[i] == null)
                    {
                        continue;
                    }
                    acct[i].Balance = 0;
                }
            }
        }

        public void FixLanguage(Account a, string s)
        {
            a[s] = string.IsNullOrEmpty(a[s])
                ? null
                : (a[s] == "spa" || a[s] == "SPANISH" ? "SP" : "EN");
        }

        public override void FixDate(Account a, string s)
        {
            if (string.IsNullOrEmpty(a[s]))
            {
                return;
            }
            if (a[s] == @"0/00/00" || a[s] == @"00/00/0001" || a[s] == "0001-01-01" || a[s] == @"01/01/0001")
            {
                a[s] = null;
            }

            // 0001-01-01
        }


        public override bool StartTransform(Account acct, BasicContext context)
        {
            // *************  HARD CODED VALUES **********************
            acct["GuarantorCountryCode"] = acct["PatientCountryCode"] = "US";   // hard code country code to US , not supplied by client.
            acct.FacilityKey = "WCH";                                           // Hard coding to WCH, only a single facility for this client.
            acct["BillingStatusDate"] = acct.LastReconciliationDate;

            if( !this.Decode(acct, "PatientTypeCode", "ServiceLocationCode", DecodeResponses.ON_FAILURE_FAIL))
            {
                return false; // returning false here will fail the SEIDR job. Error was logged in Decode method already.
            }

            // *************  HARD CODED VALUES **********************

            FixDates(acct);                     // have to deal with dummy dates

            acct.BillingStatus = BillingStatusCode.BILLED;
            if(acct.OriginalBillDate == new DateTime(0001,01,01))  // if original bill date is 0001-01-01 set to null
            {
                acct.OriginalBillDate = null;
                acct.BillingStatus = BillingStatusCode.UNBILLED;
            }
            
            FixPhone(acct, "PatientPhoneNumber"); // remove spaces and commas
            FixPhone(acct, "GuarantorPhoneNumber"); // remove spaces and commas
            FixPayerInfo(acct);             // Fix Is Self Pay flag and Payer code for ins1-4, also replace commas in phone numbers.           
            ParseNames(acct);               // Parse out the first and last name from single name field ( if possible )
            FixSSN(acct);                   // format SSN into numbers only and pad left if < 9 digits.  Some come as ###,###,### ( with commas )
            ParseCityState(acct);           // Parse city and state from city field.
            FixVendorCode(acct);            // set vendor code to UNKNOWN if empty or ???
            FixLanguage(acct, "PatientLanguageCode"); // set to null, SP or EN
            for (int i = 1; i <= 4; i++)
            {
                FixPhone(acct, "Ins" + i + "_BillToPhone");
            }

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
            SelfPayFix(acct);               // For self pay accounts move the balance into patient balance and zero out the insurance balances.

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
