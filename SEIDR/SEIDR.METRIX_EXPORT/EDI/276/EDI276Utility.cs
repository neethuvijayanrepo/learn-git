using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cymetrix.Andromeda.ClaimStatus;

namespace SEIDR.METRIX_EXPORT.EDI
{
    public static class EDI276Utility
    {

        private static string CleanUpUnknownClaimAmount(string claimAmount)
        {//Per TransUnion, may get better results with just '0' instead of '0.00'
            string clAmt = claimAmount;
            if (claimAmount.Equals("0.00") || claimAmount.Equals("0.0"))
            {
                clAmt = "0";
            }
            return clAmt;

        }
        public static string GenerateEDI276String(DataSet ds, 
            string[] nodesToRemove, ExportBatchModel batch, List<string> exportedAccountIDList)
        {
            if (ds.Tables.Count == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            string batchID = batch.ExportBatchID.ToString();
            int counter = 0;

            DataTable dt276 = ds.Tables[0];

            foreach (DataRow row in dt276.Rows)
            {

                EDI_276 working276 = new EDI_276(row["TestOrProduction"].ToString() == "T");
                working276.InitISAProperties(row["ISA01"].ToString(), row["ISA02"].ToString(), row["ISA03"].ToString(),
                    row["ISA04"].ToString(), row["ISA05"].ToString(), row["ISA06"].ToString(), row["ISA07"].ToString(),
                    row["ISA08"].ToString(), batchID + row["RowNum"].ToString().PadLeft(4, '0'));//counter.ToString().PadLeft(4,'0'));


                working276.InitGSProperties(row["GS02"].ToString(), row["GS03"].ToString());
                working276.SetTransactionSetControlNumber((batchID + counter.ToString()).PadLeft(4, '0'));

                working276.SetPayerName(row["OrganizationName"].ToString(), row["PayerID"].ToString());

                working276.SetInfoReceiver(row["NM103_2100B_InfoReceiverName"].ToString(), row["NM109_2100C_NPI"].ToString(), row["NM109_2100C_FedTaxID"].ToString());//Loop 2100B
                working276.SetFacilityInfo(row["NM109_2100C_NPI"].ToString(), row["FacilityName"].ToString());//Loop 2100C

                bool patientIsSubscriber = (bool)row["PatientIsSubscriber"];
                string bDate = row["SubscriberBirthDate"] == DBNull.Value ? String.Empty : row["SubscriberBirthDate"].ToString();
                string gender = row["SubscriberGenderCode"] == DBNull.Value ? String.Empty : row["SubscriberGenderCode"].ToString(); //ARB GenderCode
                if (patientIsSubscriber)
                {
                    bDate = row["PatientBirthdate"].ToString();
                    gender = row["PatientGender"].ToString();
                }

                working276.SetSubscriberInfo(bDate, gender, row["SubscriberLastName"].ToString(), row["SubscriberFirstName"].ToString(),
                    row["SubscriberMiddleName"].ToString(), row["CertificateNumber"].ToString());


                //working276.SetClaimStatusTrackingNumber(row["TRN02"].ToString());
                //working276.SetLoop2200D(row["TRN02"].ToString(), row["MedicalRecordNumber"].ToString(), row["ClaimAmount"].ToString(),
                //    row["AdmissionDate"].ToString(), row["DischargeDate"].ToString());


                working276.SetLoop2200D(row["TRN02"].ToString(), row["SUB_ID"].ToString(), CleanUpUnknownClaimAmount(row["ClaimAmount"].ToString()),
                    row["AdmissionDate"].ToString(), row["DischargeDate"].ToString(), row["ClaimNumber"].ToString(), row["AccountNumber"].ToString());

                /*
				{
				working276.SetDependentInfo(row["PatientBirthdate"].ToString(), row["PatientGender"].ToString(), row["PatientLastName"].ToString()
					, row["PatientFirstName"].ToString(), row["PatientMiddle"].ToString());
				}
				 * */
                if (!patientIsSubscriber)
                //{
                //    working276.RemoveDependentInfo();
                //}
                //else
                {
                    working276.SetDependentInfo(row["DependentBirthdate"] == DBNull.Value ? String.Empty : row["DependentBirthdate"].ToString()
                        , row["DependentGender"].ToString(), row["DependentLastName"].ToString()
                        , row["DependentFirstName"].ToString(), row["DependentMiddle"].ToString());
                }


                //working276.ReInitDependentProperties();

                exportedAccountIDList.Add(row["AccountID"].ToString());

                builder.Append(working276.ToString(nodesToRemove) + Environment.NewLine);

                counter++;
            }
            batch.RecordCount = counter;
            return builder.ToString();
        }
    }
}
