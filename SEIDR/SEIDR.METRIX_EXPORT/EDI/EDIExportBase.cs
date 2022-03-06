using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.EDI
{
    public abstract class EDIExportBase :ExportJobBase
    {
        public readonly EDICode TypeCode;
        private const string EDI_CODE_PREFIX = "EDI";
        public readonly string EdiTransactionCode;
        

        protected EDIExportBase(EDICode type)
        {
            TypeCode = type;
            EdiTransactionCode = TypeCode.ToString().Substring(EDI_CODE_PREFIX.Length);
        }

        public  EdiTransactionSetInfo GetTransactionSetInfo(ExportContextHelper context)
        {
            var param = new {context.VendorName, EDICode = EdiTransactionCode};
            return context.MetrixManager.SelectSingle<EdiTransactionSetInfo>(param, Schema: EXPORT_SCHEMA);
        }
        public List<EdiCriteria> GetOpenEdiCriteriaList(ExportContextHelper context)
        {

            var param = new {context.VendorName, TransactionSetCode = EdiTransactionCode};
            return context.MetrixManager.SelectList<EdiCriteria>(param, Schema: EXPORT_SCHEMA);
        }
        /// <summary>
        /// Creates a file name using the cleaned project Name, FacilityName, <see cref="TypeCode"/>,  processing date, and provided extension.
        /// <para>Configuration parameters for whether or not to include a GUID and how to format the processing date.</para>
        /// </summary>
        /// <param name="projectName">Project Description - should already be cleaned by <see cref="ExportJobBase.PrepForFileName(string)"/> </param>
        /// <param name="facilityName">Facility Description - should already be cleaned by <see cref="ExportJobBase.PrepForFileName(string)"/></param>
        /// <param name="dateIn">Either processing date or <see cref="DateTime.Now"/> </param>
        /// <param name="extension">Extension for result file name - do not include the dot</param>
        /// <param name="includeGuid"></param>
        /// <param name="dateFormat">Format for applying the <paramref name="dateIn"/> parameter to the resulting file name.</param>
        /// <returns></returns>
        public string CreateFileName(string projectName, string facilityName, DateTime dateIn, 
            string extension = "txt", 
            bool includeGuid = true, string dateFormat = "yyyyMMdd")
        {
            string guidString = string.Empty;
            if (includeGuid)
                guidString = "G" + Guid.NewGuid().ToString().Substring(23) + "_";


            string formattedDate = dateIn.ToString(dateFormat);
            if (string.IsNullOrWhiteSpace(facilityName))
                return $"{projectName}_{TypeCode}_{guidString}{formattedDate}.{extension}";
            else
                return $"{projectName}_{facilityName}_{TypeCode}_{guidString}{formattedDate}.{extension}"; 
            /*
            return String.Format("{0}_{1}_EDI276_{2}_G{3}_{4}.{5}",
                                 MakeValidFileName(projectName.Replace("'", "").Replace(".", ""))
                               , MakeValidFileName(facilityName.Replace("'", "").Replace(".", ""))
                               , scheduleTypeCode.ToUpper()
                               , Guid.NewGuid().ToString().Substring(23)
                               , dateIn.ToString("yyyyMMdd"), extension);
                               */
        }
    }
}
