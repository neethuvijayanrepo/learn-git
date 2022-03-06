using System;

namespace SEIDR.DemoMap
{
    [Flags]
    public enum NameHelperUpdateMode
    {
        None = 0,
        LastName = 1,
        FirstName = 2,
        MI = 4,
        MI_STRICT = 8,
        MiddleName = 16,
        /// <summary>
        /// Invalid combination.
        /// </summary>
        GarbageMiddle = MI | MiddleName | MI_STRICT,
        LastFirst = LastName | FirstName,
        Default = LastFirst | MI,
        DefaultStrict = LastFirst | MI_STRICT,
        LastFirstMiddle = LastFirst | MiddleName
    }
}
