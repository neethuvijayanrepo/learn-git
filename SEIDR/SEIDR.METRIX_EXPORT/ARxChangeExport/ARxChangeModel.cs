using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.ARxChangeExport
{
    public class ExportBatchARxChangeModel
    {

        public string Metrix_Reference_Number { get; set; }
        public decimal? Total_Balance { get; set; }
        public decimal? Insurance_Balance { get; set; }
        public decimal? Patient_Balance { get; set; }
        public string Primary_Insurance_Description { get; set; }
        public decimal? Total_PatientPayments { get; set; }
        public decimal? Total_InsurancePayments { get; set; }
        public decimal? Total_Adjustments { get; set; }
        public decimal? Total_Charges { get; set; }
        public byte? ARxChange_Scored { get; set; }
    }
    public class ExportARxChangeModel
    {
        public int? ProjectId { get; set; }
        public int? OrganizationID { get; set; }

        public int? JobExecutionID { get; set; }
        public string FilePath { get; set; }

    }
}

