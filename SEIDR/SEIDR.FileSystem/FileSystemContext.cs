using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem
{
    public class FileSystemContext : BaseContext
    {
        public const ResultStatusCode SUCCESS_BOUNDARY = ResultStatusCode.SC;
        public const ResultStatusCode DEFAULT_RESULT = ResultStatusCode.SC;
        public const ResultStatusCode COMPLETION_BOUNDARY = ResultStatusCode.C;
        public const ResultStatusCode DEFAULT_FAILURE = ResultStatusCode.F;
        /// <summary>
        /// Allow tracking the status that has been set most recently as an enum.
        /// <para>If the actual status is from a different enum (e.g. ValidationError), then it just defaults to ResultStatusCode.F</para>
        /// </summary>
        public ResultStatusCode? ResultCode { get; private set; } = null;
        /// <summary>
        /// Uses the ResultStatusCode to set <see cref="FileSystemContext"/>.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeNameSpace"></param>
        /// <returns></returns>
        public ExecutionStatus SetStatus(ResultStatusCode code, string codeNameSpace = null)
        {
            ResultCode = code;
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = code.In(SUCCESS_BOUNDARY, COMPLETION_BOUNDARY, DEFAULT_FAILURE) 
                                    ? nameof(SEIDR) 
                                    : nameof(FileSystem);
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

        public ExecutionStatus SetStatus(ValidationError code, string codeNameSpace = null)
        {
            if (code == ValidationError.None)
            {
                return SetStatus(DEFAULT_RESULT);
            }
            ResultCode = ResultStatusCode.F;
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = nameof(FileSystem);
            }
            ResultStatus = new ExecutionStatus
            {
                ExecutionStatusCode = code.ToString(),
                Description = code.GetDescription(),
                IsError = true,
                IsComplete = false,
                NameSpace = codeNameSpace
            };
            return ResultStatus;
        }
    }
}
