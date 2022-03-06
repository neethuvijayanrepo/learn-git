using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.METRIX_EXPORT.Statements
{
    [IJobMetaData(nameof(StatementXMLGenerationFollowupJob), nameof(METRIX_EXPORT), "Update ExportBatch and LetterInstance DateSent", false, false, DEFAULT_CONFIGURATION_TABLE)]
    public class StatementXMLGenerationFollowupJob : ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            var metrix = context.MetrixManager;
            using (var help = context.GetExportBatchHelperModel("APP.usp_LetterInstance_u_DateSent"))
            {
                help["FacilityID"] = DBNull.Value;
                help["ExportBatchStatusCode"] = ExportStatusCode.C;
                metrix.ExecuteNonQuery(help);
            }
            return DEFAULT_COMPLETE;
        }
    }
}
