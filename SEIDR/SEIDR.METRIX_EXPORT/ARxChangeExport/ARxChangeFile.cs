using SEIDR.METRIX_EXPORT.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.ARxChangeExport
{
    class ARxChangeFile
    {
        private static Regex unsafeCharactersAndSpace = new Regex("[,\n\r\t\"| ]{1,}", RegexOptions.Compiled);
        public TextWriter CreateFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException("filePath");
            }

            return File.CreateText(filePath);
        }
        public void WriteExportFileHeader(TextWriter writer)
        {

            var header = ComposeHeader();

            writer.Write(header);

        }
        public string ComposeHeader()
        {
            return "Metrix_Reference_Number,Total_Balance,Insurance_Balance,Patient_Balance,Primary_Insurance_Description,Total_PatientPayments,Total_InsurancePayments,Total_Adjustments,Total_Charges, ARxChange_Scored\r\n";
        }

        public void WriteExportFileRow(TextWriter writer, ExportBatchARxChangeModel item)
        {

            var row = ComposeRow(item);

            writer.Write(row);

        }

        public string ComposeRow(ExportBatchARxChangeModel item)
        {

            StringBuilder builder = new StringBuilder();

            builder
                .AppendCsvCell(Cleanse(item.Metrix_Reference_Number))
                .CellSeparator()               
                .AppendCsvCell(item.Total_Balance)
                .CellSeparator()
                .AppendCsvCell((item.Insurance_Balance))
                .CellSeparator()
                .AppendCsvCell(item.Patient_Balance)
                .CellSeparator()
                .AppendCsvCell(Cleanse(item.Primary_Insurance_Description))
                .CellSeparator()
                .AppendCsvCell(item.Total_PatientPayments)
                .CellSeparator()
                .AppendCsvCell(item.Total_InsurancePayments)
                .CellSeparator()
                .AppendCsvCell(item.Total_Adjustments)
                .CellSeparator()
                .AppendCsvCell(item.Total_Charges)
                .CellSeparator()
                .AppendCsvCell(item.ARxChange_Scored)


                .RowSeparator();

            return builder.ToString();

        }
        private static string Cleanse(string value)
        {

            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = unsafeCharactersAndSpace.Replace(value, " ");

            value = value.Trim();

            return value;

        }
    }
}
