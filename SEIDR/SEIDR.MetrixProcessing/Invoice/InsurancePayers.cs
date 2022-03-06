using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice
{
    /// <summary>
    /// For holding Ins1-4 payer descriptions for an Account or Encounter
    /// </summary>
    public class InsurancePayers
    {
        public InsurancePayers(IDataRecord record)
        {
            var tmp = record[nameof(Ins1)];
            Ins1 = tmp is DBNull ? null : (string)tmp;

            tmp = record[nameof(Ins2)];
            Ins2 = tmp is DBNull ? null : (string)tmp;

            tmp = record[nameof(Ins3)];
            Ins3 = tmp is DBNull ? null : (string)tmp;

            tmp = record[nameof(Ins4)];
            Ins4 = tmp is DBNull ? null : (string)tmp;
        }
        public string Ins1 { get; private set; }
        public string Ins2 { get; private set; }
        public string Ins3 { get; private set; }
        public string Ins4 { get; private set; }
    }
}
