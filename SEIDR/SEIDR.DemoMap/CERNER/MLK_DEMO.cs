using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.Doc;
using SEIDR.JobBase;
using SEIDR.DemoMap;
using SEIDR.DemoMap.BaseImplementation;

namespace SEIDR.DemoMap
{
    [IJobMetaData(JobName: nameof(MLK_DEMO), NameSpace: NAMESPACE, Description: 
        "MLK Demo Map", ThreadName: "MLK DMAP", AllowRetry:false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class MLK_DEMO : CERNER.CernerBase
    {

        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
            //Cerner preFinancial transform.
            if (!base.PreFinancialTransform(acct, context))
                return false;

            acct.CurrentInsuranceBalance = acct.CurrentAccountBalance - acct.CurrentPatientBalance;
            return true;
        }

    }
}
