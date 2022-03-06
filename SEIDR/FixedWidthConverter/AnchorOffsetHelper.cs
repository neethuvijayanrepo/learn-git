using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedWidthConverter
{
    public class AnchorOffsetHelper
    {
        public int Offset = 0;
        public string ColumnName;
        public int StartPosition;
        public int? EndPosition;
        public int PullCount => EndPosition.HasValue ? EndPosition.Value - StartPosition : 0;
        //Line +- Item1 named as Item2, pulling string from Item3 up to Item4 or end of line
        //public List<Tuple<int, string, int, int?>
        public override string ToString()
        {
            string ret = ColumnName + " >>> (Offset:";
            if (Offset >= 0)
                ret += "+";
            ret += Offset + "):" + StartPosition;
            if (EndPosition.HasValue)
                return ret + "-" + EndPosition.Value;
            else
                return ret;
        }
        public static List<AnchorOffsetHelper> GetFromFixWidthSettings(SEIDR.FixWidthConverter fwc)
        {
            List<AnchorOffsetHelper> helper = new List<AnchorOffsetHelper>();
            foreach(var tuple in fwc.AnchorModDerivePulls)
            {
                helper.Add(new AnchorOffsetHelper
                {
                    Offset = tuple.Item1,
                    ColumnName = tuple.Item2,
                    StartPosition = tuple.Item3,
                    EndPosition = tuple.Item4                
                });
            }
            return helper;
        }
        public static void UpdateConverter(SEIDR.FixWidthConverter fwc, List<AnchorOffsetHelper> input)
        {
            fwc.AnchorModDerivePulls.Clear();
            foreach(var anchor in input)
            {
                fwc.AnchorModDerivePulls.Add(new Tuple<int, string, int, int?>(anchor.Offset, anchor.ColumnName, anchor.StartPosition, anchor.EndPosition));
            }
        }
        public static List<Tuple<int, string, int, int?>> GetAnchorModPulls(List<AnchorOffsetHelper> input)
        {
            var outList = new List<Tuple<int, string, int, int?>>();
            foreach(var anchor in input)
            {
                outList.Add(new Tuple<int, string, int, int?>(anchor.Offset, anchor.ColumnName, anchor.StartPosition, anchor.EndPosition));
            }
            return outList;
        }
    }
}
