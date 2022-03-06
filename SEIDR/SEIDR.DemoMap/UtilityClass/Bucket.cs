using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;

namespace SEIDR.DemoMap
{
    public class Bucket
    {
        /*
         //Should not be taking a bucket from one account and putting on another.
         If need to create a new bucket on an account, use the method that's already on the account.
         If need to move a bucket to a different sequence number, use the shift method.
        public void LinkToAccount(Account a)
        {
            if (_account != null)
                throw new InvalidOperationException("Bucket is already linked to an account.");
            _account = a;
        } */

        public void UnlinkAccount()
        {
            _account = null;
            SequenceNumber = -1;
        }
        private Account _account;
        public bool Modified { get; private set; } = false;
        public const string BAD_BUCKET_PAYER_CODE = "CYMUNK";
        public const string SELFPAY_PAYER_CODE = "*SELFPAY*";
        public const string OOO_BUCKET_PAYER_CODE = "OOO";
        public const int MAX_SEQUENCE_NUMBER = 8;

        public bool IsBad => Payer?.PayerCode == BAD_BUCKET_PAYER_CODE;
        public bool IsOOO => Payer?.PayerCode == OOO_BUCKET_PAYER_CODE;

        private readonly Doc.DocRecord _source;
        public readonly ContextObjectBase Context;

        public Address GetAddressInfo()
        {
            AddressOwner type = (AddressOwner)Enum.Parse(typeof(AddressOwner), 
                                                         SequenceNumber.ToString());
            return new Address(_source, type);
        }
        private decimal? GetBalance()
        {
            return _source.GetDecimal(_prefix + nameof(Balance));
        }

        private void SetBalance()
        {
            _source[_prefix + nameof(Balance)] = PrincipalBalance;
        }
        public Bucket(Doc.DocRecord source, int sequence, Account account)
        {
            _prefix = "Ins" + sequence + "_";
            _source = source;
            _account = account;
            Context = _account.Context;
            SequenceNumber = sequence;
            string payerCode = source[_prefix + nameof(PayerCode)];
            if (!string.IsNullOrEmpty(payerCode))
            {
                SetPayerMaster(payerCode);
                var pb = GetBalance();
                if (pb == null)
                    SetBalance(); 
                else
                    _Balance = pb.Value; //_Balance defaults to 0, so don't need to set this if pb == null. Also don't need to use the property.
                //Already know we have a payer sequence, so just make sure a value is there.
            }
            else
            {
                var pb = GetBalance();
                if (pb.HasValue && pb.Value != 0)
                {
                    SetPayerInfo(BAD_BUCKET_PAYER_CODE);
                    _Balance = pb.Value;
                }
            }
        }
        /// <summary>
        /// Use to change the PayerCode.
        /// </summary>
        /// <param name="payerCode"></param>
        public void SetPayerInfo(string payerCode)
        {
            Modified = true;
            SetPayerMaster(payerCode);

        }
        void SetPayerMaster(string payerCode)
        {
            if (string.IsNullOrWhiteSpace(payerCode))
            {
                Payer = null;
                return;
            }
            string facilityKey = _account.FacilityKey;
            PayerMaster_MapInfo p;
            string selfPayColumn = _prefix + nameof(IsSelfPay);
            if (Context.CheckPayerExist(facilityKey, payerCode, out p))
                Payer = p;
            else
            {
                bool defaultSelfPay = false;
                if (payerCode == SELFPAY_PAYER_CODE)
                    defaultSelfPay = true;
                else if(payerCode.NotIn(OOO_BUCKET_PAYER_CODE, BAD_BUCKET_PAYER_CODE))
                {
                    defaultSelfPay = _source.GetBool(selfPayColumn) ?? false;
                }
                bool isSelfPay = _source.GetBool(selfPayColumn) ?? defaultSelfPay;
                Payer = Context.GetPayerInfo(facilityKey, payerCode, isSelfPay);
            }
            _source[_prefix + nameof(IsSelfPay)] = Payer.IsSelfPay.Format();
        }

        public void SetPayerInfo(string payerCode, bool defaultSelfPay)
        {
            Modified = true;
            string facilityKey = _account.FacilityKey;
            PayerMaster_MapInfo p;
            if (Context.CheckPayerExist(facilityKey, payerCode, out p))
                Payer = p;
            else
            {
                Payer = Context.GetPayerInfo(facilityKey, payerCode, defaultSelfPay);
            }
        }
        public bool Valid => Payer != null;
        public PayerMaster_MapInfo Payer { get; private set; }
        public string PayerCode => Payer.PayerCode;
        public bool IsSelfPay => Payer.IsSelfPay;
        public PayerType PayerType => Payer.PayerType;
        public string FacilityKey => _account.FacilityKey;
        private decimal _Balance;
        /// <summary>
        /// String representation of <see cref="Balance"/>. Primary usage is for APB.
        /// </summary>
        public string PrincipalBalance => _Balance.ToString(System.Globalization.CultureInfo.InvariantCulture);
        /// <summary>
        /// Balance associated with the payer bucket.
        /// </summary>
        public decimal Balance
        {
            get { return _Balance; }
            set
            {
                if (_Balance == value)
                    return;
                _Balance = value;
                Modified = true;
            }
        }
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// Shift the bucket information to a new sequence. 
        /// <para>NOTE: The new position will be completely overwritten, and data from the old position will be cleared!</para>
        /// </summary>
        /// <param name="newSequenceNumber"></param>
        public void Shift(int newSequenceNumber)
        {
            if (newSequenceNumber == SequenceNumber)
                return;
            if (_account == null)
                throw new InvalidOperationException("Cannot Shift a bucket if it's not linked to an account.");
            if (Modified)
                Apply();
            var bucket = _account[newSequenceNumber];
            if(bucket != null)
                _account.RemoveBucket(bucket); // Should take care of clearing the new position. Should only need to do this if there's already a bucket at that position.
            string newPrefix = "Ins" + newSequenceNumber + "_";
            var bucketFrom = _source.GetColumns(c => c.ColumnName.StartsWith(_prefix));
            foreach (var col in bucketFrom)
            {
                string newValue = _source[col];
                if (string.IsNullOrEmpty(newValue))
                    continue;

                string colTo = col.ColumnName.Replace(_prefix, newPrefix);
                _source[colTo] = newValue; // _source[col];
                //if (clearOldPosition)
                _source[col] = null;
            }
            SequenceNumber = newSequenceNumber;
            _prefix = newPrefix;
            _account.CheckBucketSequences();
            Modified = false;
        }

        /// <summary>
        /// Apply balance and payer changes to the sequence number.
        /// </summary>
        public void Apply()
        {
            if (SequenceNumber < 0)
                throw new InvalidOperationException("Cannot apply a bucket that has been removed.");
            if (!Modified)
                return;
            _source[_prefix + nameof(PayerCode)] = Payer.PayerCode;
            _source[_prefix + nameof(Balance)] = PrincipalBalance;
            _source[_prefix + nameof(IsSelfPay)] = Payer.IsSelfPay.Format();
            Modified = false;
        }
        /// <summary>
        /// Clear the key bucket fields for this sequence in the source line.
        /// </summary>
        public void ClearSource()
        {
            Payer = null;
            Balance = 0;
            if (SequenceNumber < 0)
                return; //already unattached, shouldn't need to do cleanup again.
            ClearBucketSequence(_source, SequenceNumber);
            Modified = false;
        }

        private string _prefix;
        public string this[string field]
        {
            get
            {
                return _source[_prefix + field];
            }
            set
            {
                if (field.In(nameof(Balance), nameof(SequenceNumber), nameof(PayerCode), nameof(IsSelfPay)))
                    throw new InvalidOperationException("Cannot modify field " + field + " from indexer. Use property.");
                _source[_prefix + field] = value;
            }
        }
        /// <summary>
        /// Gets a subset list of columns associated with this bucket, based on a predicate.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public IEnumerable<Doc.DocRecordColumnInfo> GetColumns(Predicate<Doc.DocRecordColumnInfo> test)
        {
            return from col in _source.GetColumns(test)
                   where col.ColumnName.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase)
                   select col;
        }
        
        public static void ClearBucketSequence(Doc.DocRecord source, int sequenceNumber)
        {
			// For Metrix loading purposes, we really only care about PayerCode/balance really.
            string prefix = "Ins" + sequenceNumber + "_";
            source[prefix + nameof(PayerCode)] = null;
            source[prefix + nameof(Balance)] = null;
            source[prefix + nameof(IsSelfPay)] = null;

            //foreach(var col in source.GetColumns(c => c.ColumnName.StartsWith("Ins" + sequenceNumber, StringComparison.OrdinalIgnoreCase)))
            //{
            //    source[col] = null;
            //}
            /*
            source.SetInsuranceBalance(sequenceNumber, null);
            source.SetInsuranceBalance(sequenceNumber, null);
            source.SetInsurancePayerCode(sequenceNumber, null);
            */
        }

    }
}
