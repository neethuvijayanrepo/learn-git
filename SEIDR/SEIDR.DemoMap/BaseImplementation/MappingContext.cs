using SEIDR.JobBase;

namespace SEIDR.DemoMap.BaseImplementation
{
    public class MappingContext: BaseContext
    {
        public override void SetStatus(bool success)
        {
            SetStatus(success ? DEFAULT_RESULT : DEFAULT_FAILURE, nameof(SEIDR));
        }
        public const ResultCode SUCCESS_BOUNDARY = ResultCode.SC;
        public const ResultCode COMPLETION_BOUNDARY = ResultCode.C;
        public const ResultCode DEFAULT_RESULT = ResultCode.SC;
        public const ResultCode DEFAULT_FAILURE = ResultCode.F;
        public const ResultCode DEFAULT_COMPLETE = ResultCode.C;

        /// <summary>
        /// Because some for some mapping processes, we may want to go to the end of file before returning, so may try to set status multiple times.
        /// </summary>
        public ResultCode CurrentStatus { get; private set; } = ResultCode.SC;

        public ExecutionStatus SetStatus(ResultCode code, string codeNameSpace = null)
        {
            if (CurrentStatus == code && ResultStatus != null)
                return ResultStatus;
            CurrentStatus = code;
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = code.In(DEFAULT_RESULT, DEFAULT_COMPLETE, DEFAULT_FAILURE)
                                    ? nameof(SEIDR)
                                    : nameof(DemoMap);
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