using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TestProject.Utilities.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// To escape ' and " in a string 
        /// </summary>
        /// <param name="name">The input string</param>
        /// <returns>String with escaped ' & "</returns>
        public static string EscapeQuotes(this string name)
        {
            return name.Replace("\"", "\"").Replace("'", "\\'");
        }

        /// <summary>
        /// To convert a string to a phone number.
        /// </summary>
        /// <param name="phoneNumberString">Input string</param>
        /// <returns>Phone number as string</returns>
        public static string ToPhoneNumber(this string phoneNumberString)
        {
            if (string.IsNullOrEmpty(phoneNumberString))
            {
                return string.Empty;
            }

            var numberPattern = new System.Text.RegularExpressions.Regex("\\d+");
            MatchCollection numberStrips = numberPattern.Matches(phoneNumberString);
            if (numberStrips.Count < 1)
            {
                return string.Empty;
            }

            StringBuilder phoneNumber = new StringBuilder();
            foreach (Match strip in numberStrips)
            {
                phoneNumber.Append(strip.Value);
            }

            if (phoneNumber.Length > 6)
            {
                phoneNumber.Insert(6, " ");
            }

            if (phoneNumber.Length > 3)
            {
                phoneNumber.Insert(3, " ");
            }

            return phoneNumber.ToString();
        }
    }
}
