using System;

namespace SEIDR.MetrixProcessing.Invoice
{
    public class PreviousPlacementInfo
    {
        public int Encounter_ProjectID { get; private set; }
        public DateTime? PreviousPlacementDate { get; private set; }
        public DateTime? PreviousCancellationDate { get; private set; }
    }
}