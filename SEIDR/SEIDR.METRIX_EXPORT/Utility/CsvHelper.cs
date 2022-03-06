using GenericParsing;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SEIDR.METRIX_EXPORT.Utility
{
    public static class CsvHelper
    {
        private static readonly Regex unsafeCharactersAndSpace = new Regex("[,\n\r\t\"| ]{1,}", RegexOptions.Compiled);
        private static string CsvCleanse(string value)
        {
            //Cleanse versus escape?
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = unsafeCharactersAndSpace.Replace(value, " ");

            value = value.Trim();

            return value;

        }

        static readonly char[] charactersToEscape = new char[] { ',', '\"', '\r', '\n' };
        const char escapeCharacter = '\"';
        const string rowSeparator = "\r\n";
        const string decimalFormat = "0.00";
        const string dateFormat = "MM-dd-yyyy";

        public static StringBuilder RowSeparator(this StringBuilder builder)
        {
            builder.Append(rowSeparator);
            return builder;
        }

        public static StringBuilder CellSeparator(this StringBuilder builder)
        {
            builder.Append(',');
            return builder;
        }

        public static StringBuilder CellSeparatorPipe(this StringBuilder builder)
        {
            builder.Append('|');
            return builder;
        }
        public static StringBuilder AppendCsvCell(this StringBuilder builder, int? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }

            builder.Append(value.Value.ToString());

            return builder;
        }

        public static StringBuilder AppendCsvCell(this StringBuilder builder, decimal? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }

            builder.Append(value.Value.ToString(decimalFormat));

            return builder;
        }

        public static StringBuilder AppendCsvCell(this StringBuilder builder, DateTime? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }

            builder.Append(value.Value.ToString(dateFormat));

            return builder;
        }

        public static StringBuilder AppendCsvCell(this StringBuilder builder, string value, bool Cleanse = true)
        {

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (value == null)
            {
                return builder;
            }

            if (Cleanse)
                value = CsvCleanse(value);

            bool escape = value.IndexOfAny(charactersToEscape) != -1;

            if (escape)
            {
                builder.Append(escapeCharacter);
                foreach (char nextChar in value)
                {
                    builder.Append(nextChar);
                    if (nextChar == escapeCharacter)
                        builder.Append(escapeCharacter);
                }

                builder.Append(escapeCharacter);

            }
            else
            {
                builder.Append(value);
            }

            return builder;

        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable result = null;

            // Parse the CSV file with no header and fill the datatable
            using (GenericParserAdapter parser = new GenericParserAdapter(strFilePath))
            {
                parser.FirstRowHasHeader = true;
                result = parser.GetDataTable();
            }

            return result;
        }

    }
}
