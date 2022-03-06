// ReSharper disable InconsistentNaming

using System.Diagnostics;

namespace SEIDR.DemoMap.BaseImplementation
{
    public class DemoMapJobConfiguration
    {
        /*
        public int DemoMapID { get; set; }
        public int JobProfile_JobID { get; set; }
        public string Description { get; set; }*/
        public int SkipLines { get; set; }

        [Conditional("DEBUG")]
        public void SetDoAPB(bool doAPB)
        {
            DoAPB = doAPB;
        }

        public bool DoAPB { get; private set; }

        [Conditional("DEBUG")]
        public void SetOutputFolder(string folder)
        {
            OutputFolder = folder;
        }

        public string OutputFolder { get; private set; }


        public int? FilePageSize { get; set; }

        public int FileMapID { get; set; }
        public int FileMapDatabaseID { get; set; }
        public int PayerLookupDatabaseID { get; set; } //Change to indicate StagingDatabaseID?

        [Conditional("DEBUG")]
        public void SetEnable_OOO(bool value)
        {
            Enable_OOO = value;
        }

        public bool Enable_OOO { get; private set; } = true;

        [Conditional("DEBUG")]
        public void SetDelimiter(char delim)
        {
            Delimiter = delim;
        }

        public char Delimiter { get; private set; } = '|';

        [Conditional("DEBUG")]
        public void SetOutputDelimiter(char outputDelim)
        {
            OutputDelimiter = outputDelim;
        }

        public char OutputDelimiter { get; private set; } = '|';

        [Conditional("DEBUG")]
        public void Set_PatientBalanceUnavailable(bool avail)
        {
            _PatientBalanceUnavailable = avail;
        }

        public bool _PatientBalanceUnavailable { get; private set; } = false;

        [Conditional("DEBUG")]
        public void Set_InsuranceBalanceUnavailable(bool avail)
        {
            _InsuranceBalanceUnavailable = avail;
        }

        public bool _InsuranceBalanceUnavailable { get; private set; } = false;

        [Conditional("DEBUG")]
        public void Set_InsuranceDetailUnavailable(bool avail)
        {
            _InsuranceDetailUnavailable = avail;
        }

        public bool _InsuranceDetailUnavailable { get; private set; } = false;

        [Conditional("DEBUG")]
        public void Set_PartialDemographicLoad(bool avail)
        {
            _PartialDemographicLoad = avail;
        }

        public bool _PartialDemographicLoad { get; private set; } = false;


        public bool OOO_InsuranceBalanceValidation { get; private set; } = false;

        public bool HasHeaderRow { get; set; } = true;
    }
}
