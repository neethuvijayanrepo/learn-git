using System;
using System.Collections.Generic;
using System.Data;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SEIDR.DemoMap.CLIENT_DMAP
{
    public class Generic_dmap : DemoMapJob
    {
        private Dictionary<string, Dictionary<string, string>> _decodes = null;
        public enum DecodeResponses
        {
            ON_FAILURE_NULL = 2,
            ON_FAILURE_DEFAULT = 4,
            ON_FAILURE_FAIL = 6,
            ON_FAILURE_NONE = 8
        };

        public Dictionary<string, Dictionary<string, string>> Decodes => _decodes;

        private void CreateDecodes(MappingContext context, DemoMapJobConfiguration settings)
        {
            var stagingConMgr = context.Executor.GetManager(settings.FileMapDatabaseID, false);
            DataTable dt = null;
            int orgID = context.Execution.OrganizationID;
            dt = stagingConMgr.ExecuteText($@"
select UPPER(lookup_group) as lookup_group,UPPER(search_val) as search_val,output_val 
from dataservices.Preprocess.DS_DMAP_LOOKUPS p WITH (NOLOCK)
WHERE DD is null AND organizationID={orgID}
OR (OrganizationID = -1 AND DD IS NULL
	AND NOT EXISTS(SELECT null 
					FROM dataservices.Preprocess.DS_DMAP_LOOKUPS
					WHERE Lookup_group = p.Lookup_Group 
					AND Search_Val = p.Search_Val
					AND OrganizationID = {orgID})
	)").Tables[0];
            Dictionary<string, Dictionary<string, string>> decodeTypes = new Dictionary<string, Dictionary<string, string>>();
            foreach (DataRow dr in dt.Rows)
            {
                string lg = dr["lookup_group"].ToString(), d = dr["search_val"].ToString(), v = dr["output_val"].ToString();
                Dictionary<string, string> curDict = decodeTypes.ContainsKey(lg) ? decodeTypes[lg] : new Dictionary<string, string>();
                curDict[d] = v;
                decodeTypes[lg] = curDict;
            }
            this._decodes = decodeTypes;
        }

        /*
         * Can be used either as a true lookup to replace a value on the account record or just to test if a given lookup is satisfied by 
         * only supplying the first 3 parameters.
         */
        public bool Decode(Account a, string lookupGroup, string fieldToSet = null, DecodeResponses d = DecodeResponses.ON_FAILURE_NONE)
        {
            if (string.IsNullOrEmpty(fieldToSet) || string.IsNullOrEmpty(lookupGroup))
        	{           
                a.Context.LogError($"Decode failed for account {a.AccountNumber} in group: {lookupGroup}. Field to set or lookup group was null.");
                return false;
            }

            string lookupVal = "___NULL___";
            if (a[fieldToSet] != null)
            {
                lookupVal = a[fieldToSet].ToUpper();
            }

            lookupGroup = lookupGroup.ToUpper();
            if (this.Decodes.ContainsKey(lookupGroup) && this.Decodes[lookupGroup].ContainsKey(lookupVal))
            {
                a[fieldToSet] = this.Decodes[lookupGroup][lookupVal];
                return true;
            }
            else if (d == DecodeResponses.ON_FAILURE_FAIL)
            {
                a.Context.LogError($"Decode failed for account {a.AccountNumber}.  Looking for {lookupVal} in decode table {lookupGroup}");
                return false;
            }
            else if (d == DecodeResponses.ON_FAILURE_NONE)
            {
                return true;
            }
            else if (d == DecodeResponses.ON_FAILURE_DEFAULT)
            {
                if (this.Decodes.ContainsKey(lookupGroup) && this.Decodes[lookupGroup].ContainsKey("_DEFAULT"))
                {
                    a[fieldToSet] = this.Decodes[lookupGroup]["_DEFAULT"];
	                return true;
	            }
                a.Context.LogError($"Decode failed for account {a.AccountNumber}.  No default value found for group: {lookupGroup}");
                return false;
            }
            else
            {
                a[fieldToSet] = null;
                return true;   // should get here if lookup failed and failure indicates use NULL.
            }
        }

        public bool DecodeCheck(Account a, string lookupGroup, string lookupVal)
        {
            lookupVal = a[lookupVal].ToUpper();
            lookupGroup = lookupGroup.ToUpper();
            if (this.Decodes.ContainsKey(lookupGroup) && this.Decodes[lookupGroup].ContainsKey(lookupVal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override BasicContext Setup(MappingContext callingContext, DemoMapJobConfiguration settings)
        {
            CreateDecodes(callingContext, settings);
            return base.Setup(callingContext, settings);
        }

        public virtual bool? SetInpatientFromPatientType(Account a, params string[] InpatientCodes)
        {
            bool? res;
            const string PATIENT_TYPE_CODE = "PatientTypeCode";
            string patType = a[PATIENT_TYPE_CODE];
            if (patType == null || InpatientCodes.Length == 0)
            {
                res = a.Inpatient = null;
            }
            else
                res = a.Inpatient = InpatientCodes.Exists(ip => ip.Equals(patType, StringComparison.OrdinalIgnoreCase));
            return res;
        }

        public virtual void FixDate(Account a, string item) { }

        public void FixDates(Account a)
        {
            var lstCheckDatesColumn = a.GetColumns(col =>
            {
                if (col.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (col.ColumnName.In(Account.SETTINGS_COLUMNS))
                {
                    return false;
                }

                if (col.ColumnName.Equals(LAST_RECON_DATE, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return col.ColumnName.IndexOf("DATE", StringComparison.OrdinalIgnoreCase) >= 0;
            });
            foreach (var item in lstCheckDatesColumn)
            {
                FixDate(a, item);
            }
        }

        private void FixBalanceField(Account a, string fieldName)
        {
            Type aType = typeof(Account);
            PropertyInfo piInstance = aType.GetProperty(fieldName);
            var pv = (System.Decimal)piInstance.GetValue(a, null);
            piInstance.SetValue(a, pv == 0 ? 0 : pv);
        }

        private void FixNumbers(Account a)
        {
            foreach (var c in GetMoneyColumns(a))
            {
                a[c] = string.IsNullOrEmpty(a[c]) ? "0.00" : a.GetMoney(c).ToString();
            }
            FixBalanceField(a, "CurrentAccountBalance");
            FixBalanceField(a, "CurrentPatientBalance");
            FixBalanceField(a, "CurrentInsuranceBalance");
            FixBalanceField(a, "NonBillableBalance");
        }

        public virtual void FixSSN(Account a, string columnName)
        {
            string test = a[columnName];
            if (string.IsNullOrEmpty(test))
                return;
            string pat = @"[0-9]+";
            string testr = Regex.Replace(test, @"[^0-9]", "");
            if (Regex.Match(testr, pat).Success)
            {
                // testr = testr.Length < 9 ? testr.PadLeft(9,'0') : testr;
                a[columnName] = testr;
            }
            else
            {
                a[columnName] = "000000000";
            }
        }

        private void _FixSSNFields(Account a)
        {
            foreach (var s in a.GetColumnsContaining("SSN"))
            {
                if (!s.ToString().EndsWith("SSN"))
                    continue;
                FixSSN(a, s);
            }
        }

        private void _FixRelationship(Account a, string columnName)
        {
            if (string.IsNullOrEmpty(a[columnName])) return;
            if (a[columnName].Equals("Self", StringComparison.InvariantCultureIgnoreCase))
            {
                a[columnName] = "18";
            }
        }

        private void _FixRelationships(Account a)
        {
            foreach (var s in a.GetColumnsContaining("RelationshipCode"))
            {
                _FixRelationship(a, s);
            }
        }

        public override bool StartTransform(Account acct, BasicContext context)
        {
            _FixSSNFields(acct);
           // _FixRelationships(acct);
            Decode(acct, "Homeless", "Homeless", DecodeResponses.ON_FAILURE_DEFAULT);  // can probably be used for all clients.
            //FixNumbers(acct); // temp fix before ryan implements proper fix. //See logic in Base.CleanMoneyFields
            return base.StartTransform(acct, context);
        }
    }
}
