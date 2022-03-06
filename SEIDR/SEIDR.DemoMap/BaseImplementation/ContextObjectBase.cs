using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using SEIDR.DataBase;

namespace SEIDR.DemoMap.BaseImplementation
{
    public class BasicContext : ContextObjectBase { }
    /// <summary>
    /// Base class for context sync in DMAP
    /// </summary>
    public abstract class ContextObjectBase
    {
        private static string _displayVersion;
        public static string AssemblyVersion
        {
            get
            {
                if (_displayVersion == null)
                {
                    var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    var build = new DateTime(2000, 1, 1)
                                .AddDays(v.Build)
                                .AddSeconds(v.Revision * 2);
                    _displayVersion = $"{Environment.NewLine}Version {v} ({build})";
                }

                return _displayVersion;
            }
        }
        public readonly object KeyLogLock = new object();
        private readonly object _skipLock = new object();
        private int _skipped = 0;
        public int SkippedAccounts
        {
            get
            {
                lock (_skipLock)
                    return _skipped;
            }
        }

        public int IncrementSkippedCount()
        {
            lock (_skipLock)
            {
                return ++_skipped;
            }
        }

        /// <summary>
        /// Applies when the Implementation of <see cref="DemoMapJob{T}.Finish(MappingContext, T)"/> returns a failure status.
        /// <para>If this is true, then the files will still be finalized.</para>
        /// <para>Otherwise, the files will be cleaned up as temporary files. (Default Behavior)</para>
        /// </summary>
        public virtual bool KeepFilesOnFailingFinish { get; } = false;

        #region Job Initialization
        public virtual void Init(MappingContext context, long RecordCount, DemoMapJobConfiguration settings, 
            JobBase.IJobExecutor executor)
        {
            ProcessingDate = context;
            _Executor = executor;
            Settings = settings;
            ReconciliationDate = ProcessingDate.AddDays(-1).ToString(METRIX_DATE_FORMAT);
            FileRecordCount = RecordCount;

            SelfPayCodes.Add(Bucket.SELFPAY_PAYER_CODE);

            var dataServiceConMgr = context.Executor.GetManager(settings.FileMapDatabaseID);

            MapColumns = MAPS_DELIMITED.GetMAPS_DELIMITED_Columns(dataServiceConMgr, settings.FileMapID);

            DatabaseManager payerDB = context.Executor.GetManager(settings.PayerLookupDatabaseID);
            PayerInfo = payerDB.SelectList<PayerMaster_MapInfo>(new { context.Execution.OrganizationID }, Schema: "STAGING");

        }
        public virtual void DoCleanup() { }
        #endregion

        public DemoMapJobConfiguration Settings { get; private set; }
        private JobBase.IJobExecutor _Executor;

        public void LogInfo(string message)
        {
            lock(SyncObject)
                _Executor.LogInfo(message);
        }

        public void LogError(string message, Exception ex = null,
            int? ExtraID = null)
        {
            lock(SyncObject)
                _Executor.LogError(message, ex, ExtraID);
        }


        /// <summary>
        /// Date Format to use for Date columns in Metrix data loading
        /// </summary>
        public const string METRIX_DATE_FORMAT = "MM/dd/yyyy";
        /// <summary>
        /// ProcessingDate - 1, formatted in <see cref="METRIX_DATE_FORMAT"/> 
        /// </summary>
        public string ReconciliationDate { get; private set; }
        /// <summary>
        /// Number of records in the original file.
        /// </summary>
        public long FileRecordCount { get; private set; }
        /// <summary>
        /// Object for locking purposes
        /// </summary>
        public object SyncObject { get; } = new object();
        bool _valid = true;
        /// <summary>
        /// 
        /// </summary>
        public bool Valid
        {
            get
            {
                lock (SyncObject)
                    return _valid;
            }
        }
        public void FailValidation(string message = null, Exception ex = null)
        {
            lock (SyncObject)
            {
                _valid = false;
                if(!string.IsNullOrEmpty(message) || ex != null)
                    _Executor.LogError(message, ex);
            }
        }

        public DateTime ProcessingDate { get; private set; }

        public PayerMaster_MapInfo this[string FacilityCode, string PayerCode] => GetPayerInfo( FacilityCode, PayerCode);

        /// <summary>
        /// Attempts to add the payer map info, if it does not already exist.
        /// <para>Return true if the new payer is added, else false.</para>
        /// </summary>
        /// <param name="newPayer"></param>
        /// <returns></returns>
        public bool AddPayerInfo(PayerMaster_MapInfo newPayer)
        {
            lock (_payerLock)
            {
                if (!PayerInfo.Exists(p => p.PayerCode == newPayer.PayerCode && p.FacilityCode == newPayer.FacilityCode))
                {
                    PayerInfo.Add(newPayer);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Populate this during context object initialization - used for setting default IsSelfPay values for new PayerCode/FacilityKey combinations
        /// </summary>
        /// <param name="PayCodeList"></param>
        public void AddSelfPayCodes(params string[] PayCodeList)
        {
            var q = from payer in PayCodeList
                    where payer.NotIn(Bucket.BAD_BUCKET_PAYER_CODE, Bucket.OOO_BUCKET_PAYER_CODE)
                        && SelfPayCodes.NotExists(sp => sp == payer)
                    select payer;

            SelfPayCodes.AddRange(q);
        }
        List<string> SelfPayCodes { get; } = new List<string>();

        /// <summary>
        /// Gets the Payer Information based on a database lookup.
        /// <para>If the payer code is not found for this facility key, will default to false unless the payer code is used as self pay for another facility in the organization.</para>
        /// </summary>
        /// <param name="FacilityCode"></param>
        /// <param name="PayerCode"></param>
        /// <param name="DefaultSelfPay">If the payer does not exist, use as the SelfPay value for going forward.</param>
        /// <returns></returns>
        public PayerMaster_MapInfo GetPayerInfo(string FacilityCode, string PayerCode, bool? DefaultSelfPay = null)
        {
            if (string.IsNullOrWhiteSpace(PayerCode))
                return null;
            PayerMaster_MapInfo rPayer;
            lock (_payerLock)
                rPayer = PayerInfo.FirstOrDefault(payer => payer.FacilityCode == FacilityCode && payer.PayerCode == PayerCode);
            if (rPayer == null)
            {
                bool selfPay;
                if (SelfPayCodes.Contains(PayerCode))
                {
                    System.Diagnostics.Debug.WriteLine($"Force PayerCode {PayerCode} under Facility {FacilityCode} to SelfPay = true. (Default value passed was '{DefaultSelfPay}')");
                    selfPay = true;
                }
                else if (DefaultSelfPay.HasValue)
                {
                    selfPay = DefaultSelfPay.Value;
                }
                else
                {
                    /*
                     
                    If any other facility has this payer as self pay, assume self pay.
                    Something to keep in mind: 
                        facility master table logic can have a "lead" facility set, which can drive the conforming process.
                    */
                lock(_payerLock)
                    selfPay = PayerInfo.Exists(payer => payer.PayerCode == PayerCode && payer.IsSelfPay);
                    System.Diagnostics.Debug.WriteLine($"Force PayerCode {PayerCode} under Facility {FacilityCode} to SelfPay = {selfPay}.");
                }

                rPayer = new PayerMaster_MapInfo(PayerCode, FacilityCode, selfPay);
            lock (_payerLock)
                PayerInfo.Add(rPayer);
            }
            return rPayer;
        }
        private readonly object _payerLock = new object();
        /// <summary>
        /// Checks if the payer information is cached in the context.
        /// </summary>
        /// <param name="FacilityCode"></param>
        /// <param name="PayerCode"></param>
        /// <param name="Payer"></param>
        /// <returns></returns>
        public bool CheckPayerExist(string FacilityCode, string PayerCode, out PayerMaster_MapInfo Payer)
        {
            if (PayerCode == null)
            {
                Payer = null;
                return false;
            }

            lock (_payerLock)
            {
                Payer = PayerInfo.FirstOrDefault(payer => payer.FacilityCode == FacilityCode && payer.PayerCode == PayerCode);
            }
            return Payer != null;
        }
        /// <summary>
        /// Only modify this during initialization.
        /// </summary>
        private List<PayerMaster_MapInfo> PayerInfo { get; set; } = new List<PayerMaster_MapInfo>();
        //public IReadOnlyDictionary<string, bool> PayerInfo { get; private set; }
        public List<MAPS_DELIMITED> MapColumns { get; private set; }

        
        
    }
}
