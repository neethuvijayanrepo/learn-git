using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.LiveVoxExport
{
    public class ExportBatchItem
    {

        public string AccountID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone4 { get; set; }
        public string GuarantorFirstName { get; set; }
        public string GuarantorLastName { get; set; }
        public DateTime? PatientBirthDate { get; set; }
        public int? PatientAge { get; set; }
        public string AccountNumber { get; set; }
        public DateTime? AdmissionOrDischargeDate { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string PatientType { get; set; }
        public string ServiceLocation { get; set; }
        public string PhysicianName { get; set; }
        public string AttendingPhysicianName { get; set; }
        public decimal? InsurancePacketBalance { get; set; }
        public decimal? SelfPayPacketBalance { get; set; }
        public decimal? TotalPacketBalance { get; set; }
        public string Insurance1Name { get; set; }
        public string Insurance2Name { get; set; }
        public string Insurance3Name { get; set; }
        public decimal? Insurance1Balance { get; set; }
        public decimal? Insurance2Balance { get; set; }
        public decimal? Insurance3Balance { get; set; }
        public int? SelfPaySegmentCategoryID { get; set; }
        public DateTime? SelfPayDeterminationDate { get; set; }
        public int? NumberofcallsConnected { get; set; }
        public DateTime? DateLastMessageLeft { get; set; }
        public DateTime? DateofLastConnectedCall { get; set; }
        public int? DaysSinceLastConnectedCall { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string AccountStatusDescription { get; set; }
        public int? LastLetterInSeriesSent { get; set; }
        public DateTime? LastLetterDate { get; set; }
        public string ARXQuality { get; set; }
        public int? EstimatedIncome { get; set; }
        public int? ProjectID { get; set; }
    }
}
