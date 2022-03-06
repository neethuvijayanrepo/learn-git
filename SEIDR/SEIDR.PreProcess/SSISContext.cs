using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.FileSystem;
using SEIDR.JobBase;

namespace SEIDR.PreProcess
{
    public class SSISContext : BaseContext
    {
        public const ResultStatusCode SUCCESS_BOUNDARY = ResultStatusCode.SC;
        public const ResultStatusCode DEFAULT_RESULT = ResultStatusCode.SC;
        public const ResultStatusCode COMPLETION_BOUNDARY = ResultStatusCode.C;
        public const ResultStatusCode DEFAULT_FAILURE = ResultStatusCode.F;
        /// <summary>
        /// Uses the ResultStatusCode to set <see cref="BaseContext.ResultStatus"/> 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeNameSpace"></param>
        /// <returns></returns>
        public ExecutionStatus SetStatus(ResultStatusCode code, string codeNameSpace = null)
        {
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = code.In(SUCCESS_BOUNDARY, COMPLETION_BOUNDARY)
                                    ? nameof(SEIDR)
                                    : null; //JobNameSpace
            }
            ResultStatus = new ExecutionStatus
            {
                ExecutionStatusCode = code.ToString(),
                Description = code.GetDescription(),
                IsError = code < SUCCESS_BOUNDARY,
                IsComplete = code >= COMPLETION_BOUNDARY,
                NameSpace = codeNameSpace
            };
            return ResultStatus;
        }
    }
}
