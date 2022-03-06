using System;
using System.Collections.Generic;

namespace SEIDR.DemoMap.EPIC
{
    public class EpicClaimPayer
    {
        Decimal _TotalForPayer = 0;
        List<EpicClaim> _ClaimsForPayer = null;
        string _PayerCode = "";

        public EpicClaimPayer(string p)
        {
            _PayerCode = p;
            _ClaimsForPayer = new List<EpicClaim>();
        }

        public void AddClaim(EpicClaim e)
        {
            _ClaimsForPayer.Add(e);
            _TotalForPayer += e.GetAmount();
        }

        public Decimal TotalsForPayer => _TotalForPayer;
    }
}
