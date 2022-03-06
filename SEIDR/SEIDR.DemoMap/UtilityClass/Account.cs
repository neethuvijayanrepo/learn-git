using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;
// ReSharper disable InconsistentNaming

namespace SEIDR.DemoMap
{
    public class Account
    {
        public static readonly string[] SETTINGS_COLUMNS = 
        {
            nameof(_PatientBalanceUnavailable),
            nameof(_InsuranceBalanceUnavailable),
            nameof(_InsuranceDetailUnavailable),
            nameof(_PartialDemographicLoad)
        };

        private static readonly string[] PROP_COLUMNS =
        {
            nameof(LastReconciliationDate),
            nameof(AccountNumber),
            nameof(FacilityKey),
            nameof(VendorCode),
            nameof(Inpatient),
            nameof(OriginalBillDate),
            nameof(NonBillableBalance),
            nameof(CurrentInsuranceBalance),
            nameof(CurrentPatientBalance),
            nameof(CurrentAccountBalance)
        };
        public static implicit operator Doc.DocRecord(Account a) => a._source;
        #region indexers/ Docrecord direct sets.
        public Bucket this[int sequence]
        {
            get
            {
                if (sequence < 1 || sequence > Bucket.MAX_SEQUENCE_NUMBER)
                    throw new ArgumentOutOfRangeException("Sequence must be between 1 and " + Bucket.MAX_SEQUENCE_NUMBER, 
                                                          nameof(sequence));
                if (_buckets == null)
                    PrepBuckets();
                return _buckets[sequence];
            }
        }

        public string this[Doc.DocRecordColumnInfo column]
        {
            get { return _source[column]; }
            set
            {
#if DEBUG
                if(column.ColumnName.Like("Ins[1-8]\\_%", false))
                    throw new InvalidOperationException("Cannot modify insurance from Account object.");
                if (column.ColumnName.In(SETTINGS_COLUMNS))
                    throw new InvalidOperationException("Cannot modify setting columns");
                if (column.ColumnName.In(PROP_COLUMNS))
                {
                    throw new InvalidOperationException("Cannot modify this column directly. Use a property.");
                }
#endif
                _source[column] = value;
            }
        }
        public string this[string columnName]
        {
            get { return _source[columnName]; }
            set
            {
#if DEBUG
                if (columnName.Like("Ins[1-8]\\_%", false))
                    throw new InvalidOperationException("Cannot modify insurance from Account object.");
                if (columnName.In(SETTINGS_COLUMNS))
                    throw new InvalidOperationException("Cannot modify setting columns");
                if (columnName.In(PROP_COLUMNS))
                {
                    throw new InvalidOperationException("Cannot modify this column directly. Use a property.");
                }
#endif
                _source[columnName] = value;
            }
        }

        public void SetBool(string columnName, bool? value)
        {
            _source[columnName] = value.Format();
        }
        /// <summary>
        /// Checks if a bool field can be parsed, and if so puts it in a format for SQL
        /// <para>True/Yes/Y/1 => 1, False/No/N => 0, other => ""</para>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="includeYesNo"></param>
        public void CheckBoolValue(string columnName, bool includeYesNo = true)
        {
            _source[columnName] = _source[columnName].CheckSQLBool(includeYesNo).Format();
        }

        public void CheckBoolValueNoNull(string columnName, bool defaultForNull, bool includeYesNo = true)
        {
            _source[columnName] = (_source[columnName].CheckSQLBool(includeYesNo) ?? defaultForNull).Format();
        }
        public bool? GetBool(string columnName) => _source.GetBool(columnName);
        /// <summary>
        /// Checks if a DateTime field can be parsed, and if so puts it in the format specified by <see cref="ContextObjectBase.METRIX_DATE_FORMAT"/> .
        /// </summary>
        /// <param name="columnName"></param>
        public void CheckDateTime(string columnName)
        {
            _source[columnName] = _source.GetDateTime(columnName).Format();
        }
        public void SetDateTime(string columnName, DateTime? value)
        {
            _source[columnName] = value.Format();
        }

        public DateTime? GetDateTime(string columnName) => _source.GetDateTime(columnName);
        public decimal? GetMoney(string column) => _source.GetDecimal(column);
        public void SetDecimal(string columnName, decimal? value) => _source[columnName] = value.ToString();
        

        #endregion

        #region other helpers

        private NameHelper _PatientName;

        public NameHelper PatientName
        {
            get
            {
                if (_PatientName == null)
                    _PatientName = new NameHelper(_source, false);
                return _PatientName;
            }
        }
        private NameHelper _GuarantorName;
        public NameHelper GuarantorName
        {
            get
            {
                if (_GuarantorName == null)
                    _GuarantorName = new NameHelper(_source);
                return _GuarantorName;
            }
        }

        public void FormatNames(NameHelperUpdateMode updateMode = NameHelperUpdateMode.Default)
        {
            if(!PatientName.IsEmpty)
                PatientName.SetValues(updateMode);
            if(!GuarantorName.IsEmpty)
                GuarantorName.SetValues(updateMode);
        }

        public void FormatPatientName(NameHelperUpdateMode updateMode = NameHelperUpdateMode.Default)
        {
            if (PatientName.IsEmpty)
                return;
            PatientName.SetValues(updateMode);
        }

        public void FormatGuarantorName(NameHelperUpdateMode updateMode = NameHelperUpdateMode.Default)
        {
            if (GuarantorName.IsEmpty)
                return;
            GuarantorName.SetValues(updateMode);
        } 
        public void FormatName(bool Guarantor = true,
            NameHelperUpdateMode updateMode = NameHelperUpdateMode.Default)
        {
            if (Guarantor)
                FormatGuarantorName(updateMode);
            else
                FormatPatientName(updateMode);
        }

        public NameHelper GetPatientName()
        {
            return new NameHelper(_source, false);
        }

        public NameHelper GetGuarantorName()
        {
            return new NameHelper(_source, true);
        }
        #endregion
        private readonly Doc.DocRecord _source;

        public readonly ContextObjectBase Context;
        private string _AccountNumber;
        public string AccountNumber
        {
            get
            {
                return _AccountNumber;
            }
            set
            {
#if DEBUG
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException("Cannot set Account Number to null/empty.");
#endif
                if (_AccountNumber.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;
                _AccountNumber = value;
                _changeState |= PropertyChangeState.ACCOUNT_NUMBER;
            }
        }

        private string _FacilityKey;
        public const int FACILITY_KEY_MAX_LENGTH = 12;
        public string FacilityKey
        {
            get { return _FacilityKey; }
            set
            {
                if (_FacilityKey.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;
#if DEBUG
                if (value != null && value.Length > FACILITY_KEY_MAX_LENGTH)
                {
                    string Message = $"Cannot Set FacilityKey to '{value}' - length of {value.Length} exceeds maximum of {FACILITY_KEY_MAX_LENGTH}.";
                    throw new InvalidOperationException(Message);
                }
#endif
                _FacilityKey = value ?? string.Empty;
                _changeState |= PropertyChangeState.FACILITY_CODE;
            }
        }
        public string LastReconciliationDate => Context.ReconciliationDate;

        /// <summary>
        /// Returns column information. Columns used for settings or properties on the Account object are automatically excluded.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="mappedOnly">If true, ignores columns marked by the Map Utility as not to be used (CymetrixFieldName prepended with _N_)</param>
        /// <returns></returns>
        public IEnumerable<Doc.DocRecordColumnInfo> GetColumns(Predicate<Doc.DocRecordColumnInfo> test, bool mappedOnly = true)
        {
            return from col in _source.GetColumns(test)
                   where col.ColumnName.NotIn(PROP_COLUMNS)
                       && col.ColumnName.NotIn(SETTINGS_COLUMNS)
                       && 
                       (
                           mappedOnly == false 
                           || !col.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase)
                        )
                   //Columns marked by map utility as not in use...
                   select col;
        }

        public IEnumerable<Doc.DocRecordColumnInfo> GetColumnsContaining(string contained, bool mappedOnly = true)
        {
            return _source.GetColumns(c =>
            {
                if (PROP_COLUMNS.Contains(c.ColumnName))
                    return false;
                if (SETTINGS_COLUMNS.Contains(c.ColumnName))
                    return false;
                if (mappedOnly && c.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase))
                    return false;
                return c.ColumnName.IndexOf(contained, StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

#region Buckets
        private Bucket[] _buckets = null;

        /// <summary>
        /// Valid buckets attached to the account.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Bucket> Buckets
        {
            get
            {
                PrepBuckets();
                return from b in _buckets
                       where b != null
                       select b;
            }
        }

        public bool RoomForMoreBuckets => MaxSequence < Bucket.MAX_SEQUENCE_NUMBER;
        private int _BucketCount = 0;

        public int BucketCount
        {
            get
            {
                if (_buckets == null)
                    RefreshBucketInfo();
                return _BucketCount;
            }
        }

        /// <summary>
        /// Adds a bucket to the next sequence without any details. Then returns the bucket.
        /// </summary>
        /// <param name="PayerCode"></param>
        /// <param name="Balance"></param>
        /// <param name="defaultSelfPay">Default value for IsSelfPay, if the payer code is new.</param>
        public Bucket AddBucket(string PayerCode, decimal Balance, bool defaultSelfPay = false)
        {
            if (_buckets == null)
                PrepBuckets();
            int newSequence = _MaxSequence + 1;
            if (_buckets[newSequence] != null)
            {
                throw new InvalidOperationException("Inconsistent Sequence state, likely due to shifting. Bucket already exists in Sequence " + newSequence + " - Cannot Add.");
            }
#if DEBUG
            try
#endif
            {

	            Bucket b = new Bucket(_source, newSequence, this);
	            b.SetPayerInfo(PayerCode, defaultSelfPay);
	            b.Balance = Balance;
	            _buckets[newSequence] = b;
	            _MaxSequence = newSequence;
	            _BucketCount++;
	            return b;
	        }
#if DEBUG
            catch (Exception ex)
            {
                Context.LogError($"Unable to add new bucket at sequence {newSequence} for account {AccountNumber}, payerCode '{PayerCode}', balance {Balance}", ex);
                throw;
            }
#endif

        }
        /// <summary>
        /// If Buckets has not been called yet, sets up the Buckets List. (Lazy Initialization)
        /// </summary>
        public void PrepBuckets()
        {
            if(_buckets == null)
                RefreshBucketInfo();
        }

        private int _MaxSequence = -1;

        public int MaxSequence
        {
            get
            {
                if (_MaxSequence < 0)
                    RefreshBucketInfo();
                return _MaxSequence;
            }
        }

        public bool HasBucketsPrepped => _buckets != null;
        /// <summary>
        /// Reload the BucketSummary from the DocRecord.
        /// </summary>
        /// <returns></returns>
        public Bucket[] RefreshBucketInfo()
        {
            //_buckets = new Bucket[1..8];
            /*]
             //Will give an error about object type Bucket[*] cannot be cast as type Bucket[]
            Array b = (Bucket[]) Array.CreateInstance(typeof(Bucket), 
                                                       new [] { Bucket.MAX_SEQUENCE_NUMBER}, 
                                                       new [] {1});*/
            _buckets = new Bucket[Bucket.MAX_SEQUENCE_NUMBER + 1]; // + 1 because we're going to ignore index 0
            _BucketCount = 0;
            for (int seq = 1; seq <= Bucket.MAX_SEQUENCE_NUMBER; seq++)
            {
                var bucket = new Bucket(_source, seq, this); //Move bad bucket logic into this class.
                if (bucket.Valid)
                {
                    _buckets[seq] = bucket;
                    _BucketCount++;
                    //_buckets.Add(bucket);
                }
                else
                    bucket.ClearSource();
            }

            if (_BucketCount == 0)
                _MaxSequence = 0;
            else
                _MaxSequence = _buckets.Max(b => b?.SequenceNumber) ?? 0;
            return _buckets;
        }

        public void CheckBucketSequences()
        {
            if (_buckets == null)
                return;
            for (int i = 1; i <= Bucket.MAX_SEQUENCE_NUMBER; i++)
            {
                var b = _buckets[i-1];
                if (b == null)
                    continue;
                while (b.SequenceNumber != i)
                {
                    var b2 = _buckets[b.SequenceNumber-1];
                    if (b2 == null)
                    {
                        _buckets[b.SequenceNumber-1] = b;
                        _buckets[i-1] = null;
                        break;
                    }
                    if (b2.SequenceNumber == b.SequenceNumber)
                        throw new Exception("Duplicate Sequence Number - " + b.SequenceNumber);
                    _buckets[b.SequenceNumber-1] = b;
                    b = b2;
                    if (b.SequenceNumber == i)
                        _buckets[i-1] = b;
                }
            }

            _MaxSequence = _buckets.Max(b => b?.SequenceNumber) ?? 0;
        }

        public void SyncBuckets()
        {
            if (_buckets == null)
                return;
            //Note: we want to clear fields from missing buckets, so include sequences after MaxSequence
            for (int seq = 1; seq <= Bucket.MAX_SEQUENCE_NUMBER; seq++)
            {
                var b = this[seq];
                if (b == null)
                {
                    Bucket.ClearBucketSequence(_source, seq);
                }
                else if(b.Modified)
                {
                    b.Apply();
                }
            }
        }
        public void RemoveBucket(Bucket b)
        {
            if(_buckets[b.SequenceNumber] != b)
                throw new  InvalidOperationException("Bucket Not Associated with Account's Bucket List.");
            _buckets[b.SequenceNumber] = null;
            if (b.SequenceNumber == _MaxSequence)
                _MaxSequence = _buckets.Max(ib => ib?.SequenceNumber) ?? 0;
            b.ClearSource();
            b.UnlinkAccount();
            _BucketCount--;
        }
#endregion

        
        public Account(Doc.DocRecord source, ContextObjectBase context)
        {
            _source = source;
            Context = context;

            _PatientBalanceUnavailable = source.GetBool(nameof(_PatientBalanceUnavailable)) ?? false;
            _InsuranceBalanceUnavailable = source.GetBool(nameof(_InsuranceBalanceUnavailable)) ?? false;
            _InsuranceDetailUnavailable = source.GetBool(nameof(_InsuranceDetailUnavailable)) ?? false;
            _PartialDemographicLoad = source.GetBool(nameof(_PartialDemographicLoad)) ?? false;
            Refresh(); //Data pull from _source
        }


        public decimal? GetBucketInsuranceBalances()
        {
            if (_InsuranceDetailUnavailable)
                return null;
            PrepBuckets();
            decimal balance = 0;
            foreach (var b in Buckets)
            {
                if(!b.IsSelfPay)
                    balance += b.Balance;
            }
            return balance;
        
            //return _source.SumInsuranceBalances();
        }
        /// <summary>
        /// Sum of balances associated with buckets having SelfPay = false, or 0 if no such bucket details are available.
        /// </summary>
        public decimal TotalInsuranceBucketBalance => GetBucketInsuranceBalances() ?? 0;
        public decimal? GetBucketSelfPayBalances()
        {
            if (_InsuranceDetailUnavailable)
                return null;
            decimal balance = 0;
            PrepBuckets();
            foreach (var b in Buckets)
            {
                if (b.IsSelfPay)
                    balance += b.Balance;
            }
            return balance;
            //return _source.SumPatientBalance();
        }
        /// <summary>
        /// Sum of balances associated with buckets having SelfPay = true, or 0 if no such bucket details are available.
        /// </summary>
        public decimal TotalSelfPayBucketBalance => GetBucketSelfPayBalances() ?? 0;
#region helper properties

        private decimal _NonBillableBalance;

        public decimal NonBillableBalance
        {
            get { return _NonBillableBalance; }
            set
            {
                _NonBillableBalance = value;
                _changeState |= PropertyChangeState.NONBILLABLE_BALANCCE;
            }
        }

        private decimal _CurrentAccountBalance;

        public decimal CurrentAccountBalance
        {

            get
            {
                return _CurrentAccountBalance;
            }
            set
            {
                _CurrentAccountBalance = value;
                _changeState |= PropertyChangeState.ACCOUNT_BALANCE;
            }
        }
        private decimal _CurrentPatientBalance;

        public decimal CurrentPatientBalance
        {
            get
            {
                if (_PatientBalanceUnavailable)
                    _CurrentPatientBalance = CurrentAccountBalance;
                return _CurrentPatientBalance;
            }
            set
            {
#if DEBUG
                if (_PatientBalanceUnavailable)
                    throw new InvalidOperationException(nameof(_PatientBalanceUnavailable) + " is set.");
#endif
                _CurrentPatientBalance = value;
                _changeState |= PropertyChangeState.PATIENT_BALANCE;
            }
        }
        public bool _PartialDemographicLoad { get; }
        public bool _PatientBalanceUnavailable { get; }
        private decimal _CurrentInsuranceBalance;

        public decimal CurrentInsuranceBalance
        {
            get
            {
                if (_InsuranceBalanceUnavailable)
                    _CurrentInsuranceBalance = 0;
                return _CurrentInsuranceBalance;
            }
            set
            {
#if DEBUG
                if (_InsuranceBalanceUnavailable)
                    throw new InvalidOperationException(nameof(_InsuranceBalanceUnavailable) + " is set.");
#endif
                _CurrentInsuranceBalance = value;
                _changeState |= PropertyChangeState.INSURANCE_BALANCE;
            }
        }
        public bool _InsuranceBalanceUnavailable { get; }
        public bool _InsuranceDetailUnavailable { get; }


        public bool? NeedOOO
        {
            get
            {
                if (_InsuranceDetailUnavailable || _InsuranceBalanceUnavailable)
                    return null;
                return _CurrentInsuranceBalance != _source.SumInsuranceBalances();
            }
        }
        private bool? _Inpatient;

        void getInpatient()
        {
            string original = _source[nameof(Inpatient)];
            if (!string.IsNullOrWhiteSpace(original))
            {
                _Inpatient = original.CheckSQLBool(true);
                if (_Inpatient.HasValue)
                    return;

				//Note: if original not in "I", "O", or SQL Bool, then need to change value to null/empty.
                _changeState |= PropertyChangeState.INPATIENT;
                if (original.Equals("I", StringComparison.OrdinalIgnoreCase))
                    _Inpatient = true;
                else if (original.Equals("O", StringComparison.OrdinalIgnoreCase))
                    _Inpatient = false;
            }
        }
        public bool? Inpatient
        {
            get
            {
                return _Inpatient;
            }
            set
            {
                if (_Inpatient == value)
                    return;
                _Inpatient = value;
                _changeState |= PropertyChangeState.INPATIENT;
            }
        }

        private BillingStatusCode _billingStatus = BillingStatusCode.UNKNOWN;

        public BillingStatusCode BillingStatus
        {
            get
            {
                return _billingStatus;
            }
            set
            {
                if (_billingStatus == value)
                    return;
                if (value == BillingStatusCode.UNKNOWN)
                    throw new InvalidOperationException("Cannot set BillingStatus to Unknown");
                _billingStatus = value;
                _changeState |= PropertyChangeState.BILLING_STATUS;
            }
        }
        private string _Vendor = null;
        public const string DEFAULT_VENDOR = "*UNKNOWN*";
        public string VendorCode
        {
            get
            {
                return _Vendor;
            }
            set
            {
                if (_Vendor.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;

                _Vendor = string.IsNullOrWhiteSpace(value) ? DEFAULT_VENDOR : value;
                _changeState |= PropertyChangeState.VENDOR;
            }
        }

        private DateTime? _OriginalBillDate;
        public DateTime? OriginalBillDate
        {
            get
            {
                return _OriginalBillDate;
            }
            set
            {
                if (_OriginalBillDate != value)
                {
                    _OriginalBillDate = value;
                    _changeState |= PropertyChangeState.ORIGINAL_BILL_DATE;
                }
            }
        }
#endregion
#region Address Info

        private Address _PatientAddress;

        /// <summary>
        /// Patient Address information (PatientAddress1/PatientAddress2/PatientCity/PatientState/PatientZip).
        /// <para>If you modify these individually (outside of the address object), then you may need to call <see cref="Address.Refresh"/> </para>
        /// </summary>
        public Address PatientAddress
        {
            get
            {
                if(_PatientAddress == null)
                    _PatientAddress = new Address(_source, AddressOwner.Patient);
                return _PatientAddress;
            }
        }

        private Address _GuarantorAddress;
        /// <summary>
        /// Guarantor Address information (PatientAddress1/PatientAddress2/PatientCity/PatientState/PatientZip).
        /// <para>If you modify these individually (outside of the address object), then you may need to call <see cref="Address.Refresh"/> </para>
        /// </summary>
        public Address GuarantorAddress
        {
            get
            {
                if(_GuarantorAddress == null)
                    _GuarantorAddress = new Address(_source, AddressOwner.Guarantor);
                return _GuarantorAddress;
            }
        }

        private Address _PatientEmployerAddress;
        /// <summary>
        /// patient Employer address information (PatientAddress1/PatientAddress2/PatientCity/PatientState/PatientZip).
        /// <para>If you modify these individually (outside of the address object), then you may need to call <see cref="Address.Refresh"/> </para>
        /// </summary>
        public Address PatientEmployerAddress
        {
            get
            {
                if (_PatientEmployerAddress == null)
                    _PatientEmployerAddress = new Address(_source, AddressOwner.PatientEmployer);
                return _PatientEmployerAddress;
            }
        }

        private Address _GuarantorEmployerAddress;
        /// <summary>
        /// Guarantor Employer address information (PatientAddress1/PatientAddress2/PatientCity/PatientState/PatientZip).
        /// <para>If you modify these individually (outside of the address object), then you may need to call <see cref="Address.Refresh"/> </para>
        /// </summary>
        public Address GuarantorEmployerAddress
        {
            get
            {
                if (_GuarantorEmployerAddress == null)
                    _GuarantorEmployerAddress = new Address(_source, AddressOwner.GuarantorEmployer);
                return _GuarantorEmployerAddress;
            }
        }

#endregion


        /// <summary>
        /// Method called toward the end of <see cref="Refresh"/> .
        /// <para>Can be overridden to allow refreshing additional properties.</para>
        /// </summary>
        protected virtual void PropertyRefresh() { }
        /// <summary>
        /// Method called at the end of <see cref="Sync"/> .
        /// <para>Can be overridden to allow syncing additional properties.</para>
        /// </summary>
        protected virtual void PropertySync() { }
        /// <summary>
        /// Set values to pull refresh from the linked DocRecord (file data - may or may not be changed)
        /// </summary>
        public void Refresh()
        {
            _AccountNumber = _source[nameof(AccountNumber)];
            _FacilityKey = _source[nameof(FacilityKey)] ?? string.Empty;
            _Vendor = _source[nameof(VendorCode)];
            if (string.IsNullOrWhiteSpace(_Vendor))
            {
                _source[nameof(VendorCode)] = _Vendor = DEFAULT_VENDOR;
            }
            _OriginalBillDate = _source.GetDateTime(nameof(OriginalBillDate));

            getInpatient();
            if (_changeState.HasFlag(PropertyChangeState.INPATIENT))
                _source[nameof(Inpatient)] = _Inpatient.Format(); 

            _billingStatus = _source.GetBillingStatus();

            _CurrentAccountBalance = _source.GetDecimal(nameof(CurrentAccountBalance)) ?? 0M;

            if (_PatientBalanceUnavailable)
            {
                _CurrentPatientBalance = CurrentAccountBalance;
                // Main hit in this logic is column name -> index mapping,
                // so just brute force set the column value to be correct.

                //var temp = _source.GetDecimal(nameof(CurrentPatientBalance)) ?? 0M;
                //if(temp != CurrentAccountBalance)
                _source.SetMoney(nameof(CurrentPatientBalance), _CurrentPatientBalance);
            }
            else
            {
                var d = _source.GetDecimal(nameof(CurrentPatientBalance));
                if (d.HasValue)
                    CurrentPatientBalance = d.Value;
                else
                {
                    _CurrentPatientBalance = 0M;
                    _source.SetMoney(nameof(CurrentPatientBalance), 0M);
                }
            }

            if (_InsuranceDetailUnavailable)
            {
                _CurrentInsuranceBalance = 0;
                _source.SetMoney(nameof(CurrentInsuranceBalance), 0M);
            }
            else
            {
                var d = _source.GetDecimal(nameof(CurrentInsuranceBalance));
                if (d.HasValue)
                    CurrentInsuranceBalance = d.Value;
                else
                {
                    _CurrentInsuranceBalance = 0M;
                    _source.SetMoney(nameof(CurrentInsuranceBalance), 0M);
                }
            }

            {
                var d = _source.GetDecimal(nameof(NonBillableBalance));
                if (d.HasValue)
                    _NonBillableBalance = d.Value;
                else
                {
                    _NonBillableBalance = 0M;
                    _source.SetMoney(nameof(NonBillableBalance), 0M);
                }
            }
            

            //These don't need to be called for sync, just refresh
            _GuarantorAddress?.Refresh();
            _PatientAddress?.Refresh();
            _PatientEmployerAddress?.Refresh();
            _GuarantorEmployerAddress?.Refresh();


            PropertyRefresh();
            _changeState = PropertyChangeState.NONE;

        }
        /// <summary>
        /// Optional usage to correct the Non billable balance.
        /// <para>For some clients, we may want to put the difference into insurance or patient balance instead, though</para>
        /// <para>And eventually, this may belong in the 'OTHER' category of bucket</para>
        /// </summary>
        public void FixNonBillable()
        {
            var expected = CurrentAccountBalance - CurrentPatientBalance - CurrentInsuranceBalance;
            if (expected != NonBillableBalance)
                NonBillableBalance = expected;
        }
        /// <summary>
        /// Sync the AccountSummary object to the underlying DocRecord which will be used to write to the actual file.
        /// </summary>
        public void Sync()
        {
            if (Modified)
            {
                if (_changeState.HasFlag(PropertyChangeState.ACCOUNT_BALANCE))
                    _source.SetMoney(nameof(CurrentAccountBalance), CurrentAccountBalance);
                if (_changeState.HasFlag(PropertyChangeState.PATIENT_BALANCE))
                    _source.SetMoney(nameof(CurrentPatientBalance), CurrentPatientBalance);
                if (_changeState.HasFlag(PropertyChangeState.INSURANCE_BALANCE))
                    _source.SetMoney(nameof(CurrentInsuranceBalance), CurrentInsuranceBalance);
                if (_changeState.HasFlag(PropertyChangeState.NONBILLABLE_BALANCCE))
                    _source.SetMoney(nameof(NonBillableBalance), NonBillableBalance);

                if (_changeState.HasFlag(PropertyChangeState.BILLING_STATUS))
                    _source.SetBillingStatus(_billingStatus);
                if (_changeState.HasFlag(PropertyChangeState.VENDOR))
                    _source[nameof(VendorCode)] = _Vendor;
                if (_changeState.HasFlag(PropertyChangeState.ORIGINAL_BILL_DATE))
                    _source.SetDateTime(nameof(OriginalBillDate), _OriginalBillDate);
                if (_changeState.HasFlag(PropertyChangeState.INPATIENT))
                    _source[nameof(Inpatient)] = _Inpatient.Format();


                /*
                 * AccountNumber and FacilityKey should not really change under normal circumstances. HOWEVER, we may need to either:
                 * 1. Modify the Account number by trimming or concatenating other fields
                 * 2. Construct the facility key from other fields (E.g. a substring of the AccountNumber)
                 *
                 * We want to be able to do this so that the payer master lookup logic can work correctly
                 */
                if (_changeState.HasFlag(PropertyChangeState.ACCOUNT_NUMBER))
                    _source[nameof(AccountNumber)] = _AccountNumber;
                if (_changeState.HasFlag(PropertyChangeState.FACILITY_CODE))
                    _source[nameof(FacilityKey)] = _FacilityKey;
                
                //Record should now match the object properties,
                //just need to check bucket and any virtual properties (PropertySync)
                _changeState = PropertyChangeState.NONE; 
            }
            SyncBuckets();
            PropertySync();
        }
        public bool Modified => _changeState != PropertyChangeState.NONE;
        PropertyChangeState _changeState = PropertyChangeState.NONE;

        [Flags]
        enum PropertyChangeState
        {
            NONE = 0,
            ACCOUNT_NUMBER,
            FACILITY_CODE = 2 * ACCOUNT_NUMBER,
            VENDOR = 2 * FACILITY_CODE,
            ORIGINAL_BILL_DATE = 2 * VENDOR,
            INPATIENT = 2 * ORIGINAL_BILL_DATE,
            BILLING_STATUS = 2 * INPATIENT,
            PATIENT_BALANCE = 2* BILLING_STATUS,
            INSURANCE_BALANCE = 2 * PATIENT_BALANCE,
            ACCOUNT_BALANCE = 2 * INSURANCE_BALANCE,
            NONBILLABLE_BALANCCE = 2 * ACCOUNT_BALANCE
        }
    }
}
