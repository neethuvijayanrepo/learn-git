using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.Utility
{
    /// <summary>
    /// Document Writer for Export(CSV Writer)
    /// </summary>
    class DelimitedFileHelper
    {
        private readonly Regex unsafeCharactersAndSpace;
        readonly char[] charactersToEscape = new char[] { '\0', '\"', '\r', '\n' };
        const char escapeCharacter = '\"';
        const string rowSeparator = "\r\n";
        readonly char cellSeparator ;
        const string decimalFormat = "0.00";
        const string dateFormat = "MM-dd-yyyy";
        public DelimitedFileHelper(char delimiter) {
            cellSeparator = delimiter;
            charactersToEscape[0] = delimiter;
            unsafeCharactersAndSpace = new Regex("[" + new string(charactersToEscape) + "]{1,}", RegexOptions.Compiled);
        }
        /// <summary>
        /// Create file
        /// </summary>
        /// <param name="filePath">path</param>
        /// <returns></returns>
        public TextWriter CreateFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException("filePath");
            }
            return File.CreateText(filePath);
        }

        /// <summary>
        /// Write Header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="dataClass"></param>
        /// <param name="ignoreProperties">List of Properties to be ignored</param>
        public void WriteExportFileHeader<T>(TextWriter writer, T dataClass, List<string> ignoreProperties = null)
        {
            var header = ComposeHeader(dataClass, ignoreProperties);
            writer.Write(header);
        }
        /// <summary>
        /// Write records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="dataList">list of records</param>
        /// <param name="ignoreProperties">List of Properties to be ignored</param>
        public void WriteExportFileRow<T>(TextWriter writer, List<T> dataList, List<string> ignoreProperties = null)
        {
            foreach (var item in dataList)
            {
                var row = ComposeRow(item, ignoreProperties);
                writer.Write(row);
            }
        }
        /// <summary>
        /// Compose the Header string for the given class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataClass"></param>
        /// <param name="ignoreProperties">List of Properties to be ignored</param>
        /// <returns></returns>
        private string ComposeHeader<T>(T dataClass, List<string> ignoreProperties = null)
        {
            string output = "";
            var properties = dataClass.GetType().GetProperties();

            for (var i = 0; i < properties.Length; i++)
            {
                if (ignoreProperties == null || !ignoreProperties.Contains(properties[i].Name))
                {
                    string propName;
                    propName = GetDisplayName(properties[i]);
                    output += propName;
                    if (i != properties.Length - 1)
                    {
                        output += cellSeparator;
                    }
                }
            }
            output += cellSeparator;
            output += rowSeparator;
            return output;
        }
        /// <summary>
        /// Get Name/Display Name of the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public string GetDisplayName(PropertyInfo property)
        {
            var attrName = GetAttributeDisplayName(property);
            if (!string.IsNullOrEmpty(attrName))
                return attrName;

            return property.Name.ToString();
        }
        /// <summary>
        /// Get Dispaly Name attribute
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private string GetAttributeDisplayName(PropertyInfo property)
        {
            var atts = property.GetCustomAttributes(
                typeof(DisplayNameAttribute), true);
            if (atts.Length == 0)
                return null;
            return (atts[0] as DisplayNameAttribute).DisplayName;
        }
        /// <summary>
        /// Compose single row of record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="ignoreProperties">List of Properties to be ignored</param>
        /// <returns></returns>
        private string ComposeRow<T>(T item, List<string> ignoreProperties = null)
        {
            StringBuilder builder = new StringBuilder();
            var properties = item.GetType().GetProperties();

            for (var i = 0; i < properties.Length; i++)
            {
                if (ignoreProperties == null || !ignoreProperties.Contains(properties[i].Name))
                {
                    builder = AppendCsvCell(builder, properties[i].Name, item);
                    builder = CellSeparator(builder);
                }
            }
            builder = RowSeparator(builder);

            return builder.ToString();
        }
        /// <summary>
        /// Append row separator string to the builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public StringBuilder RowSeparator(StringBuilder builder)
        {
            builder.Append(rowSeparator);
            return builder;
        }
        /// <summary>
        /// Append cell separatior character to the builder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private StringBuilder CellSeparator(StringBuilder builder)
        {
            builder.Append(cellSeparator);
            return builder;
        }
        /// <summary>
        /// Append single cell of reord based on the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private StringBuilder AppendCsvCell<T>(StringBuilder builder, string propName, T value)
        {
            Type t = value.GetType();
            PropertyInfo propInfo = t.GetProperty(propName);
            if (propInfo.PropertyType == typeof(int))
            {
                builder = AppendCell(builder, Convert.ToInt32(propInfo.GetValue(value)));
            }
            else if (propInfo.PropertyType == typeof(decimal))
            {
                builder = AppendCell(builder, Convert.ToDecimal(propInfo.GetValue(value)));
            }
            else if ((propInfo.PropertyType == typeof(DateTime)) || (propInfo.PropertyType == typeof(DateTime?)))
            {
               if( propInfo.GetValue(value) != null)
                    builder = AppendCell(builder,  Convert.ToDateTime(propInfo.GetValue(value)));
               else
                    builder = AppendCell(builder, "");
            }
            else if (propInfo.PropertyType == typeof(string))
            {
                builder = AppendCell(builder, Convert.ToString(propInfo.GetValue(value)));
            }
            return builder;
        }
        /// <summary>
        /// Append Int value in a row
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder AppendCell(StringBuilder builder, int? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }
            builder.Append(value.Value.ToString());
            return builder;
        }
        /// <summary>
        /// Append Decimal value in a row
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder AppendCell(StringBuilder builder, decimal? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }
            builder.Append(value.Value.ToString(decimalFormat));
            return builder;
        }
        /// <summary>
        /// Append DateTime value in a row
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringBuilder AppendCell(StringBuilder builder, DateTime? value)
        {
            if (value.HasValue == false)
            {
                return builder;
            }
            builder.Append(value.Value.ToString(dateFormat));
            return builder;
        }
        /// <summary>
        /// Append String value in a row
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <param name="needCleanse"></param>
        /// <returns></returns>
        public StringBuilder AppendCell(StringBuilder builder, string value, bool needCleanse = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (value == null)
            {
                return builder;
            }
            if (needCleanse)
                value = Cleanse(value);

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
        /// <summary>
        /// Replace unsafe characters from the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string Cleanse(string value)
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
