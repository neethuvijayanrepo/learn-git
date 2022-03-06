using System.Collections.Generic;
using SEIDR.DataBase;

namespace SEIDR.DemoMap
{
    public class MAPS_DELIMITED
    {
        //public decimal MAPID { get; set; }
        //public int? PackageID { get; set; }
        public string CymetrixFieldName { get; set; }
        public string ClientFieldName { get; set; }
        public int? ClientFieldIndex { get; set; }
        //public string Notes { get; set; }

        public const string GET_EXECUTION_INFO = "SSIS.SP_Get_SSIS_Delimited_Maps";
        public static List<MAPS_DELIMITED> GetMAPS_DELIMITED_Columns(DatabaseManager dm, int nPackageID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = GET_EXECUTION_INFO;
                helper[nameof(nPackageID)] = nPackageID;
                return dm.SelectList<MAPS_DELIMITED>(helper);
            }
        }
    }
}
