using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class InvoicingContext : JobBase.BaseContext
    {
        public void CheckResetEvent(bool reset)
        {
            lock (resetSync)
            {
                if (reset && open)
                {
                    ResetEvent.Reset();
                    open = false;
                }
            }
            ResetEvent.WaitOne();
        }

        public void ClearResetEvent()
        {
            lock (resetSync)
                open = true;
            ResetEvent.Set();

        }

        private bool open;
        private readonly object resetSync = new object();
        System.Threading.ManualResetEvent ResetEvent = new System.Threading.ManualResetEvent(false);
        public const InvoiceResultCode SUCCESS_BOUNDARY = InvoiceResultCode.SC;
        public const InvoiceResultCode DEFAULT_RESULT = InvoiceResultCode.SC;
        public const InvoiceResultCode COMPLETION_BOUNDARY = InvoiceResultCode.C;
        public const InvoiceResultCode DEFAULT_FAILURE = InvoiceResultCode.F;
        /// <summary>
        /// Uses the ResultStatusCode to set <see cref="InvoiceResultCode"/> 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codeNameSpace"></param>
        /// <returns></returns>
        public ExecutionStatus SetStatus(InvoiceResultCode code, string codeNameSpace = null)
        {
            if (string.IsNullOrWhiteSpace(codeNameSpace))
            {
                codeNameSpace = code.In(SUCCESS_BOUNDARY, COMPLETION_BOUNDARY, DEFAULT_FAILURE)
                                    ? nameof(SEIDR)
                                    : nameof(Invoice);
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
        private List<InvoiceRuleOverride> _Rules;

        public Project_InvoiceSettings GetProjectSettings()
        {

            var map = new { ProjectID };
            //Multiple settingIDs, but treat as a single result set per Project. So select single.
            return Metrix.SelectSingle<Project_InvoiceSettings>(map, Schema: "APP");
        }

        public int ProjectID
        {
            get
            {
                if (Execution.ProjectID.HasValue)
                    return Execution.ProjectID.Value;
                throw new InvalidOperationException("Invoicing Context requires ProjectID");
            }
        }
        DataBase.DatabaseManager _Metrix;

        public DataBase.DatabaseManager Metrix
        {
            get
            {
                if (_Metrix == null)
                    _Metrix = Executor.GetManager("METRIX");
                return _Metrix;
            }
        }
        

        public void LoadOverrides()
        {
            using (var h = Metrix.GetBasicHelper())
            {
                h.QualifiedProcedure = InvoiceRuleOverride.SELECTOR;
                h[nameof(ProjectID)] = ProjectID;
                _Rules = Metrix.SelectList<InvoiceRuleOverride>(h);
            }
        }

        /// <summary>
        /// Checks that the default behavior for the invoice rule should be followed. Inverts the logic for CheckRuleOverrideFlag
        /// </summary>
        /// <param name="key"></param>
        /// <param name="postingDate"></param>
        /// <returns></returns>
        public bool CheckRuleFlag(string key, DateTime postingDate) 
            => _Rules.NotExists(r 
                                    => r.InvoiceRuleKey.Equals(key, StringComparison.OrdinalIgnoreCase)
                                       && postingDate >= r.DateFrom
                                       && postingDate < r.DateThrough
                                       && r.OverrideFlag);
        /// <summary>
        /// Checks if the InvoiceRule should not follow default behavior. (Override = true)
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="postingDate"></param>
        /// <returns></returns>
        public bool CheckRuleOverrideFlag(string Key, DateTime postingDate)
            => _Rules.Exists(r 
                                 => r.InvoiceRuleKey.Equals(Key, StringComparison.OrdinalIgnoreCase)
                                    && postingDate >= r.DateFrom
                                    && postingDate < r.DateThrough
                                    && r.OverrideFlag);
        /// <summary>
        /// Checks for the decimal value associated with the rule's override.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="postingDate"></param>
        /// <returns></returns>
        public decimal? CheckRuleOverrideValue(string key, DateTime postingDate) 
            => _Rules
               .FirstOrDefault(r 
                                   => r.InvoiceRuleKey.Equals(key, StringComparison.OrdinalIgnoreCase)
                                      && postingDate >= r.DateFrom
                                      && postingDate < r.DateThrough)
               ?.OverrideValue;
        /// <summary>
        /// Checks for the decimal value to use for the rule. If there's no override, returns <paramref name="original"/> 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="postingDate"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public decimal CheckRuleValue(string key, DateTime postingDate, decimal original)
            => _Rules
               .FirstOrDefault(r
                                   => r.InvoiceRuleKey.Equals(key, StringComparison.OrdinalIgnoreCase)
                                      && postingDate >= r.DateFrom
                                      && postingDate < r.DateThrough)
               ?.OverrideValue
               ?? original;

    }
}
