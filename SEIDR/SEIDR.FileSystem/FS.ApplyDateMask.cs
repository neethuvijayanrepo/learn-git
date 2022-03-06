using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEIDR.FileSystem
{
    partial class FS
    {
        /// <summary>
        /// Uses the provided <paramref name="originalDate"/> to replace the date mask of a filepath. Example:
        /// <para>&lt;-1D>&lt;YYYY>&lt;MM>&lt;DD>_test.txt for processing date 2/23/2018 -> 20180222_test.txt</para>
        /// <para>For the reverse(using a date mask on a specific file to pull an associated file date) see: <see cref="SEIDR.Doc.DocExtensions.ParseDateRegex(string, string, ref DateTime)"/>. E.g., to pull the 2/23/2018 date from 20180222_test.txt</para>
        /// </summary>
        /// <param name="SourceFile"></param>
        /// <param name="originalDate"></param>
        /// <returns></returns>
		public static string ApplyDateMask(string SourceFile, DateTime originalDate)
        {
            const string MONTH_KEY = "M";
            const string YEAR_KEY = "Y";
            const string DAY_KEY = "D";
            //Ex: <-1D><YYYY><MM><DD>_test.txt for processing date 2/23/2018 -> 20180222_test.txt
            if (string.IsNullOrEmpty(SourceFile))
                return null;
            //DateTime d = originalDate;
            Match m = Regex.Match(SourceFile, @"<[\-+]\d+[MDY]>", RegexOptions.IgnoreCase);
            while(m.Success){
                string temp = m.Value;
                m = m.NextMatch();
                SourceFile = SourceFile.Replace(temp, "");
                int offset = 0;
                //temp = temp.ToUpper();
                if (temp.Contains(MONTH_KEY, StringComparison.OrdinalIgnoreCase))
                {//Month offset
                    temp = temp.Substring(0, temp.IndexOf(MONTH_KEY, StringComparison.OrdinalIgnoreCase));
                    offset = Convert.ToInt32(temp.Substring(2));
                    if (temp[1] == '-')
                    {
                        offset = offset * -1;
                    }
                    originalDate = originalDate.AddMonths(offset);
                }
                else if (temp.Contains(YEAR_KEY, StringComparison.OrdinalIgnoreCase))
                {//Year offset
                    temp = temp.Substring(0, temp.IndexOf(YEAR_KEY, StringComparison.OrdinalIgnoreCase));
                    offset = Convert.ToInt32(temp.Substring(2));
                    if (temp[1] == '-')
                    {
                        offset = offset * -1;
                    }
                    originalDate = originalDate.AddYears(offset);
                }
                else
                {//Day
                    temp = temp.Substring(0, temp.IndexOf(DAY_KEY, StringComparison.OrdinalIgnoreCase));
                    offset = Convert.ToInt32(temp.Substring(2));
                    if (temp[1] == '-')
                    {
                        offset = offset * -1;
                    }
                    originalDate = originalDate.AddDays(offset);
                }
            }
            return UserFriendlyDateRegex.Eval(SourceFile, originalDate); //Case insensitive replace.
            /*
            SourceFile = SourceFile
                .Replace("<YYYY>", originalDate.Year.ToString().PadLeft(4, '0'))
                .Replace("<YY>", originalDate.Year.ToString().PadLeft(4, '0').Substring(2, 2))
                .Replace("<MM>", originalDate.Month.ToString().PadLeft(2, '0'))
                .Replace("<M>", originalDate.Month.ToString())
                .Replace("<DD>", originalDate.Day.ToString().PadLeft(2, '0'))
                .Replace("<D>", originalDate.Day.ToString());
            return SourceFile;*/
        }
    }
}
