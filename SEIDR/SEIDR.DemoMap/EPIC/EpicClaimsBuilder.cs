using SEIDR.Doc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap.EPIC
{
    public class EpicClaimsBuilder
    {
        private string _InputFile = null;
        const string RECORD_TYPE = "06";
        private char _delimiter = '|';

        public EpicClaimsBuilder(string inputfile,char _delim)
        {
            _InputFile = inputfile;
            _delimiter = _delim; 
        }

        public EpicClaims CreateClaims()
        {
            EpicClaims e = new EpicClaims();
            string line;
            using (var s = new StreamReader(_InputFile))
            {
                while ((line = s.ReadLine()) != null)
                {
                    string[] parts = line.Split(_delimiter);
                    // Create claim records
                    EpicClaim ec = new EpicClaim(line);
                    EpicClaimPacket ep = e.FindPacket(ec.AccountNumber) ?? new EpicClaimPacket(ec.AccountNumber);
                    ep.AddClaimToPacket(ec);
                    e.AddPacket(ep);
                }
            }
            return e;
        }
    }
}
