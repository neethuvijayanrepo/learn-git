using SEIDR.DemoMap.BaseImplementation;
using SEIDR.DemoMap.CLIENT_DMAP;
using SEIDR.JobBase;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.EPIC
{
    public class EpicBase : Generic_dmap
    {
        private EpicClaims _EpicClaims = null;
        public EpicClaims Claims => _EpicClaims;

        public void ParseName(Account acct, string s, string firstNameField, string lastNameField)
        {
            s = String.IsNullOrEmpty(s) ? "" : s;
            string pattern = @"^(?<last>[\w\s-']+)\s*,\s*(?<first>.+$)";
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
                acct[firstNameField] = "";
            }
        }

        public void ParseNames(Account acct)
        {
            // parse last name from fields and put everything else in first name.
            ParseName(acct, acct.PatientName.FirstNameOriginal, "PatientFirstName", "PatientLastName");
            ParseName(acct, acct.GuarantorName.FirstNameOriginal, "GuarantorFirstName", "GuarantorLastName");
            ParseName(acct, acct["AttendingPhysicianFirstName"], "AttendingPhysicianFirstName", "AttendingPhysicianLastName");
        }

        public void FixPayerInfo(Account acct)
        {
            EpicClaimPacket ecp = Claims.FindPacket(acct.AccountNumber);
            if (ecp == null)
            {
                return;
            }

            for (int i = 1; i <= 4; i++)
            {
                Bucket b = acct[i];
                if (b == null)
                {
                    continue;
                }
                var ep = ecp.ClaimsByPayerCode;
                if (ep.ContainsKey(b.PayerCode))
                {
                    b.Balance = ep[b.PayerCode].TotalsForPayer;
                }
            }
            acct.OriginalBillDate = ecp?.FindFirstSentDate();
            FixBalanceField(acct, "CurrentAccountBalance");
            acct.CurrentInsuranceBalance = ecp == null ? 0 : ecp.TotalPacketAmount;
            acct.CurrentPatientBalance = ecp == null 
                ? acct.CurrentAccountBalance 
                : acct.CurrentAccountBalance - ecp.TotalPacketAmount;
        }

        public override BasicContext Setup(MappingContext callingContext, DemoMapJobConfiguration settings)
        {
            // idea is to copy claims file ( 06 ) to working folder alongside demo file ( 01 ) 
            string cfp = callingContext.Execution.FilePath.Replace("\\01_", "\\06_");
            var claimloc = callingContext.GetLocalFile<BasicLocalFileHelper>(cfp, true);
            EpicClaimsBuilder cb = new EpicClaimsBuilder(claimloc.WorkingFilePath,settings.Delimiter);
            _EpicClaims = cb.CreateClaims();
            return base.Setup(callingContext, settings); 
        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            // set insurance buckets based on info from claims gathered from 06 records.
            ParseNames(acct); // added as this is probably used for all epic clients.
            FixPayerInfo(acct);
            return base.StartTransform(acct, context);
        }

        public void FixBalanceField(Account a , string fieldName)
        {
            if (a[fieldName].Contains(".")) 
            	return;
            Type aType = typeof(Account);
            PropertyInfo piInstance = aType.GetProperty(fieldName);
            var pv = (System.Decimal)piInstance.GetValue(a, null);
            piInstance.SetValue(a, pv == 0 ? 0 : pv / 100);
        }

        public override bool FinishTransform(Account account, BasicContext context)
        {
            //foreach (var c in GetMoneyColumns(account))
            //{
            //    decimal? d = account.GetMoney(c.ColumnName);
            //    d = d != null && d != 0 ? d / 100 : 0;
            //    account[c] = d.ToString();
            //}
            account.FlipSigns(new string[] { "TotalPatientPayments","TotalInsurancePayments","TotalPayments","TotalAdjustments"});
            account.FlipSigns(new string[] { "Ins1_TotalPayments", "Ins2_TotalPayments", "Ins3_TotalPayments", "Ins4_TotalPayments" });
            account.FlipSigns(new string[] { "InsuranceAdjustments", "PatientAdjustments"});
            return base.FinishTransform(account, context);
        }
    }
}
