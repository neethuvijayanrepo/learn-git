using System;
using System.Collections.Generic;

namespace SEIDR.DemoMap.EPIC
{
    public class EpicClaimPacket
    {
        private List<EpicClaim> _claims = null;
        private Dictionary<string,EpicClaimPayer> _payers = null;
        private string _AccountNumber = null;
        private decimal _TotalPacketAmount = 0;

        public EpicClaimPacket(string _account)
        {
            _AccountNumber = _account;
            _claims = new List<EpicClaim>();
            _payers = new Dictionary<string, EpicClaimPayer>();
        }

        public void AddClaimToPacket(EpicClaim e)
        {
            if(! _payers.ContainsKey(e.PayerCode))
            {
                _payers[e.PayerCode] = new EpicClaimPayer(e.PayerCode);
            }
            _payers[e.PayerCode].AddClaim(e);
            _claims.Add(e);
            _TotalPacketAmount += e.GetAmount();
        }

        public EpicClaimPayer FindPayer(string p)
        {
            if(! _payers.ContainsKey(p))
            {
                return null;
            }
            return _payers[p];
        }

        public DateTime? FindFirstSentDate()
        {
            DateTime? d = null;
            foreach(var c in _claims)
            {
                if (!c.IsValidClaim) continue;
                d = d == null || d > c.DateFirstClaimSent() ? c.DateFirstClaimSent() : d;
            }
            return d;
        }

        public List<EpicClaim> Claims => _claims;

        public string AccountNumberRaw
        {
            get { return _AccountNumber; }
            set { _AccountNumber = value; }
        }

        public string AccountNumberClean => AccountNumberRaw.TrimStart('0');
        public Dictionary<string,EpicClaimPayer> ClaimsByPayerCode => _payers;
        public Decimal TotalPacketAmount => _TotalPacketAmount;
    }
}
