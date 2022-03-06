using SEIDR.JobBase;

namespace SEIDR.METRIX_EXPORT.STAGING
{
    public class ExportContext : JobBase.BaseContext
    {
        public ExecutionStatus SetStatus(ResultStatusCode result)
        {
            var status = ExportJobBase.GetStatus(result);
            ResultStatus = status;
            return status;
        }
    }
}