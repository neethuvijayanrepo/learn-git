using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;
using System.Globalization;

namespace SEIDR.DemoMap
{
    public static class Extensions
    {
        public static readonly NumberFormatInfo MONEY_FORMAT;

        static Extensions()
        {
            MONEY_FORMAT = new CultureInfo("EN-US", false).NumberFormat;
            MONEY_FORMAT.CurrencySymbol = string.Empty;
            MONEY_FORMAT.CurrencyGroupSeparator = string.Empty;
            MONEY_FORMAT.CurrencyNegativePattern = 1; // "-n".
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencynegativepattern?view=netcore-3.1
        }
        public static bool ContainsIgnoreCase(this string value, string test)
        {
            return value.IndexOf(test, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        /// <summary>
        /// If a value is provided, returns <see cref="FormatMoney(decimal)"/>. Else returns null.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string FormatMoney(this decimal? d)
        {
            if(d.HasValue)
                return d.Value.FormatMoney() ;
            return "0.00";
        }
        /// <summary>
        /// Formats a decimal value as a currency with no symbol or thousands separator. Two digits beyond the decimal.
        /// E.g., "1,405.304" => "1405.30", "132-" => "-132.00"
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string FormatMoney(this decimal d)
        {
            return d.ToString("C", MONEY_FORMAT);
        }
        public static decimal SumInsuranceBalances(this Doc.DocRecord line)
        {
            decimal sum = 0;
            for (int i = 1; i <= Bucket.MAX_SEQUENCE_NUMBER; i++)
            {
                if (line.CheckSelfPay(i))
                    continue;
                sum = sum + (line.GetInsuranceBalance(i) ?? 0);
            }
            return sum;
        }

        public static decimal SumPatientBalance(this Doc.DocRecord line)
        {
            decimal sum = 0;
            for (int i = 1; i <= Bucket.MAX_SEQUENCE_NUMBER; i++)
            {
                if (line.CheckSelfPay(i))
                    sum += (line.GetInsuranceBalance(i) ?? 0);
            }
            return sum;
        }
        public static void SetBillingStatus(this Doc.DocRecord line, BillingStatusCode newBillingStatus)
        {
            line[nameof(BillingStatusCode)] = newBillingStatus.ToString();
        }
        public static BillingStatusCode GetBillingStatus(this Doc.DocRecord line)
        {
            string code = line[nameof(BillingStatusCode)];
            if (string.IsNullOrWhiteSpace(code))
                return BillingStatusCode.UNKNOWN;

            foreach (BillingStatusCode status in Enum.GetValues(typeof(BillingStatusCode)))
            {
                //NOTE: we want to make sure that only the name can match, not the value.
                //So don't use Enum.Parse or Enum.TryParse
                if (code.Equals(status.ToString(), StringComparison.OrdinalIgnoreCase))
                    return status;
            }

            return BillingStatusCode.UNKNOWN;
            /*
            if (Enum.TryParse(code, true, out result))
            {
                return result;
            }
            return BillingStatusCode.UNKNOWN;
            */
        }
        /// <summary>
        /// Gets the value of self pay for the specified insurance sequence. If a value is not parsed or is not set, returns <paramref name="defaultValue"/>
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sequence"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool CheckSelfPay(this Doc.DocRecord line, int sequence = 1, bool defaultValue = default(bool))
        {
            return line.GetBool($"Ins{sequence}_IsSelfPay") ?? defaultValue;
        }
        /// <summary>
        /// Sets the self pay value for the insurance sequence to the formatted bool? value.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sequence"></param>
        /// <param name="value"></param>
        public static void SetSelfPay(this Doc.DocRecord line, int sequence, bool? value)
        {
            line[$"Ins{sequence}_IsSelfPay"] = value.Format();
        }
        /// <summary>
        /// Check if the file line has a payer code in the specified sequence.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static bool HasPayerCode(this Doc.DocRecord line, int sequence = 1)
        {
            return !string.IsNullOrEmpty(line.GetPayerCode(sequence));
        }
        /// <summary>
        /// Gets payer Code for specified insurance sequence. (Default to sequence 1 for Primary)
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static string GetPayerCode(this Doc.DocRecord line, int sequence = 1)
        {
            return line["Ins" + sequence + "_PayerCode"];
        }
        /// <summary>
        /// Gets the insurance balance for the specified sequence
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sequence">Defaults to 1 (Primary)</param>
        /// <returns></returns>
        public static decimal? GetInsuranceBalance(this Doc.DocRecord line, int sequence = 1)
        {
            string field = "Ins" + sequence + "_Balance";
            return line.GetDecimal(field);
        }

        public static void SetInsuranceBalance(this Doc.DocRecord line, int sequence, decimal? value)
        {
            line["Ins" + sequence + "_Balance"] = value.FormatMoney();
        }

        public static void SetInsurancePayerCode(this Doc.DocRecord line, int sequence, string newPayerCode)
        {
            line["Ins" + sequence + "_PayerCode"] = newPayerCode;
        }
        /// <summary>
        /// Format the bool as a string value for loading to Staging DB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Format(this bool b)
        {
            return b ? "1" : "0";

        }

        /// <summary>
        /// Format the bool as a string value for loading to Staging DB
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Format(this bool? b)
        {
            return b.HasValue 
                       ? b.Value 
                             ? "1" 
                             : "0" 
                       : null;
        }
        /// <summary>
        /// Gets a Bool? value from the specified field of the line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static bool? GetBool(this Doc.DocRecord line, string Column)
        {
            return line[Column].CheckSQLBool(true);
        }
        /// <summary>
        /// Gets a decimal value from the specified field of the line, or null if it cannot be parsed.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(this Doc.DocRecord line, string Column)
        {
            var value = line[Column];
            if (string.IsNullOrWhiteSpace(value))
                return 0; // MET-18475 For validation purposes, just return 0 when empty.
            decimal ret = 0;
            if (decimal.TryParse(value, out ret)) //Do not need to trim whitespace.
                return ret;
            return null;
        }

        public static void SetMoney(this Doc.DocRecord line, string Column, decimal? Value)
        {
            line[Column] = Value.FormatMoney();
        }

        public static void FlipSigns(this Account acct, params string[] MoneyColumns)
        {
            foreach (var s in MoneyColumns)
            {
                var d = acct.GetMoney(s) * -1;
                acct[s] = d.ToString();
            }
        }

        public static int? GetInt(this Doc.DocRecord line, string Column)
        {
            var value = line[Column];
            if (string.IsNullOrWhiteSpace(value))
                return null;
            int ret = 0;
            if (int.TryParse(value, out ret)) //Do not need to trim here.
                return ret;
            return null;
        }

        /// <summary>
        /// Formats a DateTime using format <see cref="ContextObjectBase.METRIX_DATE_FORMAT"/> 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Format(this DateTime value)
        {
            return value.ToString(ContextObjectBase.METRIX_DATE_FORMAT);
        }
        /// <summary>
        /// Formats a DateTime using format <see cref="ContextObjectBase.METRIX_DATE_FORMAT"/> 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Format(this DateTime? value)
        {
            return value?.ToString(ContextObjectBase.METRIX_DATE_FORMAT);
        }
        
        private static readonly System.Globalization.CultureInfo DefaultCulture = new System.Globalization.CultureInfo("en-US");

        private static readonly string[] DefaultDateFormat =
        {
            //"yyyy/MM/dd", "MM/dd/yyyy", "yyyy-MM-dd", "MM-dd-yyyy", //Covered by single M/single d
            "yyyyMMdd", "MMddyyyy",
            "yyyy/M/d", "M/d/yyyy", "yyyy-M-d", "M-d-yyyy",
            "M/d/yy", "yy-M-d"
        };

        private static readonly string[] FullMonthDateFormat =
        {//Allow whitespace
            "MMMMd,yyyy", "MMMMdyyyy", //Full month with day and year
            "MMMd,yyyy", "MMMdyyyy" //First three letters of month with day and year.
        };
        private static readonly string[] DefaultDateFormatWithTime =
        {
            "yyyyMMdd HH:mm:ss", "yyyyMMdd HH:mm",
            "MMddyyyy HH:mm:ss", "MMddyyyy HH:mm",
            "yyyy/M/d HH:mm:ss", "yyyy/M/d HH:mm",
            "M/d/yyyy HH:mm:ss", "M/d/yyyy HH:mm",
            "yyyy-M-d HH:mm:ss", "yyyy-M-d HH:mm",
            "M-d-yyyy HH:mm:ss", "M-d-yyyy HH:mm",


            "yyyyMMdd H:mm:ss", "yyyyMMdd H:mm",
            "MMddyyyy H:mm:ss", "MMddyyyy H:mm",
            "yyyy/M/d H:mm:ss", "yyyy/M/d H:mm",
            "M/d/yyyy H:mm:ss", "M/d/yyyy H:mm",
            "yyyy-M-d H:mm:ss", "yyyy-M-d H:mm",
            "M-d-yyyy H:mm:ss", "M-d-yyyy H:mm",

            "M/d/yy HH:mm:ss", "M/d/yy HH:mm",
            "yy-M-d HH:mm:ss", "yy-M-d HH:mm",

            "M/d/yy H:mm:ss", "M/d/yy H:mm",
            "yy-M-d H:mm:ss", "yy-M-d H:mm"
        };
        public static DateTime? GetDateTime(this Doc.DocRecord line, string Column)
        {
            string work = line[Column];
            if (string.IsNullOrWhiteSpace(work))
            {
                return null;
            }
            DateTime dateValue;
            if (DateTime.TryParseExact(work, DefaultDateFormat, 
                                       DefaultCulture, 
                                       System.Globalization.DateTimeStyles.AllowWhiteSpaces, out dateValue))
                return dateValue;
            if (DateTime.TryParseExact(work, DefaultDateFormatWithTime,
                                        DefaultCulture,
                                        System.Globalization.DateTimeStyles.AllowWhiteSpaces, out dateValue))
                return dateValue;
            try
            {
            	return DateTime.ParseExact(work, FullMonthDateFormat, DefaultCulture, DateTimeStyles.AllowWhiteSpaces);
            }catch(Exception ex)
            {
                throw new Exception($"DateTime Parse for column '{Column}'", ex);
            }
        }

        public static string[] GetSubElements(this string source, char delim, int Len)
        {
            string[] result = new string[Len];
            if (source == null)
                return result;
            string[] temp = source.Split(delim);
            for (int i = 0; i < temp.Length && i < Len; i++)
            {
                result[i] = temp[i];
            }

            return result;
        }
        

        public static DateTime? GetDateTimeFullMonth(this Doc.DocRecord line, string Column)
        {

            string work = line[Column];
            if (string.IsNullOrWhiteSpace(work))
            {
                return null;
            }
            DateTime dateValue;
            if (DateTime.TryParseExact(work, FullMonthDateFormat,
                                       DefaultCulture,
                                       System.Globalization.DateTimeStyles.AllowWhiteSpaces, out dateValue))
                return dateValue;            
            return null;
        }
        /// <summary>
        /// Sets the column to a date time value. (Calls Format for <paramref name="value"/> )
        /// </summary>
        /// <param name="line"></param>
        /// <param name="Column"></param>
        /// <param name="value"></param>
        public static void SetDateTime(this Doc.DocRecord line, string Column, DateTime? value)
        {
            line[Column] = value.Format();
        }

        /// <summary>
        /// Compares an input string against true/false and bit values
        /// <para>Null/Empty values as well as values that do not parse will return as null.</para>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeYesNo">If true, will also use Y/YES as 'True' and N/NO as 'False'</param>
        /// <param name="needTrim">Indicates whether or not the value needs to be trimmed first.</param>
        /// <returns></returns>
        public static bool? CheckSQLBool(this string input, bool includeYesNo = false, bool needTrim = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if(needTrim)
            	input = input.Trim();
            if (input.Equals("TRUE", StringComparison.OrdinalIgnoreCase)|| input.Equals("1", StringComparison.Ordinal))
                return true;
            if (input.Equals("FALSE", StringComparison.OrdinalIgnoreCase) || input.Equals("0", StringComparison.Ordinal))
                return false;
            if (!includeYesNo)
                return null;
            if (input.Equals("Y", StringComparison.OrdinalIgnoreCase) || input.Equals("YES", StringComparison.OrdinalIgnoreCase))
                return true;
            if (input.Equals("N", StringComparison.OrdinalIgnoreCase) || input.Equals("NO", StringComparison.OrdinalIgnoreCase))
                return false;
            return null;
        }
        /// <summary>
        /// Trims a string, but returns null if the value is empty or white space or null
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string NullifyEmpty(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            return input.Trim();
        }

    }
}
