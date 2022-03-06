using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.FTP
{
    public class FTPContext : JobBase.BaseContext
    {
        public const FTPResult SUCCESS_BOUNDARY = FTPResult.SC;
        public const FTPResult DEFAULT_RESULT = FTPResult.SC;
        public const FTPResult COMPLETION_BOUNDARY = FTPResult.C;
        public const FTPResult DEFAULT_FAILURE = FTPResult.F;
        /// <summary>
        /// Uses the FTPResult to set <see cref="FileSystemContext."/> 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeNameSpace"></param>
        /// <returns></returns>
        public ExecutionStatus SetStatus(FTPResult code, string codeNameSpace = null)
        {
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = code.In(SUCCESS_BOUNDARY, COMPLETION_BOUNDARY, DEFAULT_FAILURE)
                                    ? nameof(SEIDR)
                                    : nameof(FTP); 
            }
            ResultStatus = new ExecutionStatus
            {
                ExecutionStatusCode = code.ToString(),
                Description = code.GetDescription(),
                IsError = code < SUCCESS_BOUNDARY,
                IsComplete = code >= COMPLETION_BOUNDARY,
                NameSpace = codeNameSpace,
                SkipSuccessNotification = codeNameSpace != nameof(SEIDR)
            };
            return ResultStatus;
        }
    }
}
