using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap.EPIC
{
    public class EpicClaims
    {
        private Dictionary<string,EpicClaimPacket> _claimsDict = null;

        public EpicClaims()
        {
            _claimsDict = new Dictionary<string, EpicClaimPacket>();
        }

        public void AddPacket(EpicClaimPacket ep)
        {
            if (!_claimsDict.ContainsKey(ep.AccountNumberRaw))
            {
                _claimsDict.Add(ep.AccountNumberRaw, ep);
            }
            else
            {
                _claimsDict[ep.AccountNumberRaw] = ep;
            }
        }

        public EpicClaimPacket FindPacket(string _account)
        {
            return _claimsDict != null && !string.IsNullOrEmpty(_account) && _claimsDict.ContainsKey(_account)
                ? _claimsDict[_account]
                : null;
        }

        public IEnumerable<EpicClaim> FindValidClaims(string _account)
        {
            return FindPacket(_account).Claims.Where(el => el.IsValidClaim);
        }
    }
}
