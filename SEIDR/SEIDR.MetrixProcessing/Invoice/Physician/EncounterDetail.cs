using System;

namespace SEIDR.MetrixProcessing.Invoice.Physician
{
    public class EncounterDetail
    {
        public EncounterDetail(System.Data.IDataRecord record)
        {
            AccountNumber = (string) record[nameof(AccountNumber)];
            EncounterNumber = (string) record[nameof(EncounterNumber)];
            TotalCharges = (decimal) record[nameof(TotalCharges)];
            CurrentEncounterBalance = (decimal) record[nameof(CurrentEncounterBalance)];

            Billed = (decimal) record[nameof(Billed)];
            PreviousPayments = (decimal) record[nameof(PreviousPayments)];

            FeeSum = (decimal) record[nameof(FeeSum)];

            UserSpecifiedAccountNumber = record[nameof(UserSpecifiedAccountNumber)] as string;
            PatientName = record[nameof(PatientName)] as string;
        }
        public string AccountNumber { get; private set; }
        public string UserSpecifiedAccountNumber { get; private set; }
        public string EncounterNumber {get; private set;}
        public string PatientName { get; private set; }
        public decimal CurrentEncounterBalance { get; private set; }
        public decimal TotalCharges { get; private set; }
        public decimal Billed { get; private set; }
        public decimal PreviousPayments { get; private set; }
        public decimal FeeSum { get; private set; }
    }
}