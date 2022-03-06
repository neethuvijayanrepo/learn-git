using System;
using System.Text;
using System.IO;
using SEIDR.METRIX_EXPORT.Utility;

namespace SEIDR.METRIX_EXPORT.LiveVoxExport
{
    class LiveVoxFile
    {
        public TextWriter CreateFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath));
            }

            return File.CreateText(filePath);
        }
        const string LIVEVOX_HEADER = "AccountID,LastName,FirstName,Address,Balance,City,State,ZipCode,Phone1,Phone2,Phone3,Phone4,GuarantorFirstName,GuarantorLastName,PatientBirthDate,PatientAge,AccountNumber,AdmissionOrDischargeDate,ServiceDate,PatientType,ServiceLocation,PhysicianName,AttendingPhysicianName,InsurancePacketBalance,SelfPayPacketBalance,TotalPacketBalance,Insurance1Name,Insurance2Name,Insurance3Name,Insurance1Balance,Insurance2Balance,Insurance3Balance,SelfPaySegmentCategoryID,SelfPayDeterminationDate,NumberofcallsConnected,DateLastMessageLeft,DateofLastConnectedCall,DaysSinceLastConnectedCall,LastPaymentDate,AccountStatusDescription,LastLetterInSeriesSent,LastLetterDate,ARXQuality,EstimatedIncome,ProjectID  \r\n";
        public void WriteExportFileHeader(TextWriter writer)
        {
            writer.Write(LIVEVOX_HEADER);
        }

        public void WriteExportFileRow(TextWriter writer, ExportBatchItem item)
        {

            var row = ComposeRow(item);
            writer.Write(row);

        }
        public string ComposeRow(ExportBatchItem item)
        {

            StringBuilder builder = new StringBuilder();

            builder
                .AppendCsvCell(item.AccountID)
                .CellSeparator()
                .AppendCsvCell(item.LastName)
                .CellSeparator()
                .AppendCsvCell(item.FirstName)
                .CellSeparator()
                .AppendCsvCell(item.Address)
                .CellSeparator()
                .AppendCsvCell(item.Balance)
                .CellSeparator()
                .AppendCsvCell(item.City)
                .CellSeparator()
                .AppendCsvCell(item.State)
                .CellSeparator()
                .AppendCsvCell(item.ZipCode)
                .CellSeparator()
                .AppendCsvCell(item.Phone1)
                .CellSeparator()
                .AppendCsvCell(item.Phone2)
                .CellSeparator()
                .AppendCsvCell(item.Phone3)
                .CellSeparator()
                .AppendCsvCell(item.Phone4)
                .CellSeparator()
                .AppendCsvCell(item.GuarantorLastName)
                .CellSeparator()
                .AppendCsvCell(item.GuarantorFirstName)
                .CellSeparator()
                .AppendCsvCell(item.PatientBirthDate)
                .CellSeparator()
                .AppendCsvCell(item.PatientAge)
                .CellSeparator()
                .AppendCsvCell(item.AccountNumber)
                .CellSeparator()
                .AppendCsvCell(item.AdmissionOrDischargeDate)
                .CellSeparator()
                .AppendCsvCell(item.ServiceDate)
                .CellSeparator()
                .AppendCsvCell(item.PatientType)
                .CellSeparator()
                .AppendCsvCell(item.ServiceLocation)
                .CellSeparator()
                .AppendCsvCell(item.PhysicianName)
                .CellSeparator()
                .AppendCsvCell(item.AttendingPhysicianName)
                .CellSeparator()
                .AppendCsvCell(item.InsurancePacketBalance)
                .CellSeparator()
                .AppendCsvCell(item.SelfPayPacketBalance)
                .CellSeparator()
                .AppendCsvCell(item.TotalPacketBalance)
                .CellSeparator()
                .AppendCsvCell(item.Insurance1Name)
                .CellSeparator()
                .AppendCsvCell(item.Insurance2Name)
                .CellSeparator()
                .AppendCsvCell(item.Insurance3Name)
                .CellSeparator()
                .AppendCsvCell(item.Insurance1Balance)
                .CellSeparator()
                .AppendCsvCell(item.Insurance2Balance)
                .CellSeparator()
                .AppendCsvCell(item.Insurance3Balance)
                .CellSeparator()
                .AppendCsvCell(item.SelfPaySegmentCategoryID)
                .CellSeparator()
                .AppendCsvCell(item.SelfPayDeterminationDate)
                .CellSeparator()
                .AppendCsvCell(item.NumberofcallsConnected)
                .CellSeparator()
                .AppendCsvCell(item.DateLastMessageLeft)
                .CellSeparator()
                .AppendCsvCell(item.DateofLastConnectedCall)
                .CellSeparator()
                .AppendCsvCell(item.DaysSinceLastConnectedCall)
                .CellSeparator()
                .AppendCsvCell(item.LastPaymentDate)
                .CellSeparator()
                .AppendCsvCell(item.AccountStatusDescription)
                .CellSeparator()
                .AppendCsvCell(item.LastLetterInSeriesSent)
                .CellSeparator()
                .AppendCsvCell(item.LastLetterDate)
                .CellSeparator()
                .AppendCsvCell(item.ARXQuality)
                .CellSeparator()
                .AppendCsvCell(item.EstimatedIncome)
                .CellSeparator()
                .AppendCsvCell(item.ProjectID)

                .RowSeparator();

            return builder.ToString();

        }
       
        

    }
}
