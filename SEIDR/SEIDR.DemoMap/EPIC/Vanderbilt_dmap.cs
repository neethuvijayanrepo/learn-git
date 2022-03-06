using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.EPIC
{
    [IJobMetaData(JobName:nameof(Vanderbilt_DMAP), NameSpace: NAMESPACE, Description:
        "Vanderbilt Demo Map", ThreadName: "VANDERBILT DMAP", AllowRetry: false,
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob")]
    public class Vanderbilt_DMAP : EpicBase
    {

        public override bool StartTransform(Account acct, BasicContext context)
        {
            // *************  HARD CODED VALUES **********************
            acct["BillingStatusDate"] = acct.LastReconciliationDate;
            // *************  HARD CODED VALUES **********************

            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, BasicContext context)
        {
            return base.PreFinancialTransform(acct, context); 
        }
        public override bool FinishTransform(Account acct, BasicContext context)
        {
            acct.BillingStatus = acct.OriginalBillDate != null ? BillingStatusCode.BILLED : BillingStatusCode.UNBILLED;
            return base.FinishTransform(acct, context);
        }
        public override void ValidateFinancialTotals(Account account, BasicContext context)
        {
            base.ValidateFinancialTotals(account, context);
        }
    }
}
