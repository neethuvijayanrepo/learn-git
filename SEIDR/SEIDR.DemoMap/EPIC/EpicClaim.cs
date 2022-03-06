using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap.EPIC
{
    
    public class EpicClaim
    {
        const char DELIMITER = '|';

        private string _raw = null;
        private string[] _RawSplit;

        public EpicClaim(string s)
        {
            _raw = s;
            _RawSplit = Parse(_raw);
        }

        public string[] Parse(string s)
        {
            return _raw.Split(DELIMITER);
        }

        public string RecordType => _RawSplit[0];

        public string AccountNumber => _RawSplit[1];
        public string BucketType  => _RawSplit[2]; 
        public string PayerCode => _RawSplit[3]; 
        public string PayerName => _RawSplit[4]; 
        public string InvoiceNum => _RawSplit[5]; 
        public string ClaimAmount => _RawSplit[6]; 
        public string FirstClaimSentDate => _RawSplit[7]; 
        public string LastClaimSentDate => _RawSplit[8]; 
        public string FirstExternalClaimSentDate => _RawSplit[9]; 
        public string LastExternalClaimSentDate => _RawSplit[10]; 
        public string ClaimType  => _RawSplit[11]; 
        public string FormType => _RawSplit[12]; 
        public bool IsElectronicClaim => FormType.Equals("Electronic", StringComparison.InvariantCultureIgnoreCase) ? true : false; 
        public string AccountNumberTrimLeadingZeroes => AccountNumber.TrimStart('0'); 

        public bool IsValidClaim
        {
            get
            {
                return string.IsNullOrEmpty(InvoiceNum) && string.IsNullOrEmpty(FirstClaimSentDate)
                    ? false
                    : true;
            }
        }

        public Decimal GetAmount()
        {
            decimal ret;
            return Decimal.TryParse(ClaimAmount, out ret) ? ret : 0;
        }

        public DateTime DateFirstClaimSent()
        {
            return DateTime.ParseExact(FirstClaimSentDate, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
    }
}
