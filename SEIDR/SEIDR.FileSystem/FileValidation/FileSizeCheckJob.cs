using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.FileValidation
{
    [IJobMetaData(nameof(FileSizeCheckJob), nameof(SEIDR.FileSystem), 
        "Check that File Size is within a standard deviation range from the average file size.", 
        false,
        AllowRetry: false,
        ConfigurationTable: "SEIDR.FileSizeCheckJob",
        NotificationTime: 3 //Should be pretty quick to check.
        )]
    public class FileSizeCheckJob : ContextJobBase<FileSystemContext>
    {
        const int KB = 1000;
        const int MB = KB * 1000;
        const long GB = MB * 1000;
        const int KB_CHECK = 5 * KB;
        const int MB_CHECK = 5 * MB;
        const long GB_CHECK = 5 * GB;

        public override void Process(FileSystemContext context)
        {
            var execution = context.Execution;
            //Note: Essentially all logic is in SQL, so a unit test from C# doesn't really do much.
            execution.RefreshFileInfo();
            if (execution.FileSize == null)
            {
                context.SetStatus(ResultStatusCode.NS);
                return;
            }

            SizeCheckModel mapObj = new SizeCheckModel(execution);
            context.Manager.ExecuteNonQuery("SEIDR.usp_FileSizeCheckJob_CheckFile", mapObj);

            if (!mapObj.AllowContinue)
            {
                //ToDo: check if file is empty according to DB settings rather than just completely empty.
                //Similar for large file
                if (mapObj.Empty)
                    context.SetStatus(ResultStatusCode.EF);
                else if (mapObj.LargeFile)
                    context.SetStatus(ResultStatusCode.LF);
                else
                    context.SetStatus(ResultStatusCode.SZ);
            }

            if (mapObj.Message == null && mapObj.Deviation == 0)
                return; //Nothing to log.

            if (mapObj.Deviation != 0)
            {
                string devMessage = "Deviation from Average: " +
                                    (mapObj.Deviation < KB_CHECK
                                         ? mapObj.Deviation + "B" // < 5 KB, use B
                                         : mapObj.Deviation < MB_CHECK
                                             ? mapObj.Deviation / KB + nameof(KB) // < 5MB, use KB
                                             : mapObj.Deviation < GB_CHECK
                                                 ? mapObj.Deviation / MB + nameof(MB) // < 5GB use MB
                                                 : mapObj.Deviation / GB + nameof(GB)); //use GB for anything bigger

                if (string.IsNullOrEmpty(mapObj.Message))
                    mapObj.Message = devMessage;
                else
                    mapObj.Message += $"{Environment.NewLine}{devMessage}";
            }

            if (mapObj.AllowContinue)
                context.LogInfo(mapObj.Message);
            else
                context.LogError(mapObj.Message);
        }
    }
}
