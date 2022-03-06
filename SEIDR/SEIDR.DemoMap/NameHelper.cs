using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.Doc;

namespace SEIDR.DemoMap
{
    /// <summary>
    /// Use variations of some extraction logic that was put together in Metrix for sending out EDI276 requests.
    /// <para>However, it's a translation from SQL, so the logic still looks very different if you look at the original functions.</para>
    /// </summary>
    public class NameHelper
    {

        /// <summary>
        /// Specify whether to use Patient or Guarantor Name columns from the file.
        /// </summary>
        public bool Guarantor { get; } = true;
        public bool Patient => !Guarantor;
        public NameHelper(string LastName, string FirstName, string Middle)
        {
            LastNameOriginal = LastName.NullifyEmpty();
            FirstNameOriginal = FirstName.NullifyEmpty();
            MiddleOriginal = Middle.NullifyEmpty();

            _commaIdx = -1;
            if (LastNameOriginal == null)
            {
                ComputedName = null;
            }
            else if (FirstNameOriginal == null && MiddleOriginal == null)
                ComputedName = LastNameOriginal;
            else if (FirstNameOriginal == null)
                ComputedName = LastNameOriginal + MiddleOriginal;
            else
            {
                if (MiddleOriginal == null)
                    ComputedName = LastNameOriginal + ", " + FirstNameOriginal;
                else
                    ComputedName = LastNameOriginal + ", " + FirstNameOriginal + " " + MiddleOriginal;
                _commaIdx = LastNameOriginal.Length;
            }
        }

        public NameHelper(DocRecord source, bool guarantor = true)
        {
            Guarantor = guarantor;
            string prefix = Guarantor ? nameof(Guarantor) : nameof(Patient);
            LastNameOriginal = source[prefix + LAST_NAME].NullifyEmpty();
            FirstNameOriginal = source[prefix + FIRST_NAME].NullifyEmpty();
            MiddleOriginal = source[prefix + MIDDLE_INITIAL].NullifyEmpty();

            _commaIdx = -1;
            if (LastNameOriginal == null)
            {
                ComputedName = null;
            }
            else if (FirstNameOriginal == null && MiddleOriginal == null)
                ComputedName = LastNameOriginal;
            else if (FirstNameOriginal == null)
                ComputedName = LastNameOriginal + MiddleOriginal;
            else
            {
                if (MiddleOriginal == null)
                    ComputedName = LastNameOriginal + ", " + FirstNameOriginal;
                else
                    ComputedName = LastNameOriginal + ", " + FirstNameOriginal + " " + MiddleOriginal;
                _commaIdx = LastNameOriginal.Length;
            }
            _source = source;
        }

        private bool checkValidUpdateMode(NameHelperUpdateMode invalidCombo, NameHelperUpdateMode check)
        {
            var an = invalidCombo & check;
            return Enum.TryParse(((int) an).ToString(), out an);
        }
        private readonly DocRecord _source;

        public void SetValues(NameHelperUpdateMode updateMode)
        {
            if (updateMode == NameHelperUpdateMode.None)
                return;
            if(checkValidUpdateMode(NameHelperUpdateMode.GarbageMiddle, updateMode))
                throw new InvalidOperationException("Update Mode overlaps with Garbage Middle Name configuration.");
            string prefix = Guarantor ? nameof(Guarantor) : nameof(Patient);
            if (updateMode.HasFlag(NameHelperUpdateMode.LastName))
                _source[prefix + LAST_NAME] = ExtractLastName();
            if (updateMode.HasFlag(NameHelperUpdateMode.FirstName))
                _source[prefix + FIRST_NAME] = ExtractFirstName();
            if (updateMode.HasFlag(NameHelperUpdateMode.MI))
                _source[prefix + MI] = ExtractMI();
            else if (updateMode.HasFlag(NameHelperUpdateMode.MiddleName))
                _source[prefix + MI] = ExtractMiddleName();
            else if (updateMode.HasFlag(NameHelperUpdateMode.MI_STRICT))
                _source[prefix + MI] = ExtractMI(true);

        }
        public bool IsEmpty => string.IsNullOrEmpty(ComputedName);
        public string ComputedName { get; }
        public string LastNameOriginal { get; }
        public string MiddleOriginal { get; }
        public string FirstNameOriginal { get; }

        const string LAST_NAME = "LastName";
        const string MIDDLE_INITIAL = "MI";
        const string FIRST_NAME = "FirstName";
        private readonly int _commaIdx;

        public string LastName => ExtractLastName();
        public string FirstName => ExtractFirstName();
        public string MI => MiddleOriginal ?? ExtractMI();

        /// <summary>
        /// Attempts to extra first name. If the ComputedName is null, you may want to just use the raw FirstName (trimmed value also available in <see cref="FirstNameOriginal"/> )
        /// <para>Logic is based on Andromeda.APP.ufn_ExtractFirstNameForEDI</para>
        /// </summary>
        /// <returns></returns>
        public string ExtractFirstName()
        {
            if (ComputedName == null)
                return ComputedName;
            int semiColon, commaCheck, idx;
            string recomputed;
            if (_commaIdx < 0)
            {
                semiColon = ComputedName.IndexOf(';');
                if (semiColon < 0)
                {
                    idx = ComputedName.IndexOf(' ');
                    if (idx > 0)
                        return ComputedName.Substring(0, idx).NullifyEmpty();
                    return null;
                }

                recomputed = ComputedName.Substring(semiColon + 1).Trim();
                idx = recomputed.IndexOf(' ');
                commaCheck = recomputed.IndexOf(',');
                if (idx < 0)
                    idx = recomputed.Length;
                if (commaCheck >= 0 && commaCheck < idx)
                    idx = commaCheck;
                semiColon = recomputed.IndexOf(';');
                if (semiColon >= 0 && semiColon < idx)
                    idx = semiColon - 1;
                return recomputed.Substring(0, idx).Trim();
            }

            recomputed = ComputedName.Substring(_commaIdx + 1).Trim();
            idx = recomputed.IndexOf(' ');
            commaCheck = recomputed.IndexOf(',');
            if (idx < 0)
                idx = recomputed.Length;
            if (commaCheck >= 0 && commaCheck < idx)
                idx = commaCheck;

            semiColon = recomputed.IndexOf(';');
            if (semiColon > 0 && semiColon < idx)
                idx = semiColon - 1;

            return recomputed.Substring(0, idx);
        }

        /// <summary>
        /// Attempts to extract last name. Logic is based on Andromeda.APP.ufn_ExtractLastNameForEDI
        /// </summary>
        /// <returns></returns>
        public string ExtractLastName()
        {
            int idx;
            if (_commaIdx > 0)
                return ComputedName.Substring(0, _commaIdx);
            idx = ComputedName.IndexOf(';');
            if (idx > 0)
                return ComputedName.Substring(1, idx);
            idx = ComputedName.LastIndexOf(' ');
            if (idx > 0)
            {
                //var working = new string (ComputedName.Reverse().ToArray());
                return ComputedName.Substring(idx + 1).Trim();
            }

            return ComputedName;
        }

        public string ExtractMiddleName()
        {
            string first = ExtractFirstName();
            if (first == null)
                return null;
            int idx;
            string reversed;
            if (FirstNameOriginal.nLength() > first.Length)
            {
                string last = ExtractLastName();
                int idx2 = FirstNameOriginal.IndexOf(last, StringComparison.Ordinal);
                if (idx2 > 0)
                {
                    string test = FirstNameOriginal.Substring(0, idx2);
                    idx = test.IndexOf(' '); 
                    if (idx > 0)
                    {
                        test = test.Substring(idx + 1).Trim();
                        if (test.IndexOf(' ') > 0)
                            return null; //Multiple middle names?
                        return test; //If not multiple middle names, then return the first letter.
                    }
                    return null; //FirstName LastName
                }
                //First name original is longer than the extracted first - middle is probably inside of it.
                idx = FirstNameOriginal.IndexOf(' ');
                if (idx > 0)
                {
                    string test = FirstNameOriginal.Substring(idx + 1).Trim();
                    idx = test.IndexOf(' ');
                    if (idx >= 0)
                        return null;
                    return test;
                }
                return null;
            }

            if (LastNameOriginal == ComputedName
                || LastNameOriginal.IndexOf(first, StringComparison.Ordinal) >= 0)
            {
                //LastName contained the first name somewhere. So Middle is either in the last name or appended from Middle
                reversed = new string(ComputedName.Trim().Reverse().ToArray());
                idx = reversed.IndexOf(' ');
                if (idx >= 0)
                {
                    string test = reversed.Substring(idx + 1);
                    string firstReverse = new string(first.Reverse().ToArray());
                    idx = test.IndexOf(' '); //Test for a third name.
                                             // (reversed) First Name should be right after the first space of the reversed computed name
                    if (test.IndexOf(firstReverse, idx, StringComparison.Ordinal) == 1 + idx)
                        return new string(test.Substring(0, idx).Reverse().ToArray());
                }
            }
            else
            {
                reversed = new string(ComputedName.Trim().Reverse().ToArray());
            }

            string revFirst = new string((first + ' ').Reverse().ToArray());
            idx = reversed.IndexOf(revFirst, StringComparison.Ordinal);

            return idx < 1 
                       ? null //No space, or just 'FirstName LastName'
                       : new string(reversed.Substring(0, idx).Reverse().ToArray());
        }
       
        /// <summary>
        /// Parses out the middle initial. If strict will only return the initial if the initial is by itself already (otherwise, don't trust)
        /// <para>If strict is false, will simply call <see cref="ExtractMiddleName"/> and return the first character if there is any.</para>
        /// </summary>
        /// <param name="strict"></param>
        /// <returns></returns>
        public string ExtractMI(bool strict = false)
        {
            if (!strict)
            {
                string temp = ExtractMiddleName();
                return temp?.Substring(0, 1);
            }
            string first = ExtractFirstName();
            if (first == null)
                return null;
            int idx;
            string reversed;
            if (FirstNameOriginal.nLength() > first.Length)
            {
                string last = ExtractLastName();
                int idx2 = FirstNameOriginal.IndexOf(last, StringComparison.Ordinal);
                if (idx2 > 0)
                {
                    string test = FirstNameOriginal.Substring(0, idx2);
                    idx = test.IndexOf(' ');
                    if (idx > 0)
                    {
                        test = test.Substring(idx + 1).Trim();

                        if (test.IndexOf(' ') > 0)
                            return null;
                        return test[0].ToString(); //If not multiple middle names, then return the first letter.
                    }
                    return null;
                }
                //First name original is longer than the extracted first - middle is probably inside of it.
                idx = FirstNameOriginal.IndexOf(' ');
                if (idx > 0)
                {
                    string test = FirstNameOriginal.Substring(idx + 1).Trim();
                    if (test.Length == 1) //If end with a single letter, return that.
                        return test;
                    //if (reversed.IndexOf(test, StringComparison.Ordinal) == 0) //Testing for a case like "First M" - "M tsriF".IndexOf("M") should == 0
                    //    return FirstNameOriginal.Substring(idx + 1, 1);
                }
                return null;
            }

            if (LastNameOriginal == ComputedName
                || LastNameOriginal.IndexOf(first, StringComparison.Ordinal) >= 0)
            {
                //LastName contained the first name somewhere. So Middle is either in the last name or appended from Middle
                reversed = new string(ComputedName.Trim().Reverse().ToArray());
                idx = reversed.IndexOf(' ');
                if (idx >= 0)
                {
                    string test = reversed.Substring(idx + 1);
                    string firstReverse = new string(first.Reverse().ToArray()); 
                    idx = test.IndexOf(' '); //Test for a third name.
                                             // (reversed) First Name should be right after the first space of the reversed computed name
                    if (test.IndexOf(firstReverse, idx, StringComparison.Ordinal) == 1 + idx)
                        return test[idx - 1].ToString();
                }
            }
            else
            {
                reversed = new string(ComputedName.Trim().Reverse().ToArray());
            }

            string revFirst = new string((first + ' ').Reverse().ToArray());
            idx = reversed.IndexOf(revFirst, StringComparison.Ordinal);
            if (idx != 1)
                return null; //Check if reversed string is like 'M emaNtsriF, emaNtsaL' - if so, return M. Else return null
            return reversed[0].ToString();
        }
    }
}
