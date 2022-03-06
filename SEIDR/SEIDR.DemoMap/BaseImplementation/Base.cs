

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;
using SEIDR.DemoMap;
using SEIDR.Doc;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.BaseImplementation
{
    
    [IJobMetaData(JobName: "DMAP Base", NameSpace: NAMESPACE, Description: "Base DemoMap", 
        NeedsFilePath:true, ConfigurationTable:"SEIDR.DemoMapJob",
        AllowRetry:false)]
    public class DemoMapJob: DemoMapJob<BasicContext>
    {

    }
    public abstract partial class DemoMapJob<T> : ContextJobBase<MappingContext>
        where T: ContextObjectBase, new()
    {
        public const string NAMESPACE = nameof(DemoMap);

        #region virtual methods for client implementations
        /// <summary>
        /// Undefined virtual method to validate financial totals.
        /// <para>By default, this is called after bad bucket and self pay transformations, but before OOO evaluation.</para>
        /// <para>Gives a chance to validate financial totals after checking bad buckets/self pay before OOO buckets are created</para>
        /// </summary>
        /// <param name="account"></param>
        /// <param name="syncObject"></param>
        public virtual void ValidateFinancialTotals(Account account, T syncObject)
        {

        }
        /// <summary>
        /// Start transformation of a line from the file. Called at the very beginning.
        /// <para>NOTE: Any changes here should be account level, not bucket level.</para>
        /// </summary>
        /// <param name="account"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool StartTransform(Account account, T context)
        {
            return true;
        }
        /// <summary>
        /// Mid Transformation method call - called after Dates have been checked and money columns cleaned.
        /// <para>After this is called, will begin bucket and OOO transformations.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual bool PreFinancialTransform(Account account,  T context)
        {
            return true;
        }
        /// <summary>
        /// End of transformation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="account"></param>
        /// <returns></returns>

        public virtual bool FinishTransform(Account account,  T context)
        {
            return true;

        }

        /// <summary>
        /// Gives the chance to perform any preliminary set up, returns a context object which will be passed to Transform.
        /// <para>Base version returns an instance of the <see cref="BasicContext"/> .</para>
        /// </summary>
        /// <param name="callingContext"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public virtual T Setup(MappingContext callingContext, DemoMapJobConfiguration settings)
        {
            return new T();
        }

        /// <summary>
        /// Method to call after all records have been processed and written to output files.
        /// <para>Return value determines overall success of the job. Base version just returns true.</para>
        /// </summary>
        /// <param name="callingContext"></param>
        /// <param name="context"></param>
        public virtual ResultCode Finish(MappingContext callingContext, T context)
        {
            return MappingContext.DEFAULT_RESULT;
        }
        #endregion

        /// <summary>
        /// ContextJob Process implementation.
        /// </summary>
        /// <param name="context"></param>
        public sealed override void Process(MappingContext context)
        {
            
            if (context.CurrentFilePath == null)
            {
                context.SetStatus(false);
                return;
            }
            string inputFilePath;
            bool keepWorkingFile = true;
            if (context.WorkingFile == null)
            {
                keepWorkingFile = false;
                inputFilePath = context.WorkingFile = context.GetExecutionLocalFile(); //not going to finish/finalize the local file helper, so just get.
            }
            else
                inputFilePath = context.CurrentFilePath;

            //Get from database, include column mapping information - Column order and Name to be used for DocRecord mapping in Transform/Mapping to output file.            
            var settings = GetDemoMapJobConfiguration(context);
            Process(context, settings, inputFilePath, keepWorkingFile);
        }
        // Potential ease of debugging with a second method
        public void Process(MappingContext context, DemoMapJobConfiguration settings, string inputFilePath, bool keepWorkingFile)
        {
            const bool overwriteOutput = true; // Possible todo - configuration setting?

            string nameNoExt = Path.GetFileNameWithoutExtension(inputFilePath);
            if (context.WorkingFile != null)
            {
                nameNoExt = Path.GetFileNameWithoutExtension(context.WorkingFile.OutputFileName);
            }

            if (string.IsNullOrWhiteSpace(settings.OutputFolder))
                context.DefaultOutputDirectory = Path.GetDirectoryName(inputFilePath);
            else
                context.DefaultOutputDirectory = settings.OutputFolder;


            var demoWork = context.ReserveBasicLocalFile("DEMO");
            demoWork.OutputFileName = nameNoExt + "_DEMO.CYM";
            var apbWork = context.ReserveBasicLocalFile("APB");
            apbWork.OutputFileName = nameNoExt + "_APB.CYM";
            var recWork = context.ReserveBasicLocalFile("REC");
            recWork.OutputFileName = nameNoExt + "_REC.CYM";

            DocMetaData metaData = new DocMetaData(inputFilePath)
                .SetEmptyIsNull(true)
                .SetFileAccess(FileAccess.ReadWrite)
                .SetMultiLineEndDelimiters("\r", "\n", "\r\n")
                .SetSkipLines(settings.SkipLines)
                .SetDelimiter(settings.Delimiter);

            metaData.Columns.AllowMissingColumns = true;
            metaData.HasHeader = settings.HasHeaderRow;
            

            DocMetaData DEMO = new DocMetaData(demoWork, "DEMO");
            DocMetaData APB = new DocMetaData(apbWork, "APB");
            DocMetaData REC = new DocMetaData(recWork, "REC");

            System.Diagnostics.Debug.Assert(metaData.Delimiter.HasValue);
            DEMO.SetDelimiter(settings.OutputDelimiter);
            APB.SetDelimiter(settings.OutputDelimiter);
            REC.SetDelimiter(settings.OutputDelimiter);
            if (settings.FilePageSize.HasValue)
            {
                metaData.SetPageSize(settings.FilePageSize.Value);
                DEMO.SetPageSize(settings.FilePageSize.Value);
                APB.SetPageSize(settings.FilePageSize.Value);
                REC.SetPageSize(settings.FilePageSize.Value);
            }
            //Note: using GetManager allows mocking data in Unit Test.
            var stagingConMgr = context.Executor.GetManager(settings.PayerLookupDatabaseID, false);
            using (var helper = stagingConMgr.GetBasicHelper(true))
            {

                DataTable dtLoadTableColumns;
                helper.QualifiedProcedure = "UTIL.usp_MappingOutputFiles_sl";

                helper["Schema"] = "STAGING";
                helper["Table"] = "Account_Load";

                dtLoadTableColumns = context.Manager.Execute(helper).Tables[0];
                dtLoadTableColumns.AsEnumerable().ForEach(dr => DEMO.AddDelimitedColumns(dr[0].ToString()));

                dtLoadTableColumns = null;
                helper["Table"] = "Account_PayerBalance_Load";
                dtLoadTableColumns = context.Manager.Execute(helper).Tables[0];
                dtLoadTableColumns.AsEnumerable().ForEach(dr => APB.AddDelimitedColumns(dr[0].ToString()));

                dtLoadTableColumns = null;
                helper["Table"] = "Reconciliation_Load";
                dtLoadTableColumns = context.Manager.Execute(helper).Tables[0];
                dtLoadTableColumns.AsEnumerable().ForEach(dr => REC.AddDelimitedColumns(dr[0].ToString()));
            }


            var syncObject = Setup(context, settings) ?? new T();


            using (var dr = new DocReader(metaData))
            using (var demo = new DocWriter(DEMO))
            using (var apb = new DocWriter(APB))
            using (var rec = new DocWriter(REC))
            {
                //syncObject.FileRecordCount = dr.RecordCount;
                syncObject.Init(context, dr.RecordCount, settings, context.Executor);

                #region Columns meta data
                //RENAME CLIENT FIELDS AS METRIX FIELDS AND ADD MISSING FIELDS
                foreach (var item in syncObject.MapColumns)
                {
                    if (item.ClientFieldIndex > 0)
                        dr.Columns.RenameColumn((int)item.ClientFieldIndex - 1, item.CymetrixFieldName);
                    else
                        metaData.AddDelimitedColumns(item.CymetrixFieldName);
                }
                if (!metaData.Columns.HasColumn(null, nameof(Account._PatientBalanceUnavailable)))
                {
                    metaData.AddColumn(nameof(Account._PatientBalanceUnavailable));
                }
                if (!metaData.Columns.HasColumn(null, nameof(Account._InsuranceBalanceUnavailable)))
                {
                    metaData.AddColumn(nameof(Account._InsuranceBalanceUnavailable));
                }
                if (!metaData.Columns.HasColumn(null, nameof(Account._InsuranceDetailUnavailable)))
                {
                    metaData.AddColumn(nameof(Account._InsuranceDetailUnavailable));
                }
                if (!metaData.Columns.HasColumn(null, nameof(Account._PartialDemographicLoad)))
                {
                    metaData.AddColumn(nameof(Account._PartialDemographicLoad));
                }
                #endregion

                //adding mapping(hard coded) for the columns in recon which are not present in STAGING.Account_Load table
                DocWriterMap reconMap = new DocWriterMap(rec, dr);
                reconMap
                    .AddMapping(dr.Columns[nameof(Account.LastReconciliationDate)], rec.Columns["ReportingDate"])
                    .AddMapping(dr.Columns["LastServiceDate"], rec.Columns["LastServiceDate"])
                    .AddMapping(dr.Columns["Ins1_PayerCode"], rec.Columns["PrimaryPayerCode"])
                    .AddMapping(dr.Columns[nameof(Account.CurrentPatientBalance)], rec.Columns["PatientBalance"])
                    .AddMapping(dr.Columns["TotalPatientPayments"], rec.Columns["PatientPayments"])
                    .AddMapping(dr.Columns["PatientAdjustments"], rec.Columns["PatientAdjustments"])
                    .AddMapping(dr.Columns[nameof(Account.CurrentInsuranceBalance)], rec.Columns["InsuranceBalance"])
                    .AddMapping(dr.Columns["TotalInsurancePayments"], rec.Columns["InsurancePayments"])
                    .AddMapping(dr.Columns["InsuranceAdjustments"], rec.Columns["InsuranceAdjustments"])
                    .AddMapping(dr.Columns[nameof(Account.CurrentAccountBalance)], rec.Columns["ReportedBalance"])
                    .AddMapping(dr.Columns["FirstBillDate"], rec.Columns["FirstBillDate"])
                    .AddMapping(dr.Columns[nameof(Account.NonBillableBalance)], rec.Columns[nameof(Account.NonBillableBalance)]);


                Action<DocRecord, ParallelLoopState> lineProcess = (line, state) =>
                {
                    //In case a page happens to start in between a /r and /n. Should be the only way this happens pretty much. 
                    // Or else just empty lines in file, which we also do not care about
                    if (line == null)
                        return;
                    line[nameof(Account._PatientBalanceUnavailable)] = settings._PatientBalanceUnavailable ? "1" : "0";
                    line[nameof(Account._InsuranceBalanceUnavailable)] = settings._InsuranceBalanceUnavailable ? "1" : "0";
                    line[nameof(Account._InsuranceDetailUnavailable)] = settings._InsuranceDetailUnavailable ? "1" : "0";
                    line[nameof(Account._PartialDemographicLoad)] = settings._PartialDemographicLoad ? "1" : "0";
                    ProcessRecord(line, settings, context.Executor, syncObject, state, demo, apb, rec, reconMap);
                };

                for (int i = 0; i < dr.PageCount; i++)
                {

                    /*
                     https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallel.foreach?view=netframework-4.7.2


                     https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/data-parallelism-task-parallel-library 
                     */
                    var page = dr.GetPage(i);
#if !DEBUG 
                    ParallelLoopResult pResult = Parallel.ForEach(
                        page,
                        //new ParallelOptions { MaxDegreeOfParallelism = THREAD_COUNT, },
                        lineProcess);

                    if (!pResult.IsCompleted) //Break was signaled.
                        break;
#else
                    //Defined at top of this file: release mode does not need to check user interactive
                    if (Environment.UserInteractive)
                    {
                        foreach (var line in page)
                        {
                            line[nameof(Account._PatientBalanceUnavailable)] = settings._PatientBalanceUnavailable ? "1" : "0";
                            line[nameof(Account._InsuranceBalanceUnavailable)] = settings._InsuranceBalanceUnavailable ? "1" : "0";
                            line[nameof(Account._InsuranceDetailUnavailable)] = settings._InsuranceDetailUnavailable ? "1" : "0";
                            line[nameof(Account._PartialDemographicLoad)] = settings._PartialDemographicLoad ? "1" : "0";
                            ProcessRecord(line, settings, context.Executor, syncObject, null, demo, apb, rec, reconMap);
                            if (!syncObject.Valid)
                                break;
                        }
                        if (!syncObject.Valid)
                            break;
                    }
                    else
                    {
                        ParallelLoopResult pResult = Parallel.ForEach(
                        page,
                        //new ParallelOptions { MaxDegreeOfParallelism = THREAD_COUNT, },
                        lineProcess);

                        if (!pResult.IsCompleted) //Break was signaled.
                            break;
                    }
#endif
                }
            }
            //Note: done with parallel here.
            foreach (var kv in _loggedKeyTransformations)
            {
                string counterTimes = kv.Value > 0 ? "times" : "time";
                context.LogInfo($"Transform: Truncate FacilityKey from '{kv.Key}' to '{kv.Key.Substring(0, 12)}' {kv.Value} {counterTimes}.");
            }
        
            if (syncObject.Valid)
            {
                context.SetStatus(Finish(context, syncObject));
                bool doRegister = true;
                if (context.Failure)
                {
                    if (syncObject.KeepFilesOnFailingFinish)
                    {
                        doRegister = false; //Don't create child job executions if we have an error status.
                        context.LogInfo("Execution has failed, but moving files out from temp working space anyway.");
                    }
                    else
                        return;//Don't finish the files if the final result is a failure.
                }

                if (syncObject.SkippedAccounts > 0)
                {
                    context.LogInfo($"Skipped writing {syncObject.SkippedAccounts} record(s) to output files.");
                }

                syncObject.DoCleanup();
                var ci = GetNewChildInfo(context, true);

                demoWork.Finish(overwriteOutput);
                ci.FilePath = demoWork;
                ci.Branch = LOAD_TYPE_A;
                if(doRegister)
                    RegisterChildExecution(ci, context);

                recWork.Finish(overwriteOutput);
                ci.FilePath = recWork;
                ci.Branch = LOAD_TYPE_REC;

                if (doRegister)
                    RegisterChildExecution(ci, context);                    

                if (settings.DoAPB)
                {
                    apbWork.Finish(overwriteOutput);
                    ci.FilePath = apbWork;
                    ci.Branch = LOAD_TYPE_APB;

                    if (doRegister)
                        RegisterChildExecution(ci, context);
                }
                if (!keepWorkingFile)
                    context.WorkingFile = null;
                return;
            }
            context.SetStatus(ResultCode.VF);
        }

        private const string LOAD_TYPE_A = "A";
        private const string LOAD_TYPE_REC = "REC";
        private const string LOAD_TYPE_APB = "APB";
        void ProcessRecord(DocRecord record, DemoMapJobConfiguration settings, IJobExecutor service,
            T sync, ParallelLoopState state,
            DocWriter A, DocWriter APB, DocWriter REC, DocWriterMap ReconMap)
        {
            Account acct = new Account(record, sync);
            //Transform should perform Special handling for the record. E.g., format dates to yyyy-MM-dd, OOO buckets, balance validations, checking IsSelfPay, etc.
            //Sync object is in case there's anything that needs to be tracked at the class level, e.g. counters or additional, file-specific, settings. Or if there's any end validations of the whole file to be done at the very end. E.g., maybe there was a property that was a red flag, so we want to send an alert and have someone look at the files before it can continue moving on to Metrix. (Lots of records with Ins1 PayerCode = MissingPayerCode, for example)
            if (!Transform(record, sync, acct) && sync.Valid)
            {
                
                sync.IncrementSkippedCount();
                return;
            }

            if (!sync.Valid)
            {
                /*
                 * https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallelloopstate.break?view=netframework-4.7.2#System_Threading_Tasks_ParallelLoopState_Break
                 * 
                 * https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallelloopstate.stop?view=netframework-4.7.2
                 */
#if DEBUG
                //This is just for user interactive mode - otherwise, will not be null.
                state?.Stop();
#else
                state.Stop();
#endif
                return;
            }

            acct.Sync();
            if (!ValidateRecord(record, acct, sync))
            {
                //sync.FailValidation($"Account # {acct.AccountNumber} failed call to ValidateRecord."); //moved inside method
#if DEBUG
                state?.Stop();
#else
                state.Stop();
#endif
                return;
            }
            //acct.Refresh();

            lock (A) //Note for locking A - object is from calling method rather than public, and is reference not being changed.
                A.AddRecord(record);

            lock (REC)
                REC.AddRecord(record, ReconMap);

            if (settings.DoAPB)
            {
                List<DocRecord> apbData = new List<DocRecord>();

                /*
                 Technically this modifies the account record and could influence writing to the other files, 
                 but we're doing this after we've already put this account into the other files. 
                 
                So we should be okay                 
                 */
                decimal bucketPat = acct.GetBucketSelfPayBalances() ?? 0;
                decimal remainder = acct.CurrentPatientBalance - bucketPat;
                if (remainder != 0)
                {
                    if (acct.BucketCount == Bucket.MAX_SEQUENCE_NUMBER)
                    {
                        sync.FailValidation($"Account # '{acct.AccountNumber}' exceeds max sequence number on adding self pay bucket for APB");
                    }
                    // Only add *SELFPAY* bucket if balances don't add up to the CurrentPatientBalance - Remainder.
                    // This does not need to go into the A or REC files
                    // because Metrix doesn't care about having ARBuckets add up to the Patient Balance.
                    var b = acct.AddBucket(Bucket.SELFPAY_PAYER_CODE, remainder, true);
                    b.Apply();
                }
                
                //Note: Buckets should already be up to date in the account object due to protections for setters.
                foreach (var bucket in acct.Buckets) 
                {
                    try
                    {
                        apbData.Add(new DocRecord(APB.Columns, true)
                        {
                            [nameof(Bucket.PayerCode)] = bucket.PayerCode,
                            [FACILITY_CODE] = acct.FacilityKey,
                            [nameof(acct.AccountNumber)] = acct.AccountNumber,
                            [LAST_RECON_DATE] = sync.ReconciliationDate,
                            [nameof(acct.VendorCode)] = acct.VendorCode,
                            [nameof(BillingStatusCode)] = acct.BillingStatus.ToString(),
                            [nameof(Bucket.PrincipalBalance)] = bucket.PrincipalBalance,
                            [nameof(Bucket.SequenceNumber)] = bucket.SequenceNumber.ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        sync.FailValidation($"Unable to add APB Bucket for Account # '{acct.AccountNumber}, " +
                                            $"Sequence {bucket?.SequenceNumber.ToString() ?? "UNKNOWN"}", ex);
                    }

                }
                
                lock (APB)
                {
                    foreach (var payerData in apbData)
                        APB.AddRecord(payerData);
                }
            }
        }
    
#region Private Validation and processing

        /// <summary>
        /// Basic Transformations + OOO, if enabled. If the record is determined to be bad (or a duplicate) and needs to be filtered, then the override should return false.
        /// </summary>
        /// <param name="fileLine"></param>
        /// <param name="context">For any logic that spans across multiple records. Should be originally returned by <see cref="Setup(MappingContext, DemoMapJobConfiguration)"/> </param>
        /// <param name="account"></param>
        /// <returns>True</returns>
        public bool Transform( DocRecord fileLine, T context, Account account)
        {
            const string APPEND_FORMAT = " - Success\r\n{0}";
            StringBuilder methodCall = new StringBuilder($"Account# {account.AccountNumber}{Environment.NewLine}").Append(nameof(StartTransform));
            try
            {
                if (!StartTransform(account, context))
                    return false;
                if (account.Modified)
                {
                    methodCall.AppendFormat(APPEND_FORMAT, nameof(account.Sync));
                    account.Sync();
                }

                methodCall.AppendFormat(APPEND_FORMAT, "FacilityKey Trim");
                //Correct FacilityKey right away.
                string key = fileLine[FACILITY_CODE];
                if (string.IsNullOrWhiteSpace(key))
                    fileLine[FACILITY_CODE] = null;
                else if (key.Length > Account.FACILITY_KEY_MAX_LENGTH)
                {
                    string newKey = fileLine[FACILITY_CODE].Substring(0, Account.FACILITY_KEY_MAX_LENGTH).Trim();
                    lock (context.KeyLogLock)
                    {
                        if (!_loggedKeyTransformations.ContainsKey(key))
                        {
                            _loggedKeyTransformations.Add(key, 1);
                        }
                        else
                        {
                            _loggedKeyTransformations[key]++;
                        }
                    }

                    fileLine[FACILITY_CODE] = newKey;
                }

                methodCall.AppendFormat(APPEND_FORMAT, nameof(CleanMoneyFields));
                CleanMoneyFields(fileLine);

                methodCall.AppendFormat(APPEND_FORMAT, nameof(CheckDates));
                CheckDates(fileLine, context);


                methodCall.AppendFormat(APPEND_FORMAT, nameof(account.Refresh));
                account.Refresh(); //Because date/money cleanup could affect cached properties on the account.

                methodCall.AppendFormat(APPEND_FORMAT, nameof(PreFinancialTransform));
                if (!PreFinancialTransform(account, context))
                    return false;

                methodCall.AppendFormat(APPEND_FORMAT, nameof(account.PrepBuckets));
                account.PrepBuckets(); // Handle checking the balance/payer code of each bucket sequence.
                // Cache the information to be available for ease of calculations.
                methodCall.AppendFormat(APPEND_FORMAT, nameof(ValidateFinancialTotals));
                ValidateFinancialTotals(account, context);
                if (context.Settings.Enable_OOO)
                {
                    methodCall.AppendFormat(APPEND_FORMAT, nameof(CheckOOO));
                    CheckOOO(context.Settings, account); //Check number of buckets and sum of balances. Create a new bucket if needed.
                }
                methodCall.AppendFormat(APPEND_FORMAT, nameof(FinishTransform));
                bool result = FinishTransform(account, context);

                methodCall.AppendFormat(APPEND_FORMAT, nameof(account.Sync));
                account.Sync();
                return result;
            }
            catch(Exception ex)
            {
                methodCall.Append(" - FAILURE").Append(ContextObjectBase.AssemblyVersion);
                context.FailValidation(methodCall.ToString(), ex);
                return false;
            }

        }

        private readonly Dictionary<string, int> _loggedKeyTransformations = new Dictionary<string, int>();
        private bool ValidateRecord(DocRecord record, Account acct, T sync)
        {
            string field = "BillingStatus";
            //Use acct to take advantage of caching some of the variables.

            //var billingStatus = record.GetBillingStatus();
            if (acct.BillingStatus == BillingStatusCode.UNKNOWN)
            {
                LogError(acct, "Invalid Billing Status Code");
            sync.FailValidation();
                return false;
            }

            field = "FirstBillDate";
            if (string.IsNullOrEmpty(record[field]))
                record[field] = acct.OriginalBillDate.Format();

            field = "NonBillableBalance";
            if (string.IsNullOrEmpty(record[field]))
                record[field] = "0.00";

            //acct.Refresh();
            return true;
        }

        private void CleanMoneyFields(DocRecord record)
        {

            var moneyCleanupColumns = record.GetColumns(c =>
            {
                //Ignore columns marked by map utility as not used/ignore
                if (c.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (c.ColumnName.In(Account.SETTINGS_COLUMNS))
                    return false;
                return c.ColumnName.IndexOf("Charges", StringComparison.OrdinalIgnoreCase) > 0
                       || c.ColumnName.IndexOf("Payments", StringComparison.OrdinalIgnoreCase) > 0
                       || c.ColumnName.IndexOf("Balance", StringComparison.OrdinalIgnoreCase) > 0
                       || c.ColumnName.IndexOf("Adjustments", StringComparison.OrdinalIgnoreCase) > 0
                       || c.ColumnName.IndexOf("EstimatedAmountDue", StringComparison.OrdinalIgnoreCase) > 0;
            });

            foreach (var item in moneyCleanupColumns)
            {
                string moneyField = record[item];
                if (string.IsNullOrEmpty(moneyField))
                {
                    record[item] = "0.00";
                    continue;
                }

                //If this causes an exception, caller will log
                decimal d = decimal.Parse(moneyField
                                          .Replace("$", "")
                                          .Replace("\"", ""));
                record[item] = d.FormatMoney();
            }
        }
        private void CheckDates(DocRecord record, ContextObjectBase context)
        {
            var lstCheckDatesColumn = record.GetColumns(col =>
            {
                if (col.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (col.ColumnName.In(Account.SETTINGS_COLUMNS))
                    return false;
                if (col.ColumnName.Equals(LAST_RECON_DATE, StringComparison.OrdinalIgnoreCase))
                    return false;
                return col.ColumnName.IndexOf("DATE", StringComparison.OrdinalIgnoreCase) >= 0;
            });
            record[LAST_RECON_DATE] = context.ReconciliationDate;
            foreach (var item in lstCheckDatesColumn)
            {
            	record[item] = record.GetDateTime(item).Format();
            }
        }
        

        public const string FACILITY_CODE = nameof(Account.FacilityKey);
        public const string LAST_RECON_DATE = nameof(Account.LastReconciliationDate);

        private void CheckOOO(DemoMapJobConfiguration settings, Account account)
        {
            if (!settings.Enable_OOO)
                return;
            if (settings._InsuranceBalanceUnavailable || settings._InsuranceDetailUnavailable)
                throw new InvalidOperationException("Cannot do OOO Transformation without CurrentInsuranceBalance and Insurance Detail.");

            decimal expectedInsBalance = account.CurrentAccountBalance - account.CurrentPatientBalance - account.NonBillableBalance;
            if (settings.OOO_InsuranceBalanceValidation && account.CurrentInsuranceBalance != expectedInsBalance)
            {
                LogInfo(account, $"Modifying Insurance Balance from {account.CurrentInsuranceBalance} to {expectedInsBalance} due to mismatch.");
                account.CurrentInsuranceBalance = expectedInsBalance;
            }
            decimal buckets = account.TotalInsuranceBucketBalance;
            decimal OOO_Balance = account.CurrentInsuranceBalance - buckets;
            if (OOO_Balance != 0)
            {
                account.AddBucket(Bucket.OOO_BUCKET_PAYER_CODE, OOO_Balance, false);
                account.SyncBuckets();
            }
        }
#endregion
        public static DemoMapJobConfiguration GetDemoMapJobConfiguration(MappingContext context)
        {
            const string GET_EXECUTION_INFO = "SEIDR.usp_DemoMapJob_ss";
            var dm = context.Manager;
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(context.JobProfile_JobID)] = context.JobProfile_JobID;
                return dm.SelectSingle<DemoMapJobConfiguration>(helper, true, false);
            }
        }


        public void LogInfo(Account a, string Message)
        {
            a.Context.LogInfo($"AccountNumber: '{a.AccountNumber}', FacilityKey '{a.FacilityKey}': " + Message + ContextObjectBase.AssemblyVersion);
        }
        /// <summary>
        /// Log an error while processing an account.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="ExtraID"></param>
        public void LogError(Account a, string message, Exception ex = null, int? ExtraID = null)
        {
            message = $"AccountNumber: '{a.AccountNumber}', FacilityKey '{a.FacilityKey}': " + message + ContextObjectBase.AssemblyVersion;
            a.Context.LogError(message, ex, ExtraID);
        }
    }
}
