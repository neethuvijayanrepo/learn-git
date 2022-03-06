using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.HEALTHQUEST
{
    [IJobMetaData(JobName: nameof(UAB_DMAP), NameSpace: NAMESPACE, 
        Description: "UAB Demo Map", ThreadName: "UAB DMAP",
        NeedsFilePath: true, ConfigurationTable: "SEIDR.DemoMapJob", AllowRetry:false)]
    public class UAB_DMAP : HealthQuestBase
    {
        public override bool StartTransform(Account acct, HealthQuestContext context)
        {
            return base.StartTransform(acct, context);
        }
        public override bool PreFinancialTransform(Account acct, HealthQuestContext context)
        {
            return base.PreFinancialTransform(acct, context);
        }
        public override bool FinishTransform(Account account, HealthQuestContext context)
        {
            //Might want to comment out adding to the XR file? Not sure if it's really used for anything,
            //since it doesn't even have facility key.
            context.AddXRRecord(account); //add to the XR file 
            return base.FinishTransform(account, context);
        }
        public override void ValidateFinancialTotals(Account account, HealthQuestContext syncObject)
        {
            base.ValidateFinancialTotals(account, syncObject);
        }
    }
}
